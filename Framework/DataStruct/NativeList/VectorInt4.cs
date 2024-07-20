using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace bluebean.UGFramework.DataStruct
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct VectorInt4
    {
        public int x;
        public int y;
        public int z;
        public int w;

        public VectorInt4(int x, int y, int z, int w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public VectorInt4(int x)
        {
            this.x = x;
            this.y = x;
            this.z = x;
            this.w = x;
        }
    }
}
