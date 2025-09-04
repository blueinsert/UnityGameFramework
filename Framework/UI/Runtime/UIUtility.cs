using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.Reflection;

namespace bluebean.UGFramework.UI
{
    public partial class UIUtility
    {
        public const float PointerLongDownTime = 0.5f;
        public const float PointerClickTorrent = 0.02f;
        public const float PointerDragTorrent = 0.01f;

        public static Vector2 WorldToLocalPosition(Vector3 p, Camera worldCam, RectTransform rt, Camera uiCam)
        {
            return ScreenToLocalPosition(worldCam.WorldToScreenPoint(p), rt, uiCam);
        }

        public static Vector2 ScreenToLocalPosition(Vector2 p, RectTransform rt, Camera uiCam)
        {
            Vector2 lp = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, p, uiCam, out lp);
            return lp;
        }

        public static string AddColorTag(string txt, Color c)
        {
            //int ci = ((int)(c.r * 255) << 24) + ((int)(c.g * 255) << 16) + ((int)(c.b * 255) << 8) + (int)(c.a * 255);
            //return string.Format("<color=#{0,0:x8}>{1}</color>", ci, txt);
            return string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGBA(c), txt);
        }
        public static string AddSizeTag(string txt, int size)
        {
            return string.Format("<size={0}>{1}</size>", size, txt);
        }

        private static VisualElement BindUIDocumentFieldImpl(VisualElement uiInstance, Type espectType, string name)
        {
            VisualElement res = null;
            if (espectType == typeof(UnityEngine.UIElements.Button))
            {
                res = uiInstance.Q<UnityEngine.UIElements.Button>(name);
            }
            else if (espectType == typeof(UnityEngine.UIElements.Label))
            {
                res = uiInstance.Q<UnityEngine.UIElements.Label>(name);
            }
            else if (espectType == typeof(UnityEngine.UIElements.DropdownField))
            {
                res = uiInstance.Q<UnityEngine.UIElements.DropdownField>(name);
            }
            else if (espectType == typeof(UnityEngine.UIElements.TextField))
            {
                res = uiInstance.Q<UnityEngine.UIElements.TextField>(name);
            }
            else if (espectType == typeof(UnityEngine.UIElements.ListView))
            {
                res = uiInstance.Q<UnityEngine.UIElements.ListView>(name);
            }
            else if (espectType == typeof(UnityEngine.UIElements.ScrollView))
            {
                res = uiInstance.Q<UnityEngine.UIElements.ScrollView>(name);
            }
            else if (espectType == typeof(UnityEngine.UIElements.TemplateContainer))
            {
                res = uiInstance.Q<UnityEngine.UIElements.TemplateContainer>(name);
            }
            else if (espectType == typeof(UnityEngine.UIElements.VisualElement))
            {
                res = uiInstance.Q<UnityEngine.UIElements.VisualElement>(name);
            }
            else
            {
                Debug.LogError(string.Format("BindUIDocumentFieldImpl failed,type:{0} is not supported!", espectType.FullName));
            }
            return res;
        }

        public static void AttachUIController2UIDocument(UIViewController uiController, VisualElement uiDocumentRoot)
        {
            var type = uiController.GetType();
            foreach (var fieldInfo in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                var autoBindAttribute = fieldInfo.GetCustomAttribute<AutoBindDocumentAttribute>();
                if (autoBindAttribute != null)
                {
                    var obj = BindUIDocumentFieldImpl(uiDocumentRoot, fieldInfo.FieldType, autoBindAttribute.path);
                    if (obj != null)
                    {
                        fieldInfo.SetValue(uiController, obj);
                    }
                    else
                    {
                        Debug.LogError(string.Format("AttachUIController2UIDocument:BindFiledImpl fail {0} name:{1}", type.Name, autoBindAttribute.path));
                    }
                }
            }
            Debug.Log($"{uiController.GetType().Name} AttachUIController2UIDocument completed!");
        }

    }


}
