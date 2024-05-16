using DesertImage.ECS;
using UnityEngine;

namespace Game.Vehicle
{
    public struct GearboxInputSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<Gearbox>()
                .None<GearUpTag>()
                .None<GearDownTag>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var gearboxes = _group.GetComponents<Gearbox>();

            foreach (var entityId in _group)
            {
                var entity = _group.GetEntity(entityId);

                if (UnityEngine.Input.GetKeyDown(KeyCode.P))
                {
                    entity.Replace<GearUpTag>();
                }

                if (UnityEngine.Input.GetKeyDown(KeyCode.L))
                {
                    entity.Replace<GearDownTag>();
                }
            }
        }
    }
}