using bluebean.UGFramework.Geometry;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ShapeOverlapTest : MonoBehaviour
{
    public SphereShapeDesc m_sphere;
    public MeshShapeDesc m_mesh;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(m_sphere!=null && m_mesh != null)
        {
            if (GeometryUtil.IsSphereMeshOverlap(m_sphere.Shape, m_mesh.Shape))
            {
                //Debug.Log("overlap");
                m_sphere.m_gizmonsColor = Color.red;
            }
            else
            {
                m_sphere.m_gizmonsColor = Color.blue;
            }
        }
        
    }
}
