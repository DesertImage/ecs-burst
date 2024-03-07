using DesertImage.ECS;
using NUnit.Framework;
using Unity.Mathematics;

namespace Game
{
    public struct ParentToOriginPositionSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                // .With<Parent>()
                .With<OriginPosition>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            // var parents = _group.GetComponents<Parent>();
            var originPositions = _group.GetComponents<OriginPosition>();

            var count = _group.Count;
            for (var i = count - 1; i >= 0; i--)
            {
                // var parent = parents[i].Value;
                // if (!parent.Value) continue;
                // originPositions.Get(i).Value = parent.Value.position * (i + 1);
                originPositions.Get(i).Value = new float3(2f);
                // Assert.AreEqual(originPositions[i].Value, _group.GetEntity(i).Read<OriginPosition>().Value);
            }
        }
    }
}