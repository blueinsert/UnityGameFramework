using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.Anim
{
    [RequireComponent(typeof(MeshFilter))]
    [ExecuteInEditMode]
    public class MeshProceduralAnim : MonoBehaviour
    {
        //[HideInInspector]
        public Vector3[] m_verticles;
        //[HideInInspector]
        public int[] m_triangles;
        //[HideInInspector]
        public Vector2[] m_uvs;

        //[HideInInspector]
        [Header("产生的mesh的大小")]
        public float m_meshSize = 1.5f;
        [HideInInspector]
        [SerializeField]
        public float m_normalizedTime = 0;
        [HideInInspector]
        public bool m_isDirty = false;

        private Mesh m_mesh = null;
        private RangeRemap m_remapComp = null;

        private void Awake()
        {
            Debug.Log("MeshProceduralAnim:Awake");
            SetNormalizedTime(0);
            AnimSample();
        }

        private void OnEnable()
        {
            Debug.Log("MeshProceduralAnim:OnEnable");
        }

        // Start is called before the first frame update
        void Start()
        {
            Debug.Log("MeshProceduralAnim:Start");
        }

        private void OnValidate()
        {
            Debug.Log("MeshProceduralAnim:OnValidate");
            AnimSample();
        }

        // Update is called once per frame
        void Update()
        {
            if (m_isDirty)
            {
                AnimSample();
                m_isDirty = false;
            }
        }

        public void SetNormalizedTime(float normalizedTime)
        {
            m_normalizedTime = normalizedTime;
            if(m_remapComp == null)
            {
                var remapComp = GetComponent<RangeRemap>();
                m_remapComp = remapComp;
            }
            if (m_remapComp != null)
            {
                m_normalizedTime = m_remapComp.Remap(normalizedTime);
            }
            m_isDirty = true;
        }


        public void GenerateMesh()
        {
            //Resize the mesh.
            int n = 42;
            m_verticles = new Vector3[n * n];
            m_uvs = new Vector2[n * n];
            m_triangles = new int[(n - 1) * (n - 1) * 6];
            for (int j = 0; j < n; j++)
                for (int i = 0; i < n; i++)
                {
                    m_verticles[j * n + i] = new Vector3(m_meshSize / 2 - m_meshSize * i / (n - 1), 0, m_meshSize / 2 - m_meshSize * j / (n - 1));
                    m_uvs[j * n + i] = new Vector3(i / (n - 1.0f), j / (n - 1.0f));
                }
            int t = 0;
            for (int j = 0; j < n - 1; j++)
                for (int i = 0; i < n - 1; i++)
                {
                    m_triangles[t * 6 + 0] = j * n + i;
                    m_triangles[t * 6 + 1] = (j + 1) * n + i + 1;
                    m_triangles[t * 6 + 2] = j * n + i + 1;
                    m_triangles[t * 6 + 3] = j * n + i;
                    m_triangles[t * 6 + 4] = (j + 1) * n + i;
                    m_triangles[t * 6 + 5] = (j + 1) * n + i + 1;
                    t++;
                }
            if (m_mesh != null)
                return;
           
        } 

        private void SyncMesh()
        {
            if(m_mesh == null)
            {
                m_mesh = GetComponent<MeshFilter>().sharedMesh;
                if(m_mesh == null)
                {
                    m_mesh = new Mesh();
                    GetComponent<MeshFilter>().sharedMesh = m_mesh;
                }
            }
            m_mesh.vertices = m_verticles;
            m_mesh.triangles = m_triangles;
            m_mesh.uv = m_uvs;
            m_mesh.RecalculateNormals();
            m_mesh.RecalculateBounds();
            m_mesh.name = "GeneratedProceduralMesh";
            
        }

        public virtual float ProceduralSample(Vector2 coordinate, float normalizedTime)
        {
            var temp = 2.0f * normalizedTime - coordinate.x * coordinate.x - coordinate.y * coordinate.y;
            if (temp < 0)
                return 0;
            return Mathf.Sqrt(temp);
        }

        public void AnimSample()
        {
            //Debug.Log("MeshProceduralAnim:AnimSample");
            for (int i = 0; i < m_verticles.Length; i++)
            {
                var p = m_verticles[i];
                Vector2 coordinate = new Vector2(p.x, p.z);
                var value = ProceduralSample(coordinate, m_normalizedTime);
                m_verticles[i].y = value;
            }
            SyncMesh();
        }
    }
}
