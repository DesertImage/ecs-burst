using System;
using UnityEngine;

namespace DesertImage.Collections
{
    public struct Bounds : IEquatable<Bounds>
    {
        public readonly Vector2Int Center;
        public readonly Vector2Int Size;

        private readonly int _minX;
        private readonly int _maxX;

        private readonly int _minY;
        private readonly int _maxY;

        public Bounds(Vector2Int center, Vector2Int size)
        {
            Center = center;
            Size = size;

            var halfSize = size.x / 2;

            _minX = center.x - halfSize;
            _maxX = center.x + halfSize;

            _minY = center.y - halfSize;
            _maxY = center.y + halfSize;
        }

        public bool Contains(Vector2Int position)
        {
            return position.x >= _minX && position.x <= _maxX && position.y >= _minY && position.y <= _maxY;
        }

        public bool Contains(int x, int y) => x >= _minX && x <= _maxX && y >= _minY && y <= _maxY;

        public bool Intersects(Bounds other)
        {
            return !(_minX > other._maxX || _maxX < other._minX || _minY > other._maxY || _maxY < other._minY);
        }

        public bool Equals(Bounds other)
        {
            return Center.Equals(other.Center) && Size.Equals(other.Size);
        }

        public override bool Equals(object obj) => obj is Bounds other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Center, Size);
    }
}