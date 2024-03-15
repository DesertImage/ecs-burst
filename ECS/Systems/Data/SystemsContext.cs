using Unity.Jobs;

namespace DesertImage.ECS
{
    public struct SystemsContext
    {
        public World World;
        public float DeltaTime;
        public JobHandle Handle;
    }
}