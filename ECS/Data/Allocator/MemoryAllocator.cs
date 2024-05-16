using System;
using System.Diagnostics;

namespace DesertImage.ECS
{
    [DebuggerDisplay("Buffer size = {_bufferSize}")]
    [DebuggerTypeProxy(typeof(MemoryAllocatorDebugView))]
    public unsafe struct MemoryAllocator : IDisposable
    {
        private byte* _buffer;
        internal long _bufferSize;

        //use array or list
        public MemoryBlock** Blocks;
        public MemoryBlock* First;
        public MemoryBlock* Last;

        private int* _freeIds;
        private int _freeIdsCapacity;
        private int _freeIdsCount;

        internal int _blocksCapacity;
        private int _idCounter;

        public MemoryAllocator(long bufferSize)
        {
            _buffer = MemoryUtility.AllocateClear<byte>(bufferSize);
            _bufferSize = bufferSize;

            _blocksCapacity = 5;

            Blocks = (MemoryBlock**)MemoryUtility.Allocate<IntPtr>(IntPtr.Size * _blocksCapacity);
            for (var i = 0; i < _blocksCapacity; i++)
            {
                Blocks[i] = null;
            }

            _idCounter = 0;
            First = default;
            Last = default;

            _freeIdsCapacity = 5;
            _freeIds = MemoryUtility.AllocateClearCapacity<int>(_freeIdsCapacity);
            _freeIdsCount = 0;

            AddBlock
            (
                new MemoryBlock
                {
                    IsFree = true,
                    Size = bufferSize,
                    Offset = 0
                }
            );
        }

        public readonly T* GetPtr<T>(in Ptr ptr) where T : unmanaged
        {
#if DEBUG_MODE
            if (ptr.Id >= _blocksCapacity) throw new IndexOutOfRangeException();
#endif
            var block = Blocks[ptr.Id];
            return (T*)(_buffer + block->Offset);
        }

        public readonly void* GetPtr(in Ptr ptr)
        {
#if DEBUG_MODE
            if (ptr.Id >= _blocksCapacity) throw new IndexOutOfRangeException();
#endif
            var block = Blocks[ptr.Id];
            return _buffer + block->Offset;
        }

        public Ptr Allocate(long size, bool dontClear = false)
        {
            if (!TryGetFreeBlock(out var freeBlock, size))
            {
                freeBlock = ResizeBuffer(_bufferSize << 1, _bufferSize + size);
            }

            if (freeBlock->Size == size)
            {
                freeBlock->IsFree = false;
                return freeBlock->AsPtr(_buffer);
            }

            var newBlock = AddBlock
            (
                new MemoryBlock
                {
                    IsFree = false,
                    Offset = freeBlock->Offset,
                    Size = size,
                    Previous = freeBlock->Previous,
                    Next = freeBlock
                }
            );

            if (freeBlock->Previous != null)
            {
                freeBlock->Previous->Next = newBlock;
            }

            freeBlock->Offset += (int)size;
            freeBlock->Size -= size;
            freeBlock->Previous = newBlock;

            if (freeBlock == First)
            {
                First = newBlock;
            }

            var ptr = newBlock->AsPtr(_buffer);

            if (!dontClear)
            {
                ClearMemory(ptr);
            }

            return ptr;
        }

        public void Free(Ptr ptr)
        {
            var block = Blocks[ptr.Id];
            block->IsFree = true;

            CombineWithNext(block);
            CombineWithPrevious(block);
        }

        public void Copy(Ptr destination, Ptr origin)
        {
            var block = Blocks[origin.Id];
            MemoryUtility.Copy(GetPtr(destination), GetPtr(origin), block->Size);
        }

        public void Resize(ref Ptr ptr, long newSize)
        {
            var newPtr = Allocate(newSize);
            Copy(newPtr, ptr);

            var temp = new Ptr
            {
                Id = ptr.Id,
                // Value = ptr.Value,
#if DEBUG_MODE
                Size = ptr.Size
#endif
            };

            Free(temp);

            ptr.Id = newPtr.Id;
            // ptr.Value = newPtr.Value;
#if DEBUG_MODE
            ptr.Size = newPtr.Size;
#endif
        }

        public void ClearMemory(Ptr ptr)
        {
            MemoryUtility.Clear(GetPtr(ptr), Blocks[ptr.Id]->Size);
        }

