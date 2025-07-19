using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace bluebean.Framework.UI
{
    /// <summary>
    /// UIDocument管理器，负责管理UI文档的显示内容
    /// 单例模式，提供全局访问点
    /// </summary>
    public class UIDocumentManager : MonoBehaviour
    {
        #region Singleton
        private static UIDocumentManager _instance;
        public static UIDocumentManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<UIDocumentManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("UIDocumentManager");
                        _instance = go.AddComponent<UIDocumentManager>();
                        go.AddComponent<UIDocument>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        #endregion

        #region Fields
        [SerializeField] private UIDocument mainUIDocument;
        private bool isInitialized = false;
        #endregion

        #region Events
        public System.Action<string> OnUIElementUpdated;
        public System.Action<string> OnUIDocumentLoaded;
        public System.Action<string> OnUIDocumentUnloaded;
        #endregion

        #region Initialization
        private void Initialize()
        {
            if (isInitialized) return;
            mainUIDocument = GetComponent<UIDocument>();
            isInitialized = true;
            Debug.Log("UIDocumentManager initialized successfully");
        }
        #endregion

        public void SetContent(VisualTreeAsset visualTreeAsset){
            mainUIDocument.visualTreeAsset = visualTreeAsset;
        }
        
        /// <summary>
        /// 获取UI元素
        /// </summary>
        /// <param name="elementName">元素名称</param>
        /// <returns>VisualElement</returns>
        public VisualElement GetUIElement(string elementName)
        {
            if (mainUIDocument == null || mainUIDocument.rootVisualElement == null)
            {
                Debug.LogError("UIDocument not found or not loaded");
                return null;
            }
            
            return mainUIDocument.rootVisualElement.Q(elementName);
        }
        
        /// <summary>
        /// 添加点击事件监听
        /// </summary>
        /// <param name="elementName">元素名称</param>
        /// <param name="callback">回调函数</param>
        public void AddClickEventListener(string elementName, System.Action callback)
        {
            VisualElement element = GetUIElement(elementName);
            if (element != null)
            {
                element.RegisterCallback<ClickEvent>(evt => callback?.Invoke());
            }
            else
            {
                Debug.LogWarning($"Element '{elementName}' not found for click event");
            }
        }
        
        /// <summary>
        /// 移除点击事件监听
        /// </summary>
        /// <param name="elementName">元素名称</param>
        /// <param name="callback">回调函数</param>
        public void RemoveClickEventListener(string elementName, System.Action callback)
        {
            VisualElement element = GetUIElement(elementName);
            if (element != null)
            {
                element.UnregisterCallback<ClickEvent>(evt => callback?.Invoke());
            }
        }

        #region Private Methods
        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
        #endregion
    }
}
