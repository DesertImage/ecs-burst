using System.Collections.Generic;
using System.Linq;
using External;

namespace DesertImage.ECS
{
    public class BaseMatcher : IMatcher
    {
        private static ushort _matchersCounter;

        public ushort Id { get; }

        public ushort[] ComponentIds { get; }
        private readonly HashSet<ushort> _componentsHashSet;

        protected BaseMatcher(ushort componentId)
        {
            Id = _matchersCounter;
            _matchersCounter++;

            ComponentIds = new[] { componentId };
            _componentsHashSet = new HashSet<ushort>(ComponentIds, new UshortComparer());
        }

        public BaseMatcher(ushort[] componentIds)
        {
            Id = _matchersCounter;
            _matchersCounter++;

            if (componentIds == null) return;

            ComponentIds = componentIds.OrderBy(x => x).ToArray();
            _componentsHashSet = new HashSet<ushort>(ComponentIds, new UshortComparer());
        }

        public virtual bool IsMatch(IEntity entity)
        {
            for (var i = 0; i < ComponentIds.Length; i++)
            {
                if (entity.HasComponent(ComponentIds[i])) continue;

                return false;
            }

            return true;
        }

        public virtual bool IsContainsComponent(ushort componentId)
        {
            return _componentsHashSet.Contains(componentId);
        }

        public virtual bool Equals(IMatcher other)
        {
            if (!IsEqualTypes(other)) return false;

            if (other.ComponentIds.Length != ComponentIds.Length) return false;

            for (var i = 0; i < ComponentIds.Length; i++)
            {
                if (ComponentIds[i] != other.ComponentIds[i]) return false;
            }

            return true;
        }

        protected virtual bool IsEqualTypes(IMatcher other)
        {
            return other is BaseMatcher;
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public void OnCreate()
        {
        }

        public void ReturnToPool()
        {
            Dispose();
        }

        public void Dispose()
        {
        }
    }
}