        private MemoryBlock* CombineWithPrevious(MemoryBlock* block)
        {
            while (block->Previous != null && block->Previous->IsFree)
            {
                var previous = block->Previous;

                previous->Next = block->Next;
                previous->Size += block->Size;

                if (block->Next != null)
                {
                    block->Next->Previous = previous;
                }

                CacheFreeId(block->Id);
                *block = default;

                block = previous;
            }

            return block;
        }

        private MemoryBlock* CombineWithNext(MemoryBlock* block)
        {
            while (block->Next != null && block->Next->IsFree)
            {
                var next = block->Next;

                block->Next = next->Next;
                block->Size += next->Size;

                CacheFreeId(next->Id);

                if (block->Next == null)
                {
                    Last = block;
                    break;
                }

                block->Next->Previous = block;

                *next = default;

                block = next;
            }

            return block;
        }

        private bool TryGetFreeBlock(out MemoryBlock* block, long minSize)
        {
            block = First;

            if ((IntPtr)block == IntPtr.Zero) return false;
            if (IsValidFree(block, minSize)) return true;

            while (block != null && (block->Next != null || block->Size >= minSize))
            {
                if (IsValidFree(block, minSize)) return true;
                block = block->Next;
            }

            return false;
        }

        private static bool IsValidFree(MemoryBlock* block, long minSize)
        {
            return block->IsFree && block->Size >= minSize;
        }

        private MemoryBlock* AddBlock(MemoryBlock memoryBlock)
        {
            var id = GetNextId();

            if (id >= _blocksCapacity) ResizeBlocks(_blocksCapacity << 1);

            memoryBlock.Id = id;

            var ptr = Blocks[id];
            if (ptr == null)
            {
                ptr = Blocks[id] = MemoryUtility.AllocateInstance(memoryBlock);
            }
            else
            {
                *ptr = memoryBlock;
            }

            if (id == 0)
            {
                First = ptr;
                Last = ptr;
            }

            return ptr;
        }

        private int GetNextId()
        {
            if (_freeIdsCount == 0) return _idCounter++;

            var id = _freeIds[_freeIdsCount];

            _freeIdsCount--;

            return id;
        }

        private void CacheFreeId(int id)
        {
            if (_freeIdsCount >= _freeIdsCapacity) ResizeIds(_freeIdsCapacity << 1);
            _freeIds[_freeIdsCount] = id;
            _freeIdsCount++;
        }

        private void ResizeBlocks(int newCapacity)
        {
            var size = IntPtr.Size * newCapacity;
            var newPtr = (MemoryBlock**)MemoryUtility.Allocate<IntPtr>(size);

            MemoryUtility.Clear(newPtr, size);

            for (var i = 0; i < _blocksCapacity; i++)
            {
                newPtr[i] = Blocks[i];
            }

            MemoryUtility.Free(Blocks);

            Blocks = newPtr;

            _blocksCapacity = newCapacity;
        }

        private MemoryBlock* ResizeBuffer(long newSize, long minSize)
        {
            if (newSize < minSize) newSize = minSize + 1;

            _buffer = MemoryUtility.Resize(_buffer, _bufferSize, newSize);

            var newBlock = AddBlock
            (
                new MemoryBlock
                {
                    IsFree = true,
                    Offset = (int)_bufferSize,
                    Size = newSize - _bufferSize,
                    Previous = Last
                }
            );

            Last = newBlock;

            _bufferSize = newSize;

            return newBlock;
        }

        private void ResizeIds(int newCapacity)
        {
            _freeIds = MemoryUtility.Resize(_freeIds, _freeIdsCapacity, newCapacity);
            _freeIdsCapacity = newCapacity;
        }

        public void Dispose()
        {
            for (var i = 0; i < _blocksCapacity; i++)
            {
                var blockPtr = Blocks[i];
                if (blockPtr == null) continue;
                MemoryUtility.Free(blockPtr);
            }

            MemoryUtility.Free(Blocks);
            MemoryUtility.Free(_buffer);
            MemoryUtility.Free(_freeIds);
        }
    }

    internal sealed unsafe class MemoryAllocatorDebugView
    {
        private MemoryAllocator _data;

        public MemoryAllocatorDebugView(MemoryAllocator data) => _data = data;

        public long BufferSize => _data._bufferSize;

        public MemoryBlock*[] Blocks => MemoryUtility.ToArray(_data.Blocks, _data._blocksCapacity);
        public int BlocksCapacity => _data._blocksCapacity;

        public MemoryBlock First => *_data.First;
        public MemoryBlock Last => *_data.Last;
    }
}