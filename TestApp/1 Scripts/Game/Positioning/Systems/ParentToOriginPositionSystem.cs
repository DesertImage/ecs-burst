using DesertImage.Collections;
using DesertImage.ECS;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace Game
{
    public struct ParentToOriginPositionSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<Parent>()
                .With<OriginPosition>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var parents = _group.GetComponents<Parent>();
            var originPositions = _group.GetComponents<OriginPosition>();
            var unsafeArray = new UnsafeArray<float3>(_group.Count, Allocator.Temp);

            var count = _group.Count;
            for (var i = 0; i < count; i++)
            {
                var parent = parents[i].Value;
                if (!parent.Value) continue;
                originPositions.Get(i).Value = parent.Value.position;
            }

            unsafeArray.Dispose();
        }
    }
}