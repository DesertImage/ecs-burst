using DesertImage.Collections;
using Unity.Burst;
using UnityEngine.Jobs;

namespace DesertImage.ECS
{
    public struct TransformToEntitySystem : IInitialize, IExecute
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<Position>()
                .With<Rotation>()
                .With<Scale>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var viewTransforms = context.World.GetStatic<ViewTransforms>();

            var positions = _group.GetComponents<Position>();
            var rotations = _group.GetComponents<Rotation>();
            var scales = _group.GetComponents<Scale>();

            var job = new TransformToEntityJob
            {
                EntityIndexes = viewTransforms.Indexes,
                Positions = positions.Values,
                Rotations = rotations.Values,
                Scales = scales.Values
            };
            
            context.Handle = job.Schedule(viewTransforms.Values, context.Handle);
        }

        [BurstCompile]
        private struct TransformToEntityJob : IJobParallelForTransform
        {
            public UnsafeUintSparseSet<int> EntityIndexes;

            public UnsafeReadOnlyArray<Position> Positions;
            public UnsafeReadOnlyArray<Rotation> Rotations;
            public UnsafeReadOnlyArray<Scale> Scales;

            public void Execute(int index, TransformAccess transform)
            {
                var entityId = (int)EntityIndexes.ReadInvert(index);

                Positions.Get(entityId).Value = transform.position;
                Rotations.Get(entityId).Value = transform.rotation;
                Scales.Get(entityId).Value = transform.localScale;
            }
        }
    }
}