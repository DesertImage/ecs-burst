using System;
using UnityEngine;

namespace DesertImage.Collections
{
    public struct UintEntry : IEquatable<UintEntry>
    {
        public uint Value;
        public Vector2Int Position;

        public bool Equals(UintEntry other) => Value == other.Value && Position == other.Position;
    }
}