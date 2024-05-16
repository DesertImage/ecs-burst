using DesertImage.ECS;
using UnityEngine;

namespace Game.Physics
{
    public class FakePhysicsDecorView : EntityView
    {
        [SerializeField] private Bound Bound;
        [SerializeField] private Transform movable;
        [SerializeField] private Rigidbody rigidbody;

        public override void Initialize(in Entity entity)
        {
            base.Initialize(in entity);

            // movable.parent = null;
            entity.Replace
            (
                new FakePhysicalDecor
                {
                    Bounds = Bound,
                    Rigidbody = rigidbody,
                    Movable = movable
                }
            );
            entity.Replace(new PositionDelta { Last = transform.position });
        }
    }
}