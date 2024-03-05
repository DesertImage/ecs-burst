using System;
using System.Collections.Generic;
using Unity.Burst;

namespace DesertImage.ECS
{
    public static class ComponentsDebug
    {
#if UNITY_EDITOR
        public static readonly Dictionary<uint, object[]> Components = new Dictionary<uint, object[]>();

        [BurstDiscard]
        public static void Add<T>(uint entityId, T component) where T : struct
        {
            var componentId = ComponentTools.GetComponentIdFast<T>();
            if (Components.TryGetValue(entityId, out var components))
            {
                if (componentId >= components.Length)
                {
                    Array.Resize(ref components, (int)componentId + 1);
                }

                components[componentId] = component;
                Components[entityId] = components;
            }
            else
            {
                var array = new object[componentId + 1];
                array[componentId] = component;
                Components.Add(entityId, array);
            }
        }

        [BurstDiscard]
        public static void Remove<T>(uint entityId) where T : struct
        {
            var componentId = ComponentTools.GetComponentIdFast<T>();

            if (!Components.TryGetValue(entityId, out var components)) return;
            if (components.Length < componentId)
            {
                components[componentId] = null;
            }
        }

        [BurstDiscard]
        public static void Remove(uint entityId, uint componentId)
        {
            if (!Components.TryGetValue(entityId, out var components)) return;
            if (components.Length > componentId)
            {
                components[componentId] = null;
            }
        }

        [BurstDiscard]
        public static void RemoveAll(uint entityId)
        {
            if (!Components.TryGetValue(entityId, out var components)) return;
            Components[entityId] = Array.Empty<object>();
        }
#endif
    }
}