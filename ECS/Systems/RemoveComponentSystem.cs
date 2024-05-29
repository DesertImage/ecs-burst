namespace DesertImage.ECS
{
    public struct RemoveComponentSystem<T> : IInitialize, IExecute where T : unmanaged
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world).With<T>().Find();
        }

        public void Execute(ref SystemsContext context)
        {
            foreach (var i in _group)
            {
                _group.GetEntity(i).Remove<T>();
            }
        }
    }
}