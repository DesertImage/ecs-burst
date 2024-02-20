using System;
using System.Diagnostics;
using System.Linq;
using DesertImage.ECS;

namespace DesertImage.ECS
{
    [DebuggerDisplay("Id: {Id}")]
    [DebuggerDisplay("{Components}")]
    public class EntityDebugView
    {
#if UNITY_EDITOR
        public uint Id => _entity.Id;
        public object[] Components { get; }

        private readonly Entity _entity;

        public EntityDebugView(Entity entity)
        {
            _entity = entity;

            Components = ComponentsDebug.Components.TryGetValue(entity.Id, out var components)
                ? components
                : Array.Empty<object>();

            Components = Components.Where(x => x != null).ToArray();
        }
#endif
    }
}