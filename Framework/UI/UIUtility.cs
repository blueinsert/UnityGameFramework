using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;

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

    }


}
