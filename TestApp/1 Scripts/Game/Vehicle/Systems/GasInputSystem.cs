using DesertImage.ECS;
using Unity.Mathematics;

namespace Game.Vehicle
{
    public struct GasInputSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<GasInput>()
                .With<Gas>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var gases = _group.GetComponents<Gas>();

            foreach (var entityId in _group)
            {
                ref var gas = ref gases.Get(entityId);
                gas.Value = math.max(0f, UnityEngine.Input.GetAxis("Vertical"));
            }
        }
    }
}