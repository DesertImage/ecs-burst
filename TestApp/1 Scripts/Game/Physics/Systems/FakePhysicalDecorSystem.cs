using DesertImage.ECS;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Physics
{
    public struct FakePhysicalDecorSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<FakePhysicalDecor>()
                .With<PositionDelta>()
                // .With<View>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var fakePhysicalDecors = _group.GetComponents<FakePhysicalDecor>();
            // var positions = _group.GetComponents<Position>();
            var positionDeltas = _group.GetComponents<PositionDelta>();
            var views = _group.GetComponents<View>();

            foreach (var entityId in _group)
            {
                var decor = fakePhysicalDecors.Read(entityId);
                var positionDelta = positionDeltas.Read(entityId);
                // ref var position = ref positions.Get(entityId);
                var view = views.Read(entityId).Value.Value;

                var movable = decor.Movable.Value;

                var viewTransform = view.transform;
                var position = viewTransform.position;
                // var newPos = movable.position + (Vector3)positionDelta.Value;
                var newPos = movable.localPosition + (Vector3)positionDelta.Value;
                var bounds = decor.Bounds;

                Debug.Log($"POS delta {(Vector3)positionDelta.Value}");
                // newPos = new Vector3
                // (
                //     math.clamp(position.x + bounds.MinX, position.x + bounds.MaxX, newPos.x),
                //     math.clamp(position.y + bounds.MinY, position.y + bounds.MaxY, newPos.y),
                //     math.clamp(position.z + bounds.MinZ, position.z + bounds.MaxZ, newPos.z)
                // );

                // newPos = new Vector3
                // (
                //     math.clamp(bounds.MinX, bounds.MaxX, newPos.x),
                //     math.clamp(bounds.MinY, bounds.MaxY, newPos.y),
                //     math.clamp(bounds.MinZ, bounds.MaxZ, newPos.z)
                // );

                // var pointVelocity = decor.Rigidbody.Value.GetRelativePointVelocity(position);

                // var xLocalVelocity = math.dot(viewTransform.right, pointVelocity);
                // var yLocalVelocity = math.dot(viewTransform.up, pointVelocity);
                // var zLocalVelocity = math.dot(viewTransform.forward, pointVelocity);

                // var localVelocity = new Vector3
                // (
                //     math.dot(viewTransform.right, pointVelocity),
                //     math.dot(viewTransform.up, pointVelocity),
                //     math.dot(viewTransform.forward, pointVelocity)
                // );
                //
                // localVelocity *= context.DeltaTime;
                
                // newPos = movable.localPosition + localVelocity;  

                movable.localPosition = newPos;
                // movable.rotation = viewTransform.rotation;
            }
        }
    }
}