using System;

namespace DesertImage.ECS
{
    [Serializable]
    public struct Entity
    {
        public readonly int Id;

        public Entity(int id) => Id = id;
    }
}