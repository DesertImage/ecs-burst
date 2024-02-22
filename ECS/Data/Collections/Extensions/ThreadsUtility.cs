using System.Threading;

namespace DesertImage.ECS
{
    public static class ThreadsUtility
    {
        public static void Lock(this ref int lockIndex)
        {
#if DEBUG
            long triesCount = 0;
#endif
            while (0 != Interlocked.CompareExchange(ref lockIndex, 1, 0))
            {
                Unity.Burst.Intrinsics.Common.Pause();
#if DEBUG
                triesCount++;
                if (triesCount >= 1000000000)
                {
                    // Debug.LogError($"Infinite lock");
                    break;
                }
#endif
            }

            Interlocked.MemoryBarrier();
        }

        public static void Unlock(this ref int lockIndex)
        {
            Interlocked.MemoryBarrier();

#if DEBUG
            long triesCount = 0;
#endif
            while (1 != Interlocked.CompareExchange(ref lockIndex, 0, 1))
            {
                Unity.Burst.Intrinsics.Common.Pause();
#if DEBUG
                triesCount++;
                if (triesCount >= 1000000000)
                {
                    // Debug.LogError($"Infinite unlock");
                    break;
                }
#endif
            }
        }
    }
}