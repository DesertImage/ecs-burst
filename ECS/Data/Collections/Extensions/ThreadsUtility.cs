namespace DesertImage.ECS
{
    public static class ThreadsUtility
    {
        public static unsafe void LockIndex(ref int lockIndex)
        {
            while (0 != System.Threading.Interlocked.CompareExchange(ref lockIndex, 1, 0))
            {
                Unity.Burst.Intrinsics.Common.Pause();
            }

            System.Threading.Interlocked.MemoryBarrier();
        }

        public static unsafe void Lock(this ref int lockIndex)
        {
            while (0 != System.Threading.Interlocked.CompareExchange(ref lockIndex, 1, 0))
            {
                Unity.Burst.Intrinsics.Common.Pause();
            }

            System.Threading.Interlocked.MemoryBarrier();
        }

        public static unsafe void UnlockIndex(ref int lockIndex)
        {
            System.Threading.Interlocked.MemoryBarrier();

            while (1 != System.Threading.Interlocked.CompareExchange(ref lockIndex, 0, 1))
            {
                Unity.Burst.Intrinsics.Common.Pause();
            }

        }

        public static unsafe void Unlock(this ref int lockIndex)
        {
            System.Threading.Interlocked.MemoryBarrier();

            while (1 != System.Threading.Interlocked.CompareExchange(ref lockIndex, 0, 1))
            {
                Unity.Burst.Intrinsics.Common.Pause();
            }
        }
    }
}