using DesertImage.Collections;
using UnityEngine.Jobs;

namespace DesertImage.ECS
{
    public struct ViewTransforms
    {
        public TransformAccessArray Values;
        public UnsafeUintSparseSet<int> Indexes;
        // public int Count
    }
}