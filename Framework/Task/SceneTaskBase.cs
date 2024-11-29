using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bluebean.UGFramework;
using bluebean.UGFramework.Asset;
using bluebean.UGFramework.UI;

namespace bluebean.UGFramework
{
    /// <summary>
    /// 场景内容数据类
    /// </summary>
    public class SceneIntent
    {
        public string Name { get { return m_name; } }


        private string m_name;

        private readonly Dictionary<string, object> m_customParamDic = new Dictionary<string, object>();

        public SceneIntent(string name)
        {
            m_name = name;
        }

        public void SetCustomParam(string key, object value)
        {
            if (m_customParamDic.ContainsKey(key))
            {
                m_customParamDic[key] = value;
            }
            else
            {
                m_customParamDic.Add(key, value);
            }
        }

        public T GetCustomClassParam<T>(string key) where T : class
        {
            if (m_customParamDic.ContainsKey(key))
            {
                return m_customParamDic[key] as T;
            }
            return null;
        }

        public T GetCustomStructParam<T>(string key) where T : struct
        {
            if (m_customParamDic.ContainsKey(key))
            {
                return (T)m_customParamDic[key];
            }
            return default(T);
        }

        public void ClearCustomParam(string key)
        {
            if (m_customParamDic.ContainsKey(key))
            {
                m_customParamDic.Remove(key);
            }
        }

        public void ClearAllCustomParam()
        {
            m_customParamDic.Clear();
        }
    }

    /// <summary>
    /// 更新上下文，存放了更新过程相关的变量
    /// </summary>
    public class SceneTaskUpdateContext
    {
        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool m_isRuning;
        /// <summary>
        /// 是否是第一次执行
        /// </summary>
        public bool m_isInit;
        /// <summary>
        /// 是否从pause状态进入活跃状态
        /// </summary>
        public bool m_isResume;
        /// <summary>
        /// 视图更新完成时执行的回调
        /// </summary>
        public Action m_onViewUpdateComplete;
        /// <summary>
        /// 资源全部载入时执行的回调
        /// </summary>
        public Action m_redirectOnLoadAllAssetComplete;
        

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
            m_isRuning = false;
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
    /// 队列项
    /// </summary>
    public class PiplineQueueItem
    {
        public SceneIntent m_intent;
    }

    /// <summary>
    /// SceneTaskBase
    /// </summary>
    public abstract class SceneTaskBase : Task
    {
        #region 变量
        /// <summary>
        /// 层资源描述
        /// </summary>
        protected virtual LayerDesc[] LayerDescArray { get; set; }

        protected virtual ViewControllerDesc[] ViewControllerDescArray { get; set; }

        public SceneTaskUpdateContext UpdateCtx { get { return m_updateCtx; } }

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
        /// 更新上下文数据
        /// </summary>
        protected readonly SceneTaskUpdateContext m_updateCtx = new SceneTaskUpdateContext();

        private readonly List<string> m_assets = new List<string>();

        /// <summary>
        /// 资源数据字典
        /// </summary>
        protected readonly Dictionary<string, UnityEngine.Object> m_assetDic = new Dictionary<string, UnityEngine.Object>();

        /// <summary>
        /// Layer字典
        /// </summary>
        protected readonly Dictionary<string, SceneLayer> m_layerDic = new Dictionary<string, SceneLayer>();

        /// <summary>
        /// ViewCtrl数组
        /// </summary>
        protected MonoViewController[] m_viewControllerArray = null;

        /// <summary>
        /// 需要排队的更新请求
        /// </summary>
        protected List<PiplineQueueItem> m_piplineQueue = new List<PiplineQueueItem>();


        #endregion

        public SceneTaskBase(string name) : base(name) { }


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
                    SceneTreeManager.Instance.PopLayer(layer);
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
                    SceneTreeManager.Instance.FreeLayer(layer);
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

 

        #endregion

        #region Task重载方法

        protected sealed override bool OnStart(object param)
        {
            if (AssetUtility.Instance != null)
            {
                AssetUtility.Instance.RegisterDynamicAssetCache(m_assetDic);
                Debug.Log(string.Format("SceneTask {0}:RegisterDynamicAssetCache", GetType().Name));
            }

            var ok = OnStart(param as SceneIntent);
            return ok;
        }


        protected override void OnPause()
        {
            HideAllLayers();
        }

        protected override bool OnResume(object param)
        {
            return OnResume(param as SceneIntent);
        }


