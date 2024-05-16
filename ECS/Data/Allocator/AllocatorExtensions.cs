namespace DesertImage.ECS
{
    public static class AllocatorExtensions
    {
        public static bool IsFree(this MemoryBlock block) => block.FreeFlag == MemoryBlock.SLOT_FREE;
    }
}