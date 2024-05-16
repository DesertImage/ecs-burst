namespace DesertImage.ECS
{
    public unsafe struct MemoryBlock
    {
        public const byte SLOT_FREE = 0;
        public const byte SLOT_NOT_FREE = 1;
        
        public int Id;

        // public Ptr Ptr;

        public bool IsFree
        {
            get => FreeFlag == SLOT_FREE;
            set => FreeFlag = value ? SLOT_FREE : SLOT_NOT_FREE;
        }
        
        public byte FreeFlag;

        public int Offset;
        public long Size;

        public MemoryBlock* Previous;
        public MemoryBlock* Next;

        // public void RefreshPtr(byte* buffer) => Ptr = AsPtr(buffer);

#if DEBUG_MODE
        public Ptr AsPtr(byte* buffer) => new Ptr(Id, Size);
#else
        public Ptr AsPtr(byte* buffer) => new Ptr(Id);
#endif

    }
}