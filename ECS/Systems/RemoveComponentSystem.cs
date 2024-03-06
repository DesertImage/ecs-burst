namespace DesertImage.ECS
{
    public struct RemoveComponentSystem<T> : IInitSystem, IExecuteSystem where T : unmanaged
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world).With<T>().Find();
        }

        public void Execute(ref SystemsContext context)
        {
            foreach (var entity in _group)
            {
                entity.Remove<T>();
            }
        }
    }
}