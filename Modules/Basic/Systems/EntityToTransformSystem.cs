using UnityEngine;

namespace DesertImage.ECS
{
    public struct EntityToTransformSystem : IExecuteSystem
    {
        public Matcher Matcher => MatcherBuilder.Create()
            .With<View>()
            .With<Position>()
            .With<Rotation>()
            .With<Scale>()
            .Build();

        public void Execute(Entity entity, World world, float deltaTime)
        {
            var view = entity.Read<View>().Value.Value;
            var transform = view.transform;

            transform.position = entity.Read<Position>().Value;
            transform.rotation = Quaternion.Euler(entity.Read<Rotation>().Value);
            transform.localScale = entity.Read<Scale>().Value;
        }
    }
}