        protected override void OnStop()
        {
            if (AssetUtility.Instance != null)
            {
                AssetUtility.Instance.UnregisterDynamicAssetCache(m_assetDic);
                Debug.Log(string.Format("SceneTask {0}:UnregisterDynamicAssetCache", GetType().Name));
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

        #region 更新流程

        protected virtual bool OnStart(SceneIntent intent)
        {
            StartUpdatePipeline(intent);
            return true;
        }

        protected virtual bool OnResume(SceneIntent intent)
        {
            StartUpdatePipeline(intent);
            if (MainLayer != null && MainLayer.State == SceneLayerState.Unused)
                SceneTreeManager.Instance.PushLayer(MainLayer);
            return true;
        }

        public void OnNewIntent(SceneIntent intent)
        {
            StartUpdatePipeline(intent);
        }

        /// <summary>
        /// 在启动之前，准备需要耗时的数据准备，比如从网络上拉取
        /// </summary>
        /// <param name="onPrepareEnd"></param>
        public virtual void PrapareDataForStart(Action<bool> onPrepareEnd)
        {
            onPrepareEnd(true);
        }

        /// <summary>
        /// 管线是否还在运行
        /// </summary>
        /// <returns></returns>
        protected bool IsPipeLineRunning()
        {
            return (m_updateCtx != null && m_updateCtx.m_isRuning) || m_piplineQueue.Count != 0;
        }

        /// <summary>
        /// 启动全量更新流程
        /// </summary>
        /// <param name="intent"></param>
        protected virtual void StartUpdatePipeline(SceneIntent intent)
        {
            if (m_updateCtx.m_isRuning)
            {
                m_piplineQueue.Add(new PiplineQueueItem() { m_intent = intent });
                return;
            }

            m_updateCtx.m_isRuning = true;

            bool isNeedUpdateCache = IsNeedUpdateCache();
            if (isNeedUpdateCache)
            {
                UpdateCache();
            }
            bool isNeedLoadLayer = IsNeedLoadLayer();
            bool isNeedLoadAssets = IsNeedLoadAssets();
            if (isNeedLoadLayer || isNeedLoadAssets)
            {
                bool isLoadLayerComplete = !isNeedLoadLayer;
                bool isLoadAssetsComplete = !isNeedLoadAssets;
                if (isNeedLoadLayer)
                {
                    LoadLayer(() =>
                    {
                        isLoadLayerComplete = true;
                        if (isLoadLayerComplete && isLoadAssetsComplete)
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
                        if (isLoadLayerComplete && isLoadAssetsComplete)
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
        /// 是否需要加载显示层
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
            int toLoadLayerNum = toLoadLayerDescs.Count;
            int loadCompleteCount = 0;
            foreach (var layerDesc in toLoadLayerDescs)
            {
                SceneTreeManager.Instance.CreateLayer(layerDesc.IsUILayer?SceneLayerType.UI:SceneLayerType.ThreeD, layerDesc.LayerName, layerDesc.AssetPath, (layer) =>
                {
                    m_layerDic.Add(layerDesc.LayerName, layer);
                    loadCompleteCount++;
                    if (loadCompleteCount == toLoadLayerNum)
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
                Dictionary<string, UnityEngine.Object> tempAssetDic = new Dictionary<string, UnityEngine.Object>();
                //todo task感知加载进度
                AssetLoader.Instance.StartLoadAssetCoroutine<UnityEngine.Object>(realAssetPaths, tempAssetDic, () =>
                {
                    foreach (var pair in tempAssetDic)
                    {
                        Debug.Log(string.Format("load asset success, path:{0}", pair.Key));
                        if(!m_assetDic.ContainsKey(pair.Key))
                            m_assetDic.Add(pair.Key, pair.Value);
                    }
                    onComplete();
                });
            }
        }

        protected virtual void OnAllViewControllerCreateCompleted()
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
            foreach (var viewController in m_viewControllerArray)
            {
                viewController.AutoBindFields();
            }
            OnAllViewControllerCreateCompleted();
        }

        /// <summary>
        /// 加载所有资源完成
        /// </summary>
        private void OnLoadLayersAndAssetsComplete()
        {
            if (m_updateCtx.m_isInit)
            {
                foreach(var layer in m_layerDic.Values)
                    SceneTreeManager.Instance.PushLayer(layer);
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
        /// 更新视图
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
        /// 更新视图
        /// </summary>
        protected virtual void UpdateView()
        {

        }

        /// <summary>
        /// 更新视图
        /// </summary>
        protected virtual void PostUpdateView()
        {
            if (m_piplineQueue.Count != 0)
            {
                var item = m_piplineQueue[0];
                m_piplineQueue.RemoveAt(0);
                StartUpdatePipeline(item.m_intent);
            }
        }

        #endregion

        public T GetAsset<T>(string path) where T : UnityEngine.Object
        {
            return AssetUtility.Instance.GetAsset<T>(path);
        }

        protected void CollectAsset(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                Debug.Log("SceneTaskBase:CollectAsset string.IsNullOrEmpty(assetName)");
                return;
            }
            AssetUtility.AddAssetToList(AssetUtility.MakeAssetPath(assetName), m_assets);
        }

        protected void CollectSpriteAsset(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                Debug.Log("SceneTaskBase:CollectSpriteAsset string.IsNullOrEmpty(assetName)");
                return;
            }
            AssetUtility.AddAssetToList(AssetUtility.MakeSpriteAssetPath(assetName), m_assets);
        }

        protected void ClearAssets()
        {
            m_assets.Clear();
        }
    }
}
