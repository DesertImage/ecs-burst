using System;
using System.Diagnostics;
using System.Linq;
using DesertImage.ECS;

namespace DesertImage.ECS
{
    [DebuggerDisplay("{Components}")]
    public class EntityDebugView
    {
#if UNITY_EDITOR
        public uint Id => _entity.Id;
        public object[] Components => _components;

        private readonly Entity _entity;
        private object[] _components;

        public EntityDebugView(Entity entity)
        {
            _entity = entity;

            _components = ComponentsDebug.Components.TryGetValue(entity.Id, out var components)
                ? components
                : Array.Empty<object>();

            _components = _components.Where(x => x != null).ToArray();
        }
#endif
    }
}