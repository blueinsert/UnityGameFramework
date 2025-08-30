using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace bluebean.UGFramework.UI
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
        public UIDocument MainUIDocument{
            get{
                return m_mainUIDocument;
            }
        }
        
        [SerializeField] private UIDocument m_mainUIDocument;
        private bool m_isInitialized = false;
        #endregion

        #region Initialization
        private void Initialize()
        {
            if (m_isInitialized) return;
            m_mainUIDocument = GetComponent<UIDocument>();
            m_isInitialized = true;
            Debug.Log("UIDocumentManager initialized successfully");
        }
        #endregion

        public void SetContent(VisualTreeAsset visualTreeAsset){
            m_mainUIDocument.visualTreeAsset = visualTreeAsset;
        }

        public void ClearContent()
        {
            m_mainUIDocument.visualTreeAsset = null;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
