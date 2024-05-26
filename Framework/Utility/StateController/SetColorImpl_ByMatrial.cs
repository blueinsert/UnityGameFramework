using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework
{
    [RequireComponent(typeof(MeshRenderer))]
    public class SetColorImpl_ByMatrial : MonoBehaviour, ISetColor
    {
        private Material m_material = null;

        public Material Material
        {
            get
            {
                if(m_material == null)
                {
                    var meshRender = GetComponent<MeshRenderer>();
                    return meshRender.material;
                }
                return m_material;
            }set
            {
                var meshRender = GetComponent<MeshRenderer>();
                meshRender.material = value;
            }
        }
        public void SetColor(Color color)
        {
            var material = Material;
            if(material != null)
            {
                material.SetColor("_Color",color);
                Material = material;
            }
        }

        private void Awake()
        {
            
        }

    }
}
