using System;
using System.Runtime.InteropServices;
using DesertImage.Collections;
using Unity.Collections;

namespace DesertImage.ECS
{
    public struct SystemsState : IDisposable
    {
        public UnsafeList<ExecuteSystemData> EarlyMainThreadSystems;
        public UnsafeList<ExecuteSystemData> MultiThreadSystems;
        public UnsafeList<ExecuteSystemData> LateMainThreadSystems;
        public UnsafeList<ExecuteSystemData> RemoveTagsSystems;
        public UnsafeList<ExecuteSystemData> PhysicsSystems;
        public UnsafeList<ExecuteSystemData> DrawGizmosSystems;
        public UnsafeList<DestroySystemData> DestroySystems;
        public UnsafeUintSparseSet<uint> SystemsHash;

        public SystemsContext Context;

        public SystemsState(int capacity)
        {
            EarlyMainThreadSystems = new UnsafeList<ExecuteSystemData>(capacity, Allocator.Persistent);
            MultiThreadSystems = new UnsafeList<ExecuteSystemData>(capacity, Allocator.Persistent);
            LateMainThreadSystems = new UnsafeList<ExecuteSystemData>(capacity, Allocator.Persistent);
            PhysicsSystems = new UnsafeList<ExecuteSystemData>(capacity, Allocator.Persistent);
            RemoveTagsSystems = new UnsafeList<ExecuteSystemData>(capacity, Allocator.Persistent);
            DrawGizmosSystems = new UnsafeList<ExecuteSystemData>(capacity, Allocator.Persistent);
            DestroySystems = new UnsafeList<DestroySystemData>(capacity, Allocator.Persistent);
            SystemsHash = new UnsafeUintSparseSet<uint>(capacity);
            Context = new SystemsContext();
        }

        private static void DisposeSystems(UnsafeList<ExecuteSystemData> values)
        {
            for (var i = 0; i < values.Count; i++)
            {
                values[i].Dispose();
            }

            values.Dispose();
        }
        
        private static void DisposeSystems(UnsafeList<DestroySystemData> values)
        {
            for (var i = 0; i < values.Count; i++)
            {
                values[i].Dispose();
            }

            values.Dispose();
        }

        public unsafe void Dispose()
        {
            DisposeSystems(EarlyMainThreadSystems);
            DisposeSystems(MultiThreadSystems);
            DisposeSystems(LateMainThreadSystems);
            DisposeSystems(PhysicsSystems);
            DisposeSystems(RemoveTagsSystems);
            DisposeSystems(DrawGizmosSystems);
            
            foreach (var data in DestroySystems)
            {
                var wrapper = data.Wrapper;

                var method = Marshal.GetDelegateForFunctionPointer<SystemsTools.Destroy>((IntPtr)wrapper->MethodPtr);
                method.Invoke(wrapper, ref Context);
            }
            
            DisposeSystems(DestroySystems);

            SystemsHash.Dispose();
        }
    }
}