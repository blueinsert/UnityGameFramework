using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.Anim
{
    [RequireComponent(typeof(MeshFilter))]
    public class MeshProceduralAnim : MonoBehaviour
    {
        [HideInInspector]
        public Vector3[] X;
        //[HideInInspector]
        [Header("产生的mesh的大小")]
        public float m_meshSize = 1.5f;
        [HideInInspector]
        public float m_normalizedTime = 0;
        [HideInInspector]
        public bool m_isDirty = false;

        private RangeRemap m_remapComp = null;

        // Start is called before the first frame update
        void Start()
        {
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
            Mesh mesh = new Mesh();

            //Resize the mesh.
            int n = 42;
            X = new Vector3[n * n];
            Vector2[] UV = new Vector2[n * n];
            int[] T = new int[(n - 1) * (n - 1) * 6];
            for (int j = 0; j < n; j++)
                for (int i = 0; i < n; i++)
                {
                    X[j * n + i] = new Vector3(m_meshSize / 2 - m_meshSize * i / (n - 1), 0, m_meshSize / 2 - m_meshSize * j / (n - 1));
                    UV[j * n + i] = new Vector3(i / (n - 1.0f), j / (n - 1.0f));
                }
            int t = 0;
            for (int j = 0; j < n - 1; j++)
                for (int i = 0; i < n - 1; i++)
                {
                    T[t * 6 + 0] = j * n + i;
                    T[t * 6 + 1] = (j + 1) * n + i + 1;
                    T[t * 6 + 2] = j * n + i + 1;
                    T[t * 6 + 3] = j * n + i;
                    T[t * 6 + 4] = (j + 1) * n + i;
                    T[t * 6 + 5] = (j + 1) * n + i + 1;
                    t++;
                }
            mesh.vertices = X;
            mesh.triangles = T;
            mesh.uv = UV;
            mesh.RecalculateNormals();

            GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        private void SyncMesh()
        {
            Mesh mesh = GetComponent<MeshFilter>().mesh;
            mesh.vertices = X;
            mesh.RecalculateNormals();
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
            for (int i = 0; i < X.Length; i++)
            {
                var p = X[i];
                Vector2 coordinate = new Vector2(p.x, p.z);
                var value = ProceduralSample(coordinate, m_normalizedTime);
                X[i].y = value;
            }
            SyncMesh();
        }
    }
}
