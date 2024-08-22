using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace DesertImage.Collections
{
    public class QuadTreeTests
    {
        [Test]
        public void Add1()
        {
            const int pointsCapacity = 5;

            var data = new UnsafeUintQuadTree
            (
                new Bounds
                (
                    new Vector2Int(0, 0),
                    new Vector2Int(512, 512)
                ),
                14,
                pointsCapacity
            );

            for (var i = 0; i < pointsCapacity; i++)
            {
                data.Insert((uint)i, new Vector2Int(i, i));
            }

            var firstCount = data.Quads[0].Values.Count;
            var firstChildCount = data.Quads[0]._children.Length;

            for (var i = 5; i < pointsCapacity + 1; i++)
            {
                data.Insert((uint)i, new Vector2Int(i, i));
            }

            // var firstCount = data.Quads[0].Values.Count;
            var secondChildCount = data.Quads[0]._children.Length;
            var topLeftChildCount = data.Quads[data.Quads[0]._children[0]].Values.Count;
            var bottomLeftChildCount = data.Quads[data.Quads[0]._children[1]].Values.Count;
            var topRightChildCount = data.Quads[data.Quads[0]._children[2]].Values.Count;
            var bottomRightChildCount = data.Quads[data.Quads[0]._children[3]].Values.Count;

            data.Insert(7, new Vector2Int(-1, -1));

            var topLeftChildCountSecond = data.Quads[data.Quads[0]._children[0]].Values.Count;
            var bottomLeftChildCountSecond = data.Quads[data.Quads[0]._children[1]].Values.Count;
            var topRightChildCountSecond = data.Quads[data.Quads[0]._children[2]].Values.Count;
            var bottomRightChildCountSecond = data.Quads[data.Quads[0]._children[3]].Values.Count;

            data.Dispose();

            Assert.AreEqual(5, firstCount);
            Assert.AreEqual(0, firstChildCount);

            Assert.AreEqual(4, secondChildCount);

            Assert.AreEqual(1, topLeftChildCount);
            Assert.AreEqual(0, bottomLeftChildCount);
            Assert.AreEqual(4, topRightChildCount);
            Assert.AreEqual(0, bottomRightChildCount);

            Assert.AreEqual(1, topLeftChildCountSecond);
            Assert.AreEqual(1, bottomLeftChildCountSecond);
            Assert.AreEqual(4, topRightChildCountSecond);
            Assert.AreEqual(0, bottomRightChildCountSecond);
        }

        [Test]
        public void PerformanceInsert()
        {
            const int pointsCapacity = 10000;

            var data = new UnsafeUintQuadTree
            (
                new Bounds
                (
                    new Vector2Int(0, 0),
                    new Vector2Int(512, 512)
                ),
                14,
                pointsCapacity
            );

            var timer = new Stopwatch();

            timer.Start();
            for (var i = 0; i < pointsCapacity; i++)
            {
                data.Insert
                (
                    (uint)i,
                    new Vector2Int(Random.Range(-10, 10), Random.Range(-10, 10))
                );
            }

            timer.Stop();

            Debug.Log($"Time: {timer.Elapsed.TotalMilliseconds.ToString()}");

            data.Dispose();
        }
    }
}