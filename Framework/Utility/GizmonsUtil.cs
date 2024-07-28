using bluebean.UGFramework.DataStruct;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework
{
    public static class GizmonsUtil
    {
        public static void DrawAabb(Aabb aabb,Color color)
        {
            Gizmos.color = color;
            var size = aabb.size.XYZ();
            var p0 = aabb.min.XYZ();
            var p1 = p0 + new Vector3(0, 0, size.z);
            var p2 = p0 + new Vector3(size.x, 0, size.z);
            var p3 = p0 + new Vector3(size.x, 0, 0);
            var p4 = p0 + new Vector3(0, size.y, 0);
            var p5 = p1 + new Vector3(0, size.y, 0);
            var p6 = p2 + new Vector3(0, size.y, 0);
            var p7 = p3 + new Vector3(0, size.y, 0);
            Gizmos.DrawLine(p0, p1);
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p0);

            Gizmos.DrawLine(p4, p5);
            Gizmos.DrawLine(p5, p6);
            Gizmos.DrawLine(p6, p7);
            Gizmos.DrawLine(p7, p4);

            Gizmos.DrawLine(p0, p4);
            Gizmos.DrawLine(p3, p7);
            Gizmos.DrawLine(p2, p6);
            Gizmos.DrawLine(p1, p5);
        }
    }
}
