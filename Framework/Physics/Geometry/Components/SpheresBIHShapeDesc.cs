using bluebean.UGFramework.DataStruct;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.Geometry
{
    public class SpheresBIHShapeDesc : MonoBehaviour
    {
        public BIHSpheres spheres = null;

        public bool m_isDrawGizmons = false;
        [Range(0, 10)]
        public int m_drawDepth = 3;

        #region Gizmos

        void OnDrawGizmos()
        {
            if (!m_isDrawGizmons) return;

            GizmonsUtil.DrawAabb(spheres.AABB, Color.green);

            if (spheres != null)
            {
                int depth = 0;
                if (m_drawDepth > 0)
                    TraverseBIHNodeList(spheres.spheres, spheres.nodes, 0, ref depth);
            }
        }

        void TraverseBIHNodeList(Sphere[] spheres, BIHNode[] nodes, int index, ref int depth)
        {
            var node = nodes[index];
            int start = node.start;
            int end = start + (node.count - 1);

            // calculate bounding box of all elements:
            Aabb b = spheres[start].GetBounds();
            for (int k = start + 1; k <= end; ++k)
                b.Encapsulate(spheres[k].GetBounds());
            if (node.firstChild != -1)
            {
                int axis = node.axis;
                float left = node.leftSplitPlane;
                float right = node.rightSplitPlane;
                DrawQuad(b, axis, left, Color.blue);
                DrawQuad(b, axis, right, Color.red);
                depth++;
                if (depth < m_drawDepth)
                {
                    TraverseBIHNodeList(spheres, nodes, node.firstChild, ref depth);
                    TraverseBIHNodeList(spheres, nodes, node.firstChild + 1, ref depth);
                }
            }
        }

        void DrawQuad(Aabb aabb, int axis, float value, Color color)
        {
            Gizmos.color = color;
            var min = new Vector3(aabb.min.x, aabb.min.y, aabb.min.z);
            var max = new Vector3(aabb.max.x, aabb.max.y, aabb.max.z);
            var size = new Vector3(aabb.size.x, aabb.size.y, aabb.size.z);
            min[axis] = value;
            max[axis] = value;
            var dir1 = Vector3.one;
            var dir2 = Vector3.one;
            switch (axis)
            {
                case 0:
                    dir1 = new Vector3(0, size.y, 0);
                    dir2 = new Vector3(0, 0, size.z);
                    break;
                case 1:
                    dir1 = new Vector3(size.x, 0, 0);
                    dir2 = new Vector3(0, 0, size.z);
                    break;
                case 2:
                    dir1 = new Vector3(size.x, 0, 0);
                    dir2 = new Vector3(0, size.y, 0);
                    break;
            }
            Mesh quad = CreateQuad(min, min + dir2, max, max - dir2);
            Gizmos.DrawMesh(quad);
        }

        Mesh CreateQuad(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            Vector3[] vertices = {
            p1,p2,p3,p4
        };
            int[] indices = new int[6] { 0, 1, 3, 1, 2, 3 };
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = indices;
            mesh.RecalculateNormals();
            return mesh;
        }



        #endregion
    }
}
