using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bluebean.UGFramework;
using bluebean.UGFramework.Asset;

namespace bluebean.UGFramework.UI
{
    /// <summary>
    /// UITask更新上下文，存放了更新过程相关的变量
    /// </summary>
    public class UITaskUpdateContext
    {
        /// <summary>
        /// 视图更新完成时执行的回调
        /// </summary>
        public Action m_onViewUpdateComplete;
        /// <summary>
        /// 资源全部载入时执行的回调
        /// </summary>
        public Action m_redirectOnLoadAllAssetComplete;
        /// <summary>
        /// 是否是第一次执行
        /// </summary>
        public bool m_isInit;
        /// <summary>
        /// 是否从pause状态进入活跃状态
        /// </summary>
        public bool m_isResume;

        public void SetRedirectOnLoadAssetCompleteCb(Action action)
        {
            m_redirectOnLoadAllAssetComplete = action;
        }

        public void SetViewUpdateCompleteCb(Action action)
        {
            m_onViewUpdateComplete = action;
        }

        public void Clear()
        {
            m_onViewUpdateComplete = null;
            m_redirectOnLoadAllAssetComplete = null;
            m_isInit = false;
            m_isResume = false;
        }
    }

    /// <summary>
    /// Layer资源描述，每个层资源是加载在UILayer或3DLayer节点上的实例化prefab
    /// </summary>
    public class LayerDesc
    {
        public string LayerName;
        public string AssetPath;
        public bool IsUILayer = true;//如果不是UILayer就是3DLayer
    }

    /// <summary>
    /// ViewCtrl资源描述, 在特定层路径节点上创建并绑定的MonoViewController
    /// </summary>
    public class ViewControllerDesc
    {
        public string AtachLayerName { get; set; }
        public string AtachPath { get; set; }
        public string TypeFullName { get; set; }
    }

    /// <summary>
    /// UITask
    /// </summary>
    public abstract class UITaskBase : Task
    {
        #region 变量
        /// <summary>
        /// 层资源描述
        /// </summary>
        protected virtual LayerDesc[] LayerDescArray { get; set; }

        protected virtual ViewControllerDesc[] ViewControllerDescArray { get; set; }

        public UITaskUpdateContext UpdateCtx { get { return m_updateCtx; } }

        public UIIntent CurUIIntent { get { return m_curUIIntent; } }

        private SceneLayer MainLayer
        {
            get
            {
                if (LayerDescArray == null || LayerDescArray.Length == 0 || m_layerDic.Count == 0)
                {
                    return null;
                }
                return m_layerDic[LayerDescArray[0].LayerName];
            }
        }

        /// <summary>
        /// 存放内容数据
        /// </summary>
        protected UIIntent m_curUIIntent;

        /// <summary>
        /// 更新上下文数据
        /// </summary>
        protected readonly UITaskUpdateContext m_updateCtx = new UITaskUpdateContext();

        private readonly List<string> m_assets = new List<string>();

        /// <summary>
        /// 资源数据字典
        /// </summary>
        protected readonly Dictionary<string, UnityEngine.Object> m_assetDic = new Dictionary<string, UnityEngine.Object>();

        /// <summary>
        /// UILayer字典
        /// </summary>
        protected readonly Dictionary<string, SceneLayer> m_layerDic = new Dictionary<string, SceneLayer>();

        /// <summary>
        /// ViewCtrl数组
        /// </summary>
        protected MonoViewController[] m_viewControllerArray = null;

        #endregion

        public UITaskBase(string name) : base(name) { }


        #region 内部方法

        /// <summary>
        /// 隐藏所有显示层
        /// </summary>
        private void HideAllLayers()
        {
            foreach (var layer in m_layerDic.Values)
            {
                if (layer != null && layer.m_state == SceneLayerState.Using)
                {
                    SceneTree.Instance.PopLayer(layer);
                }
            }
        }

