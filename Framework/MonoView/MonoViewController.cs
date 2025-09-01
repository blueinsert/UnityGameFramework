﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace bluebean.UGFramework
{
    public class MonoViewController : MonoBehaviour
    {
        public static MonoViewController AttachViewControllerToGameObject(GameObject root, string path, string typeFullName ,bool execAutoBind = false)
        {
            GameObject target = FindChild(root, path);
            Type type = ClassLoader.GetType(typeFullName);
            if(type == null)
            {
                Debug.LogError(string.Format("ClassLoader.GetType \"{0}\" is null", typeFullName));
            }
            MonoViewController viewController = null;
            if((viewController = target.GetComponent(type) as MonoViewController) != null)
            {
                return viewController;
            }
            else
            {
                viewController = target.AddComponent(type) as MonoViewController;
                if (execAutoBind)
                    viewController.AutoBindFields();
                return viewController;
            }
        }

        private static GameObject FindChild(GameObject root, string path)
        {
            GameObject target = null;
            int index = path.IndexOf("/");
            if (index != -1)
            {
                string subPath = path.Substring(index + 1);
                var targetTrans = root.transform.Find(subPath);
                if (targetTrans != null)
                {
                    target = targetTrans.gameObject;
                }
                if (target == null) {
                    Debug.LogError(string.Format("can't find bindPath {0} in {1}",path, root.name));
                }
            }
            return target;
        }

        public GameObject FindChild(string path)
        {
            return FindChild(this.gameObject, path);
        }

        public void AutoBindFields()
        {
            var type = this.GetType();
            foreach (var fieldInfo in type.GetFields(BindingFlags.NonPublic|BindingFlags.Public | BindingFlags.Instance))
            {
                var autoBindAttribute = fieldInfo.GetCustomAttribute<AutoBindAttribute>();
                if (autoBindAttribute != null)
                {
                    var obj = BindFiledImpl(fieldInfo.FieldType, autoBindAttribute.path);
                    if (obj != null)
                    {
                        fieldInfo.SetValue(this, obj);
                    }
                    else
                    {
                        Debug.LogError(string.Format("BindFiledImpl fail {0} path:{1}", type.Name, autoBindAttribute.path));
                    }
                }
            }
            OnBindFieldsComplete();
        }

        private UnityEngine.Object BindFiledImpl(Type espectType, string path)
        {
            GameObject target = FindChild(path);
            if(target == null)
            {
                Debug.LogError(string.Format("BindFiledImpl no such node,path:{0}", path));

                return null;
            }
            if (espectType == typeof(GameObject)){
                return target;
            }else if (espectType.IsSubclassOf(typeof(Component)))
            {
                var component = target.GetComponent(espectType);
                return component as UnityEngine.Object;
            }

            Debug.LogError(string.Format("BindFiledImpl no such type:{0}", espectType.Name.ToString()));

            return null;
        }

        protected virtual void OnBindFieldsComplete()
        {

        }

        public virtual void OnHide() { }

        public virtual void OnShow() { }

        public virtual void OnDestry() { }
    }
}
