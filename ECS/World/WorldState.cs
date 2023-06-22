using System;
using System.Collections.Generic;

namespace DesertImage.ECS
{
    [Serializable]
    public struct WorldState
    {
        public readonly Dictionary<int, Entity> Entities;
        public readonly Dictionary<int, SortedSetPoolable<int>> Components;

        public WorldState(Dictionary<int, Entity> entities, Dictionary<int, SortedSetPoolable<int>> components)
        {
            Entities = entities;
            Components = components;
        }
    }
}