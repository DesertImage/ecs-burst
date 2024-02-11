using AOT;
using Unity.Burst;

namespace DesertImage.ECS
{
    public static unsafe class SystemsTools
    {
        private static int _idCounter = -1;

        public delegate void Execute(void* wrapper, Entity entity, float deltaTime);
        public delegate void Destroy();
        
        public static int GetId<T>() where T : ISystem
        {
            var id = SystemsTools<T>.Id;
            
            if (id < 0)
            {
                id = ++_idCounter;
            }

            return id;
        }
    }

    public static class SystemsTools<T> where T : ISystem
    {
        public static int Id = -1;
    }

    [BurstCompile]
    public static unsafe class SystemsToolsExecute<T> where T : unmanaged, IExecuteSystem
    {
        public static void* MakeExecuteMethod()
        {
            return (void*)BurstCompiler.CompileFunctionPointer<SystemsTools.Execute>(MakeExecute).Value;
        }

        [BurstCompile]
        [MonoPInvokeCallback(typeof(SystemsTools.Execute))]
        private static void MakeExecute(void* wrapper, Entity entity, float deltaTime)
        {
            var ptr = *(T*)((ExecuteSystemWrapper*)wrapper)->Value;
            ptr.Execute(entity, deltaTime);
        }
    }
}