using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace bluebean.UGFramework
{
    public static class MathUtil
    {
        public static Vector3 XYZ(this Vector4 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }
    }
}
