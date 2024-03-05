namespace DesertImage.ECS
{
    public struct RemoveComponentSystem<T> : IInitSystem, IExecuteSystem where T : unmanaged
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world).With<T>().Find();
        }

        public unsafe void Execute(SystemsContext* context)
        {
            foreach (var entity in _group)
            {
                entity.Remove<T>();
            }
        }
    }
}