using System.Threading;
using UnityEngine;

namespace DesertImage.ECS
{
    public static class ThreadsUtility
    {
        public static void LockIndex(ref int lockIndex)
        {
            while (0 != Interlocked.CompareExchange(ref lockIndex, 1, 0))
            {
                Unity.Burst.Intrinsics.Common.Pause();
            }

            Interlocked.MemoryBarrier();
        }

        public static void Lock(this ref int lockIndex)
        {
#if DEBUG
            var triesCount = 0;
#endif

            while (0 != Interlocked.CompareExchange(ref lockIndex, 1, 0))
            {
                Unity.Burst.Intrinsics.Common.Pause();
#if DEBUG
                triesCount++;
                if (triesCount >= 100000000)
                {
                    Debug.LogError($"Infinite lock {Thread.CurrentThread}");
                    break;
                }
#endif
            }

            Interlocked.MemoryBarrier();
        }

        public static void UnlockIndex(ref int lockIndex)
        {
            Interlocked.MemoryBarrier();

            while (1 != Interlocked.CompareExchange(ref lockIndex, 0, 1))
            {
                Unity.Burst.Intrinsics.Common.Pause();
            }
        }

        public static void Unlock(this ref int lockIndex)
        {
            Interlocked.MemoryBarrier();

#if DEBUG
            var triesCount = 0;
#endif
            while (1 != Interlocked.CompareExchange(ref lockIndex, 0, 1))
            {
                Unity.Burst.Intrinsics.Common.Pause();
#if DEBUG
                triesCount++;
                if (triesCount >= 100000000)
                {
                    Debug.LogError($"Infinite unlock {Thread.CurrentThread}");
                    break;
                }
#endif
            }
        }
    }
}