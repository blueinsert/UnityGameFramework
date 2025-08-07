using bluebean.UGFramework.DataStruct;
using UnityEngine;

namespace bluebean.UGFramework.Geometry
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    public class MeshShapeDesc : ShapeDescBase
    {
        public MeshShape Shape { 
            get {
                return m_shape; 
            }
        }

        private MeshShape m_shape = new MeshShape();
        private MeshPool m_meshPool = new MeshPool();
        public bool m_isDrawGizmons = false;
        [Range(0,10)]
        public int m_drawDepth = 3;

        private void OnValidate()
        {
            SetDirty();
        }

        // Start is called before the first frame update
        void Start()
        {
            SetDirty();
        }

        public override void UpdateShapeImpl()
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();

            m_shape.Build(meshFilter.sharedMesh);
            m_shape.m_local2WorldTransform.FromTransform(this.transform);
        }

        private void OnDestroy()
        {
            m_meshPool.Destroy();
        }

        #region Gizmos

        void OnDrawGizmos()
        {
            if (!m_isDrawGizmons) return;
            m_meshPool.BackAll();

             var shape = m_shape;
            GizmonsUtil.DrawAabb(shape.WorldAabb, Color.green);
            var prev = Gizmos.matrix;
            var local2WorldMatrix = Matrix4x4.TRS(shape.m_local2WorldTransform.translation, shape.m_local2WorldTransform.rotation, shape.m_local2WorldTransform.scale);
            Gizmos.matrix = local2WorldMatrix;
            if (shape.m_bihNodes!=null)
            {
                int depth = 1;
                if(m_drawDepth > 0)
                    TraverseBIHNodeList(shape.m_triangles, shape.m_bihNodes, 0, depth);
            }
            GizmonsUtil.DrawAabb(shape.LocalAabb, Color.blue);
            Gizmos.matrix = prev;
        }

        void TraverseBIHNodeList(Triangle[] tris, BIHNode[] nodes, int index, int depth)
        {
            var node = nodes[index];
            int start = node.start;
            int end = start + (node.count - 1);

            // calculate bounding box of all elements:
            Aabb b = tris[start].GetBounds();
            for (int k = start + 1; k <= end; ++k)
                b.Encapsulate(tris[k].GetBounds());
            if (node.firstChild != -1)
            {
                int axis = node.axis;
                float left = node.leftSplitPlane;
                float right = node.rightSplitPlane;
                DrawQuad(b, axis, left, Color.blue);
                DrawQuad(b, axis, right, Color.red);
                depth++;
                if(depth <= m_drawDepth)
                {
                    TraverseBIHNodeList(tris, nodes, node.firstChild,  depth);
                    TraverseBIHNodeList(tris, nodes, node.firstChild + 1,depth);
                }
            }
        }

        void DrawQuad(Aabb aabb,int axis,float value,Color color)
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
            int[] indices = new int[12] { 0, 1, 3, 1, 2, 3,
            0,3,1,1,3,2};
            Mesh mesh = m_meshPool.Allocate().m_mesh;
            mesh.vertices = vertices;
            mesh.triangles = indices;
            mesh.RecalculateNormals();
            return mesh;
        }

       

        #endregion
    }
}
