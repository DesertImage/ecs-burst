using DesertImage.ECS;
using NUnit.Framework;

namespace DesertImage.Collections
{
    public unsafe class AllocatorTests
    {
        [Test]
        public void Allocate()
        {
            var intSize = MemoryUtility.SizeOf<int>();
            var allocator = new MemoryAllocator(intSize);

            allocator.Allocate(intSize);
            allocator.Dispose();
        }

        [Test]
        public void Set()
        {
            var intSize = MemoryUtility.SizeOf<int>();
            var allocator = new MemoryAllocator(intSize);
            var ptr = allocator.Allocate(intSize);

            ptr.Set(2, allocator);

            allocator.Dispose();
        }

        [Test]
        public void Read()
        {
            const int expected = 2;

            var intSize = MemoryUtility.SizeOf<int>();
            var allocator = new MemoryAllocator(intSize);
            var ptr = allocator.Allocate(intSize);

            ptr.Set(expected, allocator);

            var result = ptr.Read<int>(allocator);

            allocator.Dispose();

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Free()
        {
            var intSize = MemoryUtility.SizeOf<int>();
            var allocator = new MemoryAllocator(intSize);
            var ptr = allocator.Allocate(intSize);

            ptr.Set(2, allocator);

            allocator.Free(ptr);
            allocator.Dispose();
        }

        [Test]
        public void Resize()
        {
            var intSize = MemoryUtility.SizeOf<int>();
            var allocator = new MemoryAllocator(intSize);

            const int ptrCount = 6;

            var ptrs = new Ptr[ptrCount];
            var results = new int[ptrCount];

            for (var i = 0; i < ptrCount; i++)
            {
                var ptr = allocator.Allocate(intSize);
                ptr.Set(i + 1, allocator);
                ptrs[i] = ptr;
            }


            for (var i = 0; i < ptrCount; i++)
            {
                results[i] = ptrs[i].Read<int>(allocator);
            }

            allocator.Dispose();

            for (var i = 0; i < ptrCount; i++)
            {
                Assert.AreEqual(i + 1, results[i]);
            }
        }

        [Test]
        public void Split()
        {
            const int fullSize = 5;
            const int firstSize = 2;

            var allocator = new MemoryAllocator(fullSize);

            allocator.Allocate(firstSize);

            var firstBlockSize = allocator.First->Size;
            var lastBlockSize = allocator.Last->Size;

            allocator.Dispose();

            Assert.AreEqual(firstSize, firstBlockSize);
            Assert.AreEqual(fullSize - firstSize, lastBlockSize);
        }

        [Test]
        public void DoubleSplit()
        {
            const int fullSize = 5;
            const int firstSize = 2;
            const int secondSize = 1;

            var allocator = new MemoryAllocator(fullSize);

            allocator.Allocate(firstSize);
            allocator.Allocate(secondSize);

            var firstBlockSize = allocator.First->Size;
            var secondBlockSize = allocator.First->Next->Size;
            var lastBlockSize = allocator.Last->Size;

            allocator.Dispose();

            Assert.AreEqual(firstSize, firstBlockSize);
            Assert.AreEqual(secondSize, secondBlockSize);
            Assert.AreEqual(fullSize - firstSize - secondSize, lastBlockSize);
        }

        [Test]
        public void CombinePrevious()
        {
            const int fullSize = 5;
            const int firstSize = 2;
            const int secondSize = 1;

            var allocator = new MemoryAllocator(fullSize);

            var first = allocator.Allocate(firstSize);
            var second = allocator.Allocate(secondSize);
            allocator.Allocate(fullSize - firstSize - secondSize);

            allocator.Free(first);
            allocator.Free(second);

            var firstBlockSize = allocator.First->Size;
            var lastBlockSize = allocator.Last->Size;

            allocator.Dispose();

            Assert.AreEqual(firstSize + secondSize, firstBlockSize);
            Assert.AreEqual(fullSize - (firstSize + secondSize), lastBlockSize);
        }

        [Test]
        public void CombineNext()
        {
            const int fullSize = 5;
            const int firstSize = 2;
            const int secondSize = 1;

            var allocator = new MemoryAllocator(fullSize);

            allocator.Allocate(firstSize);
            var second = allocator.Allocate(secondSize);

            allocator.Free(second);

            var firstBlockSize = allocator.First->Size;
            var secondBlockSize = allocator.First->Next->Size;
            var lastBlockSize = allocator.Last->Size;

            allocator.Dispose();

            Assert.AreEqual(firstSize, firstBlockSize);
            Assert.AreEqual(fullSize - firstSize, secondBlockSize);
            Assert.AreEqual(secondBlockSize, lastBlockSize);
        }

        [Test]
        public void CombineBoth()
        {
            const int fullSize = 10;
            const int firstSize = 2;
            const int secondSize = 1;
            const int thirdSize = 3;
            const int fourthSize = 3;
            const int fifthSize = 1;

            var allocator = new MemoryAllocator(fullSize);

            allocator.Allocate(firstSize);
            var second = allocator.Allocate(secondSize);
            var third = allocator.Allocate(thirdSize);
            var fourth = allocator.Allocate(fourthSize);
            allocator.Allocate(fifthSize);

            allocator.Free(second);
            allocator.Free(fourth);
            allocator.Free(third);

            var firstBlockSize = allocator.First->Size;
            var secondBlockSize = allocator.First->Next->Size;
            var lastBlockSize = allocator.Last->Size;

            allocator.Dispose();

            Assert.AreEqual(firstSize, firstBlockSize);
            Assert.AreEqual(secondSize + fourthSize + thirdSize, secondBlockSize);
            Assert.AreEqual(fifthSize, lastBlockSize);
        }

        [Test]
        public void WorldResize()
        {
            var world = Worlds.Create();
            var entity = world.GetNewEntity();

            const int ptrCount = 9;

            var bufferLists = new BufferList<int>[ptrCount];
            var results = new int[ptrCount];

            for (var i = 0; i < ptrCount; i++)
            {
                var list = entity.CreateBufferList<int>();
                list.Add(i);
                bufferLists[i] = list;
            }

            for (var i = 0; i < ptrCount; i++)
            {
                results[i] = bufferLists[i][0];
            }

            world.Dispose();

            for (var i = 0; i < ptrCount; i++)
            {
                Assert.AreEqual(i, results[i]);
            }
        }
    }
}