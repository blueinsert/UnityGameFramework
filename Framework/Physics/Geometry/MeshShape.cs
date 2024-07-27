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
        public NativeBIHNodeList m_bihNodes;
        public NativeTriangleList m_triangles;
        public NativeVector3List m_vertices;

        public void Build(Matrix4x4 local2World, Mesh source)
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
                var p1 = local2World.MultiplyPoint3x4(sourceVertices[t1]);
                var p2 = local2World.MultiplyPoint3x4(sourceVertices[t2]);
                var p3 = local2World.MultiplyPoint3x4(sourceVertices[t3]);
                t[i] = new Triangle(t1, t2, t3, p1, p2, p3);
            }
            var sourceBih = BIH.Build(ref t);

            Triangle[] tris = Array.ConvertAll(t, x => (Triangle)x);

            if (m_bihNodes != null) m_bihNodes.Dispose();
            if (m_triangles != null) m_triangles.Dispose();
            if (m_vertices != null) m_vertices.Dispose();
            m_bihNodes = new NativeBIHNodeList();
            m_triangles = new NativeTriangleList();
            m_vertices = new NativeVector3List();
            m_bihNodes.AddRange(sourceBih);
            m_triangles.AddRange(tris);
            m_vertices.AddRange(sourceVertices);
        }
    }
}
