using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bluebean.UGFramework.UI;

namespace bluebean.UGFramework
{
    public class GameObjectPool
    {
        private GameObject m_prefab;
        private GameObject m_root;

        private int m_curIndex;

        private List<GameObject> m_instances = new List<GameObject>();

        public void Setup(GameObject prefab, GameObject root)
        {
            m_prefab = prefab;
            m_root = root;
        }

        public void Deactive()
        {
            foreach (var ins in m_instances)
            {
                ins.SetActive(false);
            }
            m_curIndex = 0;
        }

        public GameObject Allocate()
        {
            foreach (var instance in m_instances)
            {
                if (!instance.gameObject.activeSelf)
                {
                    instance.gameObject.SetActive(true);
                    return instance;
                }
            }

            GameObject go = GameObject.Instantiate(m_prefab);
            go.transform.SetParent(m_root.transform, false);
            m_instances.Add(go);
            go.SetActive(true);
            return go;
        }

    }

    public class GameObjectPool<T> where T : MonoViewController
    {
        private GameObject m_prefab;
        private GameObject m_root;

        private int m_curIndex;

        private List<T> m_instances = new List<T>();

        public void Setup(GameObject prefab, GameObject root)
        {
            m_prefab = prefab;
            m_root = root;
        }

        public void Deactive()
        {
            foreach (var ins in m_instances)
            {
                ins.gameObject.SetActive(false);
            }
            m_curIndex = 0;
        }

        public T Allocate(out bool isNew)
        {
            foreach (var instance in m_instances)
            {
                if (!instance.gameObject.activeSelf)
                {
                    isNew = false;
                    instance.gameObject.SetActive(true);
                    return instance;
                }
            }

            GameObject go = GameObject.Instantiate(m_prefab);
            go.transform.SetParent(m_root.transform, false);
            MonoViewController.AttachViewControllerToGameObject(go, "./", typeof(T).FullName, true);
            isNew = true;
            var ins = go.GetComponent<T>();
            m_instances.Add(ins);
            ins.gameObject.SetActive(true);
            return ins;
        }

        public T Allocate()
        {
            bool isNew;
            return this.Allocate(out isNew);
        }

    }

    public class GameObjectPool_Manual<T> where T : MonoViewController
    {
        private GameObject m_prefab;
        private GameObject m_root;

        private List<T> m_freeList = new List<T>();

        public void Setup(GameObject prefab, GameObject root)
        {
            m_prefab = prefab;
            m_root = root;
        }


        public T Allocate()
        {
            if (m_freeList.Count > 0)
            {
                var instance = m_freeList[0];
                m_freeList.RemoveAt(0);
                instance.gameObject.SetActive(true);
                return instance;
            }

            GameObject go = GameObject.Instantiate(m_prefab);
            go.transform.SetParent(m_root.transform, false);
            MonoViewController.AttachViewControllerToGameObject(go, "./", typeof(T).FullName, true);
            var ins = go.GetComponent<T>();
            ins.gameObject.SetActive(true);
            return ins;
        }

        public void Free(T instance)
        {
            instance.gameObject.SetActive(false);
            m_freeList.Add(instance);
        }

    }
}
