using DesertImage.ECS;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Vehicle
{
    public struct WheelVisualizeSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<Wheel>()
                .With<Suspension>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var wheels = _group.GetComponents<Wheel>();
            var suspensions = _group.GetComponents<Suspension>();

            foreach (var entityId in _group)
            {
                var wheel = wheels.Read(entityId);
                var suspension = suspensions.Read(entityId);

                var transform = wheel.View.Value;

                var localPosition = transform.localPosition;
                localPosition.y = -1f * (suspension.Height - suspension.Offset - wheel.Radius);
                transform.localPosition = localPosition;

                transform.Rotate(transform.right, wheel.AngularVelocity, Space.World);
            }
        }
    }
}