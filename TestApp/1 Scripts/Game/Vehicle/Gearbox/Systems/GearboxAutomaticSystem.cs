using DesertImage.ECS;
using UnityEngine;

namespace Game.Vehicle
{
    public struct GearboxAutomaticSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<Gearbox>()
                .With<GearboxAutomatic>()
                .With<Engine>()
                .None<GearSwitch>()
                .None<GearUpTag>()
                .None<GearDownTag>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var gearboxes = _group.GetComponents<Gearbox>();
            var gearboxAutomatics = _group.GetComponents<GearboxAutomatic>();
            var engines = _group.GetComponents<Engine>();

            foreach (var entityId in _group)
            {
                var entity = _group.GetEntity(entityId);
                
                var gearbox = gearboxes.Read(entityId);
                var gearboxAuto = gearboxAutomatics.Read(entityId);
                var engine = engines.Read(entityId);

                var engineRpm = engine.Rpm;
                
                if (engineRpm >= engine.MaxRpm - 1)
                {
                    if (gearbox.Gear < gearbox.Max)
                    {
                        entity.Replace<GearUpTag>();
                    }
                }
                else if(engineRpm <= gearboxAuto.DecreaseRPM)
                {
                    if (gearbox.Gear > 2)
                    {
                        entity.Replace<GearDownTag>();
                    }
                }
            }
        }
    }
}