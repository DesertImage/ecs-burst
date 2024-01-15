using System;

namespace DesertImage
{
    [Serializable]
    public abstract class SparseSetAbstract
    {
        public abstract int Count { get; protected set; }
    }
}