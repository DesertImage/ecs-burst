using DesertImage.ECS;

namespace Game
{
    public class RemoveComponentSystem<T> : SystemBase, IReactEntityAddedSystem where T : IComponent, new()
    {
        public IMatcher Matcher { get; }

        private ushort _componentId;
        
        public RemoveComponentSystem()
        {
            var component = ComponentsTool.GetInstanceFromPool<T>();
            Matcher = Match.AllOf(component.Id);
        }

        public void Execute(IEntity entity) => entity.Remove(_componentId);
    }
}