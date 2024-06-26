using System;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;

namespace DesertImage.ECS
{
    public static unsafe class SystemsTools
    {
        private static uint _idCounter;

        public delegate void Execute(void* wrapper, ref SystemsContext context);
        public delegate void Destroy(void* wrapper, ref SystemsContext context);

        public delegate void DrawGizmos(void* wrapper, void* world);

        public static uint GetId<T>() where T : ISystem
        {
            var id = SystemsTools<T>.Id;

            if (id == 0)
            {
                id = ++_idCounter;
                SystemsTools<T>.Id = id;
            }

            return id;
        }
    }

    public static class SystemsTools<T> where T : ISystem
    {
        public static uint Id;
    }

    [BurstCompile]
    public static unsafe class SystemsToolsDrawGizmos<T> where T : unmanaged, IDrawGizmos
    {
        public static void* MakeDrawGizmosMethod()
        {
            return (void*)Marshal.GetFunctionPointerForDelegate(new SystemsTools.DrawGizmos(MakeDrawGizmos));
        }

        [BurstCompile]
        [MonoPInvokeCallback(typeof(SystemsTools.DrawGizmos))]
        private static void MakeDrawGizmos(void* wrapper, void* world)
        {
            var ptr = *(T*)((ExecuteSystemWrapper*)wrapper)->Value;
            ptr.DrawGizmos(*(World*)world);
        }
    }

    [BurstCompile]
    public static unsafe class SystemsToolsExecute<T> where T : unmanaged, IExecute
    {
        public static void* MakeMethod()
        {
            return (void*)Marshal.GetFunctionPointerForDelegate(new SystemsTools.Execute(MakeExecute));

            // return isBurst
            // ? (void*)BurstCompiler.CompileFunctionPointer<SystemsTools.Execute>(MakeExecute).Value
            // : (void*)Marshal.GetFunctionPointerForDelegate(new SystemsTools.Execute(MakeExecute));
        }

        [BurstCompile]
        [MonoPInvokeCallback(typeof(SystemsTools.Execute))]
        private static void MakeExecute(void* wrapper, ref SystemsContext context)
        {
            var ptr = *(T*)((ExecuteSystemWrapper*)wrapper)->Value;
            ptr.Execute(ref context);
        }
    }
    
    [BurstCompile]
    public static unsafe class SystemsToolsDestroy<T> where T : unmanaged, IDestroy
    {
        public static void* MakeMethod()
        {
            return (void*)Marshal.GetFunctionPointerForDelegate(new SystemsTools.Destroy(MakeDestroy));

            // return isBurst
            // ? (void*)BurstCompiler.CompileFunctionPointer<SystemsTools.Execute>(MakeExecute).Value
            // : (void*)Marshal.GetFunctionPointerForDelegate(new SystemsTools.Execute(MakeExecute));
        }

        [BurstCompile]
        [MonoPInvokeCallback(typeof(SystemsTools.Execute))]
        private static void MakeDestroy(void* wrapper, ref SystemsContext context)
        {
            var ptr = *(T*)((ExecuteSystemWrapper*)wrapper)->Value;
            ptr.OnDestroy(context.World);
        }
    }
}