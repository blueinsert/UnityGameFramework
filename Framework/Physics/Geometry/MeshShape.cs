using bluebean.UGFramework.DataStruct;
using bluebean.UGFramework.DataStruct.Native;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.Geometry
{
    public struct MeshShape
    {
        public BIHNode[] m_bihNodes;
        public Triangle[] m_triangles;
        public Vector3[] m_vertices;
        public Vector3[] m_normals;

        public AffineTransform m_local2WorldTransform;

        public bool m_hasCreated;

        public void Build(Mesh source)
        {
            var sourceTris = source.triangles;
            var sourceVertices = source.vertices;

            // Build a bounding interval hierarchy from the triangles:
            IBounded[] t = new IBounded[sourceTris.Length / 3];
            for (int i = 0; i < t.Length; ++i)
            {
                int t1 = sourceTris[i * 3];
                int t2 = sourceTris[i * 3 + 1];
                int t3 = sourceTris[i * 3 + 2];
                var p1 = sourceVertices[t1];// local2World.MultiplyPoint3x4(sourceVertices[t1]);
                var p2 = sourceVertices[t2];//local2World.MultiplyPoint3x4(sourceVertices[t2]);
                var p3 = sourceVertices[t3];// local2World.MultiplyPoint3x4(sourceVertices[t3]);
                t[i] = new Triangle(t1, t2, t3, p1, p2, p3);
            }
            var sourceBih = BIH.Build(ref t);

            Triangle[] tris = Array.ConvertAll(t, x => (Triangle)x);

            m_bihNodes = sourceBih;
            m_triangles = tris;
            m_vertices = sourceVertices;
            m_normals = source.normals;

            m_hasCreated = true;
        }
    }
}
