using System;
using System.Reflection;
using System.Runtime.InteropServices;
using DesertImage.Collections;

namespace DesertImage.ECS
{
    public static unsafe class Systems
    {
        public static void Add<T>(in World world, ExecutionOrder order) where T : unmanaged, ISystem
        {
            var systemType = typeof(T);
            var systemId = SystemsTools.GetId<T>();

            var state = world.SystemsState;

            if (Contains<T>(state))
            {
#if DEBUG_MODE
                throw new Exception($"system {systemType} already added");
#else
                return;
#endif
            }

            var system = default(T);
            var instance = MemoryUtility.AllocateInstance(in system);

            var isInit = typeof(IInitialize).IsAssignableFrom(systemType);
            if (isInit)
            {
                var methodInfo = typeof(Systems).GetMethod
                (
                    nameof(AddInit),
                    BindingFlags.Static | BindingFlags.NonPublic
                );

                var gMethod = methodInfo!.MakeGenericMethod(systemType);

                var targetDelegate = Delegate.CreateDelegate(typeof(Action<IntPtr, World>), default, gMethod);
                var converted = (Action<IntPtr, World>)targetDelegate;

                converted.Invoke((IntPtr)instance, world);
            }

            var isExecute = typeof(IExecute).IsAssignableFrom(systemType);
            if (isExecute)
            {
                var wrapper = new ExecuteSystemWrapper { Value = instance };
                var wrapperPtr = MemoryUtility.AllocateInstance(in wrapper);

                var methodInfo = typeof(Systems).GetMethod
                (
                    nameof(AddExecute),
                    BindingFlags.Static | BindingFlags.NonPublic
                );

                var gMethod = methodInfo!.MakeGenericMethod(systemType);

                var targetDelegate = Delegate.CreateDelegate
                (
                    typeof(Action<IntPtr, IntPtr, ExecutionOrder>),
                    default,
                    gMethod
                );

                var converted = (Action<IntPtr, IntPtr, ExecutionOrder>)targetDelegate;

                converted.Invoke((IntPtr)world.Ptr, (IntPtr)wrapperPtr, order);
            }

            var isGizmos = typeof(IDrawGizmos).IsAssignableFrom(systemType);
            if (isGizmos)
            {
                var wrapper = new ExecuteSystemWrapper { Value = instance };
                var wrapperPtr = MemoryUtility.AllocateInstance(in wrapper);

                var methodInfo = typeof(Systems).GetMethod
                (
                    nameof(AddDrawGizmos),
                    BindingFlags.Static | BindingFlags.NonPublic
                );

                var gMethod = methodInfo!.MakeGenericMethod(systemType);

                var targetDelegate = Delegate.CreateDelegate
                (
                    typeof(Action<IntPtr, IntPtr>),
                    default,
                    gMethod
                );

                var converted = (Action<IntPtr, IntPtr>)targetDelegate;

                converted.Invoke((IntPtr)world.Ptr, (IntPtr)wrapperPtr);
            }

            state->SystemsHash.Set(systemId, systemId);
        }

        public static void Remove<T>(SystemsState* state) where T : ISystem
        {
            if (!Contains<T>(state))
            {
#if DEBUG_MODE
                throw new Exception($"system {typeof(T)} haven't been added");
#else
                return;
#endif
            }

            var systemId = SystemsTools.GetId<T>();

            state->SystemsHash.Remove(systemId);

            Remove(ref state->EarlyMainThreadSystems, systemId);
            Remove(ref state->MultiThreadSystems, systemId);
            Remove(ref state->LateMainThreadSystems, systemId);
            Remove(ref state->RemoveTagsSystems, systemId);
        }

