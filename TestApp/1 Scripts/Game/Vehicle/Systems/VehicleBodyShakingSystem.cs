using DesertImage.ECS;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Vehicle
{
    public struct VehicleBodyShakingSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<Vehicle>()
                .With<Engine>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var vehicles = _group.GetComponents<Vehicle>();
            var engines = _group.GetComponents<Engine>();

            foreach (var entityId in _group)
            {
                ref var vehicle = ref vehicles.Get(entityId);
                var engine = engines.Read(entityId);

                var amplitude = math.lerp(40f, 60f, engine.RelativeRpm);
                var scale = math.lerp(.0008f, .005f, engine.RelativeRpm);

                var time = Time.realtimeSinceStartup * amplitude;

                vehicle.Body.Value.localPosition = new Vector3
                (
                    math.sin(time * amplitude) * scale,
                    math.cos(time * amplitude) * scale,
                    math.sin(time * amplitude) * scale
                    // math.cos(time),
                    // math.sin(time)
                );
            }
        }
    }
}