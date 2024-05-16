using System;
using System.Runtime.InteropServices;
using DesertImage.Collections;
using UnityEngine;

namespace TestApp._1_Scripts.Game
{
    public unsafe class TestLeak : MonoBehaviour
    {
        private UnsafeUintSparseSet<uint> _data;

        private void* _ptr;

        private void OnEnable()
        {
            // _data = new UnsafeUintSparseSet<uint>(20);
            // TestMethod(TestTarget);
        }

        private void TestMethod(Action method)
        {
            var ptr = Marshal.GetFunctionPointerForDelegate(method);
            Marshal.FreeHGlobal(ptr);
        }

        private void TestTarget()
        {
            
        }
            
        private void OnDisable()
        {
            // _data.Dispose();
        }
    }
}