using System;
using System.Reflection;
using System.Runtime.InteropServices;
using DesertImage.Collections;

namespace DesertImage.ECS
{
    public static unsafe class Systems
    {
        public static void Add<T>(in World world, ExecutionType type) where T : unmanaged, ISystem
        {
            var systemType = typeof(T);
            var systemId = SystemsTools.GetId<T>();

            var state = world.SystemsState;

            if (Contains<T>(state))
            {
#if DEBUG_MODE
                throw new Exception($"system {systemType} already added");
#endif
                return;
            }

            var instance = MemoryUtility.Allocate(default(T));

            var isInit = typeof(IInitSystem).IsAssignableFrom(systemType);
            if (isInit)
            {
                var methodInfo = typeof(Systems).GetMethod
                (
                    nameof(AddInit),
                    BindingFlags.Static | BindingFlags.NonPublic
                );

                var gMethod = methodInfo.MakeGenericMethod(systemType);

                var targetDelegate = Delegate.CreateDelegate(typeof(Action<IntPtr, World>), default, gMethod);
                var converted = (Action<IntPtr, World>)targetDelegate;

                converted.Invoke((IntPtr)instance, world);
            }

            var isExecute = typeof(IExecuteSystem).IsAssignableFrom(systemType);
            if (isExecute)
            {
                var wrapper = new ExecuteSystemWrapper { Value = instance };

                var wrapperPtr = MemoryUtility.Allocate(wrapper);

                var methodInfo = typeof(Systems).GetMethod
                (
                    nameof(AddExecute),
                    BindingFlags.Static | BindingFlags.NonPublic
                );

                var gMethod = methodInfo.MakeGenericMethod(systemType);

                var targetDelegate = Delegate.CreateDelegate
                (
                    typeof(Action<IntPtr, IntPtr, ExecutionType>),
                    default,
                    gMethod
                );

                var converted = (Action<IntPtr, IntPtr, ExecutionType>)targetDelegate;

                converted.Invoke((IntPtr)world.Ptr, (IntPtr)wrapperPtr, type);
            }

            state->SystemsHash.Set(systemId, systemId);
        }

        public static void Remove<T>(SystemsState* state) where T : ISystem
        {
            if (!Contains<T>(state))
            {
#if DEBUG_MODE
                throw new Exception($"system {typeof(T)} haven't been added");
#endif
                return;
            }

            var systemId = SystemsTools.GetId<T>();

            state->SystemsHash.Remove(systemId);

            for (var i = 0; i < state->EarlyMainThreadSystems.Count; i++)
            {
                var systemData = state->EarlyMainThreadSystems[i];
                if (systemData.Id != systemId) continue;
                state->EarlyMainThreadSystems.RemoveAt(i);
                return;
            }

            for (var i = 0; i < state->MultiThreadSystems.Count; i++)
            {
                var systemData = state->MultiThreadSystems[i];
                if (systemData.Id != systemId) continue;
                state->MultiThreadSystems.RemoveAt(i);
                break;
            }
        }

        public static bool Contains<T>(SystemsState* state) where T : ISystem
        {
            var id = SystemsTools.GetId<T>();
            return state->SystemsHash.Contains(id);
        }

        public static void Execute(World* world, float deltaTime)
        {
            var state = world->SystemsState;

            state->Context->DeltaTime = deltaTime;

            ExecuteMainThread(ref state->EarlyMainThreadSystems, state);
            ExecuteMultiThread(ref state->MultiThreadSystems, state);
        }

        private static void ExecuteMainThread(ref UnsafeList<ExecuteSystemData> systems, SystemsState* state)
        {
            for (var i = 0; i < systems.Count; i++)
            {
                var systemData = systems[i];

                var wrapper = systemData.Wrapper;
                var method = Marshal.GetDelegateForFunctionPointer<SystemsTools.Execute>((IntPtr)wrapper->MethodPtr);
                method.Invoke(wrapper, state->Context);
            }
        }

        public static void ExecuteMultiThread(ref UnsafeList<ExecuteSystemData> systems, SystemsState* state)
        {
            for (var i = 0; i < systems.Count; i++)
            {
                var systemData = systems[i];

                var wrapper = systemData.Wrapper;
                var method = Marshal.GetDelegateForFunctionPointer<SystemsTools.Execute>((IntPtr)wrapper->MethodPtr);
                method.Invoke(wrapper, state->Context);
            }

            // state->Context->Handle.Complete();
        }

        private static void AddInit<T>(IntPtr ptr, World world) where T : unmanaged, IInitSystem
        {
            (*(T*)ptr).Initialize(world);
        }

        private static void AddExecute<T>(IntPtr worldPtr, IntPtr wrapperPtr, ExecutionType type)
            where T : unmanaged, IExecuteSystem
        {
            var systemId = SystemsTools.GetId<T>();

            var wrapper = (ExecuteSystemWrapper*)wrapperPtr;
            wrapper->MethodPtr = SystemsToolsExecute<T>.MakeExecuteMethod();
            wrapper->IsCalculateSystem = (byte)(typeof(ICalculateSystem).IsAssignableFrom(typeof(T)) ? 1 : 0);

            var world = (World*)worldPtr;
            var state = world->SystemsState;

            var data = new ExecuteSystemData
            {
                Id = systemId,
                Wrapper = wrapper
            };

            switch (type)
            {
                case ExecutionType.MainThread:
                    state->EarlyMainThreadSystems.Add(data);
                    break;
                default:
                    state->MultiThreadSystems.Add(data);
                    break;
            }
        }
    }
}