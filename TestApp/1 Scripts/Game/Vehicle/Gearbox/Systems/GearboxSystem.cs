using DesertImage.ECS;

namespace Game.Vehicle
{
    public struct GearboxSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _upGroup;
        private EntitiesGroup _downGroup;
        private EntitiesGroup _switchGroup;
        private EntitiesGroup _updatedGroup;

        public void Initialize(in World world)
        {
            _upGroup = Filter.Create(world)
                .With<Gearbox>()
                .With<GearUpTag>()
                .Find();

            _downGroup = Filter.Create(world)
                .With<Gearbox>()
                .With<GearDownTag>()
                .Find();

            _switchGroup = Filter.Create(world)
                .With<Gearbox>()
                .With<GearSwitch>()
                .Find();

            _updatedGroup = Filter.Create(world)
                .With<Gearbox>()
                .With<ComponentUpdated<Gearbox>>()
                .Find();

            var gearboxGroup = Filter.Create(world)
                .With<Gearbox>()
                .Find();

            foreach (var entityId in gearboxGroup)
            {
                gearboxGroup.GetEntity(entityId).Replace<ComponentUpdated<Gearbox>>();
            }
        }

        public void Execute(ref SystemsContext context)
        {
            var gearboxes = _upGroup.GetComponents<Gearbox>();
            var gearSwitcher = _upGroup.GetComponents<GearSwitch>();

            foreach (var entityId in _upGroup)
            {
                ref var gearbox = ref gearboxes.Get(entityId);

                if (gearbox.Gear >= gearbox.Max) continue;

                var entity = _upGroup.GetEntity(entityId);

                entity.Replace
                (
                    new GearSwitch
                    {
                        Gear = gearbox.Gear + 1,
                        TargetTime = gearbox.SwitchTime
                    }
                );

                gearbox.Gear = 1;

                entity.Replace(new ComponentUpdated<Gearbox>());
            }

            foreach (var entityId in _downGroup)
            {
                ref var gearbox = ref gearboxes.Get(entityId);

                if (gearbox.Gear <= 0) continue;

                var entity = _upGroup.GetEntity(entityId);

                entity.Replace
                (
                    new GearSwitch
                    {
                        Gear = gearbox.Gear - 1,
                        TargetTime = gearbox.SwitchTime
                    }
                );

                gearbox.Gear = 1;

                entity.Replace(new ComponentUpdated<Gearbox>());
            }

            foreach (var entityId in _switchGroup)
            {
                ref var gearbox = ref gearboxes.Get(entityId);
                ref var gearSwitch = ref gearSwitcher.Get(entityId);

                gearSwitch.ElapsedTime += context.DeltaTime;

                if (gearSwitch.ElapsedTime < gearSwitch.TargetTime)
                {
                    gearSwitch.ElapsedTime += context.DeltaTime;
                    continue;
                }

                gearbox.Gear = gearSwitch.Gear;

                var entity = _switchGroup.GetEntity(entityId);

                entity.Remove<GearSwitch>();
                entity.Replace(new ComponentUpdated<Gearbox>());
            }


            foreach (var entityId in _updatedGroup)
            {
                ref var gearbox = ref gearboxes.Get(entityId);
                gearbox.TotalGearRatio = gearbox.GearRatio[gearbox.Gear] * gearbox.MainGear;
            }
        }
    }
}