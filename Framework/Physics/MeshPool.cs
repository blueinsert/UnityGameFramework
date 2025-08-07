using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework
{
    public class MeshResource
    {
        public Mesh m_mesh;
        public bool m_isInUsing;
    }

    public class MeshPool
    {
        public readonly List<MeshResource> m_pool = new List<MeshResource>();

        public MeshResource Allocate()
        {
            for(int i=0; i<m_pool.Count; ++i)
            {
                MeshResource resource = m_pool[i];
                if(resource!=null && !resource.m_isInUsing)
                {
                    resource.m_isInUsing = true;
                    return resource;
                }
            }
            MeshResource newResource = new MeshResource()
            {
                m_isInUsing = true,
                m_mesh = new Mesh(),
            };
            m_pool.Add(newResource);
            return newResource;

        }

        public void BackAll()
        {
            foreach(var item in m_pool)
            {
                item.m_isInUsing = false;
            }
        }

        public void Destroy()
        {
            foreach(var item in m_pool)
            {
                GameObject.DestroyImmediate(item.m_mesh);
            }
            m_pool.Clear();
        }
    }
}
