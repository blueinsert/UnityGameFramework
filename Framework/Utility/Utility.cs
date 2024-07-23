using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace bluebean.UGFramework
{
    public static class Utility
    {
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PureSign(this float val)
        {
            return ((0 <= val) ? 1 : 0) - ((val < 0) ? 1 : 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
    }
}