        private static void Remove(ref UnsafeList<ExecuteSystemData> values, uint systemId)
        {
            for (var i = values.Count - 1; i >= 0; i--)
            {
                var systemData = values[i];
                if (systemData.Id != systemId) continue;
                values.RemoveAt(i);
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

            state->Context.DeltaTime = deltaTime;

            ExecuteMainThread(ref state->EarlyMainThreadSystems, state);
            ExecuteMultiThread(ref state->MultiThreadSystems, state);
            ExecuteMainThread(ref state->LateMainThreadSystems, state);
            ExecuteMainThread(ref state->RemoveTagsSystems, state);
        }

        public static void ExecutePhysics(World* world, float deltaTime)
        {
            var state = world->SystemsState;

            state->Context.DeltaTime = deltaTime;

            ExecuteMainThread(ref state->PhysicsSystems, state);
        }

        public static void ExecuteGizmos(World* world)
        {
            var state = world->SystemsState;

            var systems = state->DrawGizmosSystems;
            for (var i = 0; i < systems.Count; i++)
            {
                var wrapper = systems[i].Wrapper;
                var method = Marshal.GetDelegateForFunctionPointer<SystemsTools.DrawGizmos>((IntPtr)wrapper->MethodPtr);
                method.Invoke(wrapper, world);
            }
        }

        private static void ExecuteMainThread(ref UnsafeList<ExecuteSystemData> systems, SystemsState* state)
        {
            for (var i = 0; i < systems.Count; i++)
            {
                var wrapper = systems[i].Wrapper;
                var method = Marshal.GetDelegateForFunctionPointer<SystemsTools.Execute>((IntPtr)wrapper->MethodPtr);
                method.Invoke(wrapper, ref state->Context);
            }
        }

        public static void ExecuteMultiThread(ref UnsafeList<ExecuteSystemData> systems, SystemsState* state)
        {
            for (var i = 0; i < systems.Count; i++)
            {
                var wrapper = systems[i].Wrapper;
                var method = Marshal.GetDelegateForFunctionPointer<SystemsTools.Execute>((IntPtr)wrapper->MethodPtr);
                method.Invoke(wrapper, ref state->Context);

                state->Context.Handle.Complete();
            }
        }

        private static void AddInit<T>(IntPtr ptr, World world) where T : unmanaged, IInitialize
        {
            (*(T*)ptr).Initialize(world);
        }

        private static void AddExecute<T>(IntPtr worldPtr, IntPtr wrapperPtr, ExecutionOrder order)
            where T : unmanaged, IExecute
        {
            var systemId = SystemsTools.GetId<T>();

            var wrapper = (ExecuteSystemWrapper*)wrapperPtr;
            wrapper->MethodPtr = SystemsToolsExecute<T>.MakeExecuteMethod();

            var world = (World*)worldPtr;
            var state = world->SystemsState;

            var data = new ExecuteSystemData
            {
                Id = systemId,
                Wrapper = wrapper
            };

            switch (order)
            {
                case ExecutionOrder.EarlyMainThread:
                    state->EarlyMainThreadSystems.Add(data);
                    break;
                case ExecutionOrder.LateMainThread:
                    state->LateMainThreadSystems.Add(data);
                    break;
                case ExecutionOrder.RemoveTags:
                    state->RemoveTagsSystems.Add(data);
                    break;
                case ExecutionOrder.Physics:
                    state->PhysicsSystems.Add(data);
                    break;
                default:
                    state->MultiThreadSystems.Add(data);
                    break;
            }
        }

        private static void AddDrawGizmos<T>(IntPtr worldPtr, IntPtr wrapperPtr) where T : unmanaged, IDrawGizmos
        {
            var systemId = SystemsTools.GetId<T>();

            var wrapper = (ExecuteSystemWrapper*)wrapperPtr;
            wrapper->MethodPtr = SystemsToolsDrawGizmos<T>.MakeDrawGizmosMethod();

            var data = new ExecuteSystemData
            {
                Id = systemId,
                Wrapper = wrapper
            };

            ((World*)worldPtr)->SystemsState->DrawGizmosSystems.Add(data);
        }
    }
}