        /// <summary>
        /// 销毁所有显示层
        /// </summary>
        private void DestroyAllLayers()
        {
            foreach (var layer in m_layerDic.Values)
            {
                if (layer != null)
                    SceneTree.Instance.FreeLayer(layer);
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 从重定向中返回，一般在资源加载完成后调用
        /// </summary>
        public void ReturnFromRedirect()
        {
            StartUpdateView();
        }

        public void OnNewIntent(UIIntent intent)
        {
            StartUpdateUITask(intent);
        }

        public void CloseAndReturn(Action<bool> onFinish = null, bool stay4Reuse = false)
        {
            UIManager.Instance.CloseAndReturn(CurUIIntent, onFinish, stay4Reuse);
        }

        #endregion

        #region Task重载方法

        protected sealed override bool OnStart(object param)
        {
            if (AssetUtility.Instance != null)
            {
                AssetUtility.Instance.RegisterDynamicAssetCache(m_assetDic);
                Debug.Log(string.Format("UITask {0}:RegisterDynamicAssetCache", GetType().Name));
            }

            var ok = OnStart(param as UIIntent);
            Debug.Log("UITask.OnStart: " + GetType().Name);
            return ok;
        }

        protected virtual bool OnStart(UIIntent intent)
        {
            StartUpdateUITask(intent);
            return true;
        }

        protected override void OnPause()
        {
            HideAllLayers();
        }

        protected override bool OnResume(object param)
        {
            return OnResume(param as UIIntent);
        }

        protected virtual bool OnResume(UIIntent intent)
        {
            StartUpdateUITask(intent);
            if (MainLayer != null && MainLayer.State == SceneLayerState.Unused)
                SceneTree.Instance.PushLayer(MainLayer);
            return true;
        }

        protected override void OnStop()
        {
            if (AssetUtility.Instance != null)
            {
                AssetUtility.Instance.UnregisterDynamicAssetCache(m_assetDic);
                Debug.Log(string.Format("UITask {0}:UnregisterDynamicAssetCache", GetType().Name));
            }
            DestroyAllLayers();
            m_assetDic.Clear();
            m_layerDic.Clear();
            OnAllAssetClear();
        }

        protected virtual void OnAllAssetClear()
        {

        }

        #endregion

        #region UI更新流程

        /// <summary>
        /// 在启动之前，准备需要耗时的数据准备，比如从网络上拉取
        /// </summary>
        /// <param name="onPrepareEnd"></param>
        public virtual void PrapareDataForStart(Action<bool> onPrepareEnd)
        {
            onPrepareEnd(true);
        }

        protected virtual void OnIntentChange(UIIntent prevIntent, UIIntent curIntent)
        {

        }

        /// <summary>
        /// 更新UITask
        /// </summary>
        /// <param name="intent"></param>
        protected void StartUpdateUITask(UIIntent intent = null)
        {
            if (intent != null)
            {
                OnIntentChange(m_curUIIntent, intent);
                m_curUIIntent = intent;
            }
            bool isNeedUpdateCache = IsNeedUpdateCache();
            if (isNeedUpdateCache)
            {
                UpdateCache();
            }
            bool isNeedLoadLayer = IsNeedLoadLayer();
            bool isNeedLoadAssets = IsNeedLoadAssets();
            if (isNeedLoadLayer || isNeedLoadAssets)
            {
                bool isLoadUILayerComplete = !isNeedLoadLayer;
                bool isLoadAssetsComplete = !isNeedLoadAssets;
                if (isNeedLoadLayer)
                {
                    LoadLayer(() =>
                    {
                        isLoadUILayerComplete = true;
                        if (isLoadUILayerComplete && isLoadAssetsComplete)
                        {
                            OnLoadLayersAndAssetsComplete();
                        }
                    });
                }
                if (isNeedLoadAssets)
                {
                    LoadAssets(() =>
                    {
                        isLoadAssetsComplete = true;
                        if (isLoadUILayerComplete && isLoadAssetsComplete)
                        {
                            OnLoadLayersAndAssetsComplete();
                        }
                    });
                }
            }
            else
            {
                OnLoadLayersAndAssetsComplete();
            }
        }

        /// <summary>
        /// 是否需要更新缓存
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsNeedUpdateCache()
        {
            return false;
        }

        /// <summary>
        /// 更新缓存
        /// </summary>
        protected virtual void UpdateCache()
        {

        }

        /// <summary>
        /// 是否需要加载UI显示层
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsNeedLoadLayer()
        {
            return m_updateCtx.m_isInit;
        }

        /// <summary>
        /// 加载所有层资源
        /// </summary>
        /// <param name="onComplete"></param>
        protected virtual void LoadLayer(Action onComplete)
        {
            var layerDescs = LayerDescArray;
            if (layerDescs == null || layerDescs.Length == 0)
            {
                onComplete();
            }
            List<LayerDesc> toLoadLayerDescs = new List<LayerDesc>();
            foreach (var layerDesc in layerDescs)
            {
                if (!m_layerDic.ContainsKey(layerDesc.LayerName))
                {
                    toLoadLayerDescs.Add(layerDesc);
                }
            }
            if (toLoadLayerDescs.Count == 0)
            {
                onComplete();
            }
            int toLoadUILayerNum = toLoadLayerDescs.Count;
            int loadCompleteCount = 0;
            foreach (var layerDesc in toLoadLayerDescs)
            {
                SceneTree.Instance.CreateLayer(layerDesc.IsUILayer?SceneLayerType.UI:SceneLayerType.ThreeD, layerDesc.LayerName, layerDesc.AssetPath, (layer) =>
                {
                    m_layerDic.Add(layerDesc.LayerName, layer);
                    loadCompleteCount++;
                    if (loadCompleteCount == toLoadUILayerNum)
                    {
                        onComplete();
                    }
                });
            }
        }

        /// <summary>
        /// 是否需要加载资源
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsNeedLoadAssets()
        {
            return false;
        }

        /// <summary>
        /// 收集资源路径List
        /// </summary>
        /// <returns></returns>
        protected virtual bool CollectAssetPathsToLoad()
        {
            return m_assets.Count!=0;
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="onComplete"></param>
        private void LoadAssets(Action onComplete)
        {
            CollectAssetPathsToLoad();
            var assetPaths = m_assets;
            //去除重复资源和已加载资源
            List<string> realAssetPaths = new List<string>();
            foreach (string path in assetPaths)
            {
                if (!m_assetDic.ContainsKey(path) && !realAssetPaths.Contains(path))
                {
                    realAssetPaths.Add(path);
                }
            }
            if (realAssetPaths.Count == 0)
            {
                onComplete();
            }
            else
            {
                //todo uitask感知加载进度
                AssetLoader.Instance.StartLoadAssetCoroutine<UnityEngine.Object>(realAssetPaths, (assetDic) =>
                {
                    foreach (var pair in assetDic)
                    {
                        Debug.Log(string.Format("load asset success, path:{0}", pair.Key));
                        m_assetDic.Add(pair.Key, pair.Value);
                    }
                    onComplete();
                });
            }
        }

        protected virtual void OnCreateAllUIViewController()
        {

        }

        /// <summary>
        /// 创建视图控制器
        /// </summary>
        private void CreateAllViewController()
        {
            if (ViewControllerDescArray.Length <= 0)
            {
                return;
            }
            m_viewControllerArray = new MonoViewController[ViewControllerDescArray.Length];
            for (int i = 0; i < ViewControllerDescArray.Length; i++)
            {
                var viewControllerDesc = ViewControllerDescArray[i];
                var sceneLayer = m_layerDic[viewControllerDesc.AtachLayerName];
                var viewController = MonoViewController.AttachViewControllerToGameObject(sceneLayer.PrefabInstance, viewControllerDesc.AtachPath, viewControllerDesc.TypeFullName);
                m_viewControllerArray[i] = viewController;
            }
            foreach (var uiViewController in m_viewControllerArray)
            {
                uiViewController.AutoBindFields();
            }
            OnCreateAllUIViewController();
        }

        /// <summary>
        /// 加载所有资源完成
        /// </summary>
        private void OnLoadLayersAndAssetsComplete()
        {
            if (m_updateCtx.m_isInit)
            {
                foreach(var layer in m_layerDic.Values)
                    SceneTree.Instance.PushLayer(layer);
            }
            if (m_viewControllerArray == null)
            {
                CreateAllViewController();
            }
            if (m_updateCtx.m_redirectOnLoadAllAssetComplete != null)
            {
                m_updateCtx.m_redirectOnLoadAllAssetComplete();
                m_updateCtx.m_redirectOnLoadAllAssetComplete = null;
                return;
            }
            StartUpdateView();
        }

        /// <summary>
        /// 更新UI视图
        /// </summary>
        protected void StartUpdateView()
        {
            UpdateView();
            if (m_updateCtx.m_onViewUpdateComplete != null)
            {
                m_updateCtx.m_onViewUpdateComplete();
            }
            m_updateCtx.Clear();
        }

        /// <summary>
        /// 更新UI视图
        /// </summary>
        protected virtual void UpdateView()
        {

        }

        #endregion

        public T GetAsset<T>(string path) where T : UnityEngine.Object
        {
            return AssetUtility.Instance.GetAsset<T>(path);
        }

        protected void CollectAsset(string assetName)
        {
            AssetUtility.AddAssetToList(assetName, m_assets);
        }

        protected void CollectSpriteAsset(string assetName)
        {
            AssetUtility.AddSpriteAssetToList(assetName, m_assets);
        }

        protected void ClearAssets()
        {
            m_assets.Clear();
        }
    }
}
