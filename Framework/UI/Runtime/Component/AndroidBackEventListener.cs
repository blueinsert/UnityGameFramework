using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace bluebean.Mugen3D.UI
{
    /// <summary>
    /// 监听android上的返回按钮事件
    /// </summary>
    [AddComponentMenu("UI/bluebean/AndroidBackEventListener")]
    public class AndroidBackEventListener : MonoBehaviour
    {
        void Start()
        {
        }

        void Update()
        {
#if UNITY_ANDROID || UNITY_STANDALONE_WIN
            // 返回键
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // 是否可以响应当前gameObject
                if (IsClickable(gameObject))
                {
                    Click(gameObject);
                }
                else
                {
                    Debug.Log("[AndroidBackEventListener] IsClickable = false, GameObject:" + gameObject.name);
                }
            }
#endif
        }

        /// <summary>
        /// 模拟点击
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        bool Click(GameObject o)
        {
            if (o == null)
                return false;
            Debug.Log("[AndroidBackEventListener] Click GameObject:" + o.name);
            List<Component> components = new List<Component>();
            o.GetComponents(typeof(MonoBehaviour), components);
            bool isProcessed = false;
            foreach (var c in components)
            {
                var pointerDownHandler = c as IPointerDownHandler;
                if (pointerDownHandler != null)
                {
                    pointerDownHandler.OnPointerDown(new PointerEventData(EventSystem.current));
                    isProcessed = true;
                }
                var pointerUpHandler = c as IPointerUpHandler;
                if (pointerUpHandler != null)
                {
                    pointerUpHandler.OnPointerUp(new PointerEventData(EventSystem.current));
                    isProcessed = true;
                }
                var clickHandler = c as IPointerClickHandler;
                if (clickHandler != null)
                {
                    clickHandler.OnPointerClick(new PointerEventData(EventSystem.current));
                    isProcessed = true;
                }
            }
            return isProcessed;
        }

        /// <summary>
        /// 是否可点击
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        bool IsClickable(GameObject obj)
        {
            if (obj == null)
                return false;
            PointerEventData p = new PointerEventData(EventSystem.current);
            Vector2 pos = Vector2.zero;
            if (SuitableClickPosition != pos)
            {
                if (!WorldPosToScreenPos(obj.transform as RectTransform, SuitableClickPosition, ref pos))
                    return false;
            }
            else
            {
                if (!GetCenterScreenPosition(obj.transform as RectTransform, ref pos))
                    return false;
            }
            if (!IsInsideScreen(pos))
                return false;
            p.position = pos;
            List<RaycastResult> result = new List<RaycastResult>();
            EventSystem.current.RaycastAll(p, result);
            if (result.Count != 0 && (result[0].gameObject == obj || IsChildTransform(obj.transform, result[0].gameObject.transform)))
                return true;
            return false;
        }

        /// <summary>
        /// ui坐标转屏幕坐标
        /// </summary>
        /// <param name="rt"></param>
        /// <param name="worldPos"></param>
        /// <param name="screenPos"></param>
        /// <returns></returns>
        bool WorldPosToScreenPos(RectTransform rt, Vector2 worldPos, ref Vector2 screenPos)
        {
            if (rt == null || rt.gameObject == null)
                return false;
            Canvas canvas = rt.gameObject.GetComponentInParent<Canvas>();
            if (canvas == null)
                return false;
            Camera uiCamera = canvas.worldCamera;
            if (uiCamera == null)
                return false;
            worldPos = rt.TransformPoint(worldPos);
            screenPos = uiCamera.WorldToScreenPoint(worldPos);
            return true;
        }

        /// <summary>
        /// 获取控件的屏幕坐标
        /// </summary>
        /// <param name="rt"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        bool GetCenterScreenPosition(RectTransform rt, ref Vector2 pos)
        {
            if (rt == null || rt.gameObject == null)
                return false;
            Canvas canvas = rt.gameObject.GetComponentInParent<Canvas>();
            if (canvas == null)
                return false;

            // UI元件坐标转换到屏幕坐标
            Vector3[] worldCorners = new Vector3[4];
            rt.GetWorldCorners(worldCorners);
            Camera uiCamera = canvas.worldCamera;
            if (uiCamera == null)
                return false;
            Vector2 p0 = uiCamera.WorldToScreenPoint(worldCorners[0]);
            Vector2 p2 = uiCamera.WorldToScreenPoint(worldCorners[2]);
            pos = (p0 + p2) * 0.5f;
            return true;
        }

        /// <summary>
        /// 是否在屏幕内
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        bool IsInsideScreen(Vector2 pos)
        {
            return pos.x >= 0 && pos.x <= Screen.width && pos.y >= 0 && pos.y <= Screen.height;
        }

        /// <summary>
        /// 是否是控件的子节点
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        /// <returns></returns>
        bool IsChildTransform(Transform parent, Transform child)
        {
            if (parent == null || child == null)
                return false;
            if (parent == child)
                return true;
            for (int i = 0; i < parent.childCount; i++)
                if (IsChildTransform(parent.GetChild(i), child))
                    return true;
            return false;
        }

        public Vector2 SuitableClickPosition;
    }

}