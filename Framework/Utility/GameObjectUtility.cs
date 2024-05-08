using bluebean.UGFramework.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework
{
    public class GameObjectUtility
    {
        public static void AttachUIController<T>(GameObject go) where T: UIViewController
        {
            MonoViewController.AttachViewControllerToGameObject(go, "./", typeof(T).FullName, true);
        }

        public static void SetLayer_R(GameObject go, int layer)
        {
            go.layer = layer;
            for (int i = 0; i < go.transform.childCount; ++i)
            {
                SetLayer_R(go.transform.GetChild(i).gameObject, layer);
            }
        }

        public static GameObject FindChildGameObject_R(GameObject go, string name)
        {
            if (go == null)
                return null;
            if (go.name == name)
                return go;

            for (int i = 0; i < go.transform.childCount; ++i)
            {
                GameObject c = FindChildGameObject_R(go.transform.GetChild(i).gameObject, name);
                if (c != null)
                    return c;
            }
            return null;
        }

        public static T FindComponentByName<T>(GameObject go, string name) where T : MonoBehaviour
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform childTransform = go.transform.GetChild(i);
                if (childTransform.name == name)
                {
                    return childTransform.GetComponent<T>();
                }
                else
                {
                    T component = FindComponentByName<T>(go.transform.GetChild(i).gameObject, name);
                    if (component != null) return component;
                }
            }
            return null;
        }

        public static void DestroyChildren(GameObject obj)
        {
            if (obj == null)
                return;

            var count = obj.transform.childCount;
            for (int i = 0; i < count; i++)
            {
                GameObject.Destroy(obj.transform.GetChild(i).gameObject);
            }
        }

        public static void DestroyChildrenImmediate(GameObject obj)
        {
            if (obj == null)
                return;

            List<GameObject> children = new List<GameObject>();

            for (int i = 0; i < obj.transform.childCount; i++)
            {
                children.Add(obj.transform.GetChild(i).gameObject);
            }

            for (int i = 0; i < children.Count; i++)
            {
                GameObject.DestroyImmediate(children[i]);
            }
        }
    }
}
