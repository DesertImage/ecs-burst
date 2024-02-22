using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using Unity.Collections;
using Debug = UnityEngine.Debug;

namespace DesertImage.Collections
{
    public class UnsafeDictionaryTests
    {
        [Test]
        public void Add1()
        {
            var data = new UnsafeDictionary<int, int>(20, Allocator.Persistent);

            data.Add(0, 1);

            var result = data.Contains(0);

            data.Dispose();

            Assert.IsTrue(result);
        }

        [Test]
        public void Add1000000()
        {
            const int count = 1_000_000;

            var timer = new Stopwatch();

            var unsafeDictionary = new UnsafeDictionary<int, int>(count + 1, Allocator.Persistent);
            var nativeDictionary = new NativeParallelHashMap<int, int>(count + 1, Allocator.Persistent);
            var dictionary = new Dictionary<int, int>();

            timer.Start();
            for (var i = 0; i < count; i++)
            {
                unsafeDictionary.Add(i, i);
            }

            timer.Stop();

            var unsafeElapsed = timer.Elapsed.TotalMilliseconds;

            timer.Reset();

            timer.Start();
            for (var i = 0; i < count; i++)
            {
                dictionary.Add(i, i);
            }

            timer.Stop();

            var classicElapsed = timer.Elapsed.TotalMilliseconds;

            timer.Reset();

            timer.Start();
            for (var i = 0; i < count; i++)
            {
                nativeDictionary.Add(i, i);
            }

            timer.Stop();

            var nativeElapsed = timer.Elapsed.TotalMilliseconds;

            Debug.Log($"Unsafe: {unsafeElapsed}");
            Debug.Log($"Classic: {classicElapsed}");
            Debug.Log($"Native: {nativeElapsed}");

            unsafeDictionary.Dispose();
            nativeDictionary.Dispose();
        }

        [Test]
        public void Remove()
        {
            var data = new UnsafeDictionary<int, int>(20, Allocator.Persistent);

            data.Add(0, 1);
            data.Remove(0);

            var result = data.Contains(0);

            data.Dispose();

            Assert.IsFalse(result);
        }
    }
}