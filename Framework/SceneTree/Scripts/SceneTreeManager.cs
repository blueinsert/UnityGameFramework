using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bluebean.UGFramework.UI;
using UnityEngine.Rendering.Universal;
using bluebean.UGFramework.Asset;

namespace bluebean.UGFramework
{
    public class SceneTreeManager : ITickable
    {
        private SceneTreeManager() { }
        private static SceneTreeManager m_instance;
        public static SceneTreeManager Instance { get { return m_instance; } }
        public static SceneTreeManager CreateInstance()
        {
            m_instance = new SceneTreeManager();
            return m_instance;
        }

        public GameObject SceneRoot { get; private set; }
        public GameObject ThreeDSceneRoot { get; private set; }
        public GameObject UISceneRoot { get; private set; }
        public GameObject UILayerRoot1 { get; private set; }
        public Canvas UILayerRoot1Canvas { get; private set; }
        public GameObject UILayerRoot2 { get; private set; }
        public Canvas UILayerRoot2Canvas { get; private set; }
        public GameObject LoadingLayerRoot { get; private set; }
        public GameObject UnusedLayerRoot { get; private set; }
        public GameObject PrefabThreeDSceneLayer { get; private set; }
        public GameObject PrefabUISceneLayer { get; private set; }

        private List<SceneLayer> m_loadingLayerList = new List<SceneLayer>();
        private List<SceneLayer> m_unusedLayerList = new List<SceneLayer>();
        private List<SceneLayer> m_usingLayerList = new List<SceneLayer>();

        private CoroutineScheduler m_coroutineManager = new CoroutineScheduler();

        private bool m_isDirty = false;

        public bool Initialize()
        {
            if (!CreateSceneRoot())
            {
                Debug.LogError("SceneManager:CreateSceneRoot Failed");
                return false;
            }
            return true;
        }

        public void Deinitialize()
        {
            //todo
        }

        private bool CreateSceneRoot()
        {
            var prefabSceneRoot = Resources.Load<GameObject>("SceneRoot");
            if (prefabSceneRoot == null)
            {
                Debug.LogError("Prefab of SceneRoot is null");
                return false;
            }
            SceneRoot = GameObject.Instantiate(prefabSceneRoot);
            SceneRoot.name = "SceneRoot";
            ThreeDSceneRoot = SceneRoot.transform.Find("3DSceneRoot").gameObject;
            UISceneRoot = SceneRoot.transform.Find("UISceneRoot").gameObject;
            UILayerRoot1 = SceneRoot.transform.Find("UISceneRoot/UILayerRoot1").gameObject;
            UILayerRoot1Canvas = UILayerRoot1.GetComponentInChildren<Canvas>();
            UILayerRoot2 = SceneRoot.transform.Find("UISceneRoot/UILayerRoot2").gameObject;
            UILayerRoot2Canvas = UILayerRoot2.GetComponentInChildren<Canvas>();
            LoadingLayerRoot = SceneRoot.transform.Find("LoadingLayerRoot").gameObject;
            UnusedLayerRoot = SceneRoot.transform.Find("UnusedLayerRoot").gameObject;
            PrefabThreeDSceneLayer = Resources.Load<GameObject>("ThreeDSceneLayer");
            if (PrefabThreeDSceneLayer == null)
            {
                Debug.LogError("Prefab of ThreeDSceneLayer is null");
                return false;
            }
            PrefabUISceneLayer = Resources.Load<GameObject>("UISceneLayer");
            if (PrefabUISceneLayer == null)
            {
                Debug.LogError("Prefab of UISceneLayer is null");
                return false;
            }
            //var clearCameraGo = GameObject.Find("ClearCamera");
            //if(clearCameraGo != null)
            //{
            //    m_clearCamera = clearCameraGo.GetComponent<Camera>();
            //}
            //if(m_clearCamera == null)
            //{
            //    Debug.LogError("the ClearCamera is not found");
            //    return false;
            //}
            //AddOverlayerCamera(UILayerRoot2Canvas.worldCamera);
            //AddOverlayerCamera(UILayerRoot1Canvas.worldCamera);
            //SetOverlayerCameraStack4Base(m_clearCamera);
            return true;
        }

        public void CreateLayer(SceneLayerType type, string layerName, string resPath, Action<SceneLayer> onComplete)
        {
            GameObject layerPrefab = null;
            switch (type)
            {
                case SceneLayerType.ThreeD:
                    layerPrefab = PrefabThreeDSceneLayer;
                    break;
                case SceneLayerType.UI:
                    layerPrefab = PrefabUISceneLayer;
                    break;
            }
            if (layerPrefab == null)
            {
                Debug.LogError("SceneManager:CreateLayer layerPrefab is null");
                onComplete(null);
            }
            var layerGo = GameObject.Instantiate<GameObject>(layerPrefab);
            var layer = layerGo.GetComponent<SceneLayer>();
            layer.SetName(layerName);
            layerGo.name = string.Format("{0}_LayerRoot", layerName);

            AddLayerToLoadingRoot(layer);

            var iter = AssetLoader.Instance.LoadAsset<GameObject>(resPath, (p, asset) =>
            {
                OnLoadLayerAssetComplete(layer, asset, onComplete);
            });
            m_coroutineManager.StartCorcoutine(iter);
        }

        private void OnLoadLayerAssetComplete(SceneLayer layer, GameObject asset, Action<SceneLayer> onComplete)
        {
            m_loadingLayerList.Remove(layer);
            if (asset != null)
            {
                var go = GameObject.Instantiate<GameObject>(asset);
                layer.AttachGameObject(go);
                AddLayerToUnsedRoot(layer);
                onComplete(layer);
            }
            else
            {
                FreeLayer(layer);
                onComplete(null);
            }
        }

        private void AddLayerToLoadingRoot(SceneLayer layer)
        {
            layer.m_state = SceneLayerState.Loading;
            m_loadingLayerList.Add(layer);

            layer.gameObject.SetActive(false);
            layer.transform.SetParent(LoadingLayerRoot.transform, false);
        }

        private void AddLayerToUnsedRoot(SceneLayer layer)
        {
            layer.m_state = SceneLayerState.Unused;
            m_unusedLayerList.Add(layer);

            layer.gameObject.SetActive(false);
            layer.transform.SetParent(UnusedLayerRoot.transform, false);
        }

        public void FreeLayer(SceneLayer layer)
        {
            m_unusedLayerList.Remove(layer);
            m_usingLayerList.Remove(layer);
            foreach (var viewCtrl in layer.gameObject.GetComponentsInChildren<MonoViewController>())
            {
                viewCtrl.OnDestry();
            }
            GameObject.Destroy(layer.gameObject);
            SetDirty();
        }

        public void PopLayer(SceneLayer layer)
        {
            if (layer.m_state != SceneLayerState.Using)
            {
                Debug.LogError("SceneManager:PopLayer but layer's state is not Using");
                return;
            }
            m_usingLayerList.Remove(layer);
            m_unusedLayerList.Add(layer);
            layer.m_state = SceneLayerState.Unused;
            foreach (var viewCtrl in layer.gameObject.GetComponentsInChildren<MonoViewController>())
            {
                viewCtrl.OnHide();
            }
            layer.transform.SetParent(UnusedLayerRoot.transform, false);
            layer.gameObject.SetActive(false);
            SetDirty();
        }

        public void PushLayer(SceneLayer layer)
        {
            if (layer.m_state != SceneLayerState.Unused)
            {
                Debug.LogError("SceneManager:PushLayer but layer's state is not Unused");
                return;
            }
            m_unusedLayerList.Remove(layer);
            m_usingLayerList.Add(layer);
            layer.m_state = SceneLayerState.Using;
            if (layer is UISceneLayer)
            {
                layer.transform.SetParent(UILayerRoot1Canvas.transform, false);
                SortUILayers();
            }
            else if (layer is ThreeDSceneLayer)
            {
                layer.transform.SetParent(ThreeDSceneRoot.transform, false);
            }
            layer.gameObject.SetActive(true);
            foreach (var viewCtrl in layer.gameObject.GetComponentsInChildren<MonoViewController>())
            {
                viewCtrl.OnShow();
            }
            SetDirty();
        }

        private void SetDirty()
        {
            m_isDirty = true;
        }

        private void ResetCameraDepth()
        {
            if (m_usingLayerList.Count < 2)
            {
                return;
            }
            lock (m_usingLayerList)
            {
                List<Camera> allCamera = new List<Camera>();
                List<SceneLayer> thirdDLayers = new List<SceneLayer>();
                List<SceneLayer> uiLayers = new List<SceneLayer>();
                for (int i = 0; i < m_usingLayerList.Count; i++)
                {
                    SceneLayer layer = m_usingLayerList[i];
                    if (layer is UISceneLayer)
                    {
                        uiLayers.Add(layer);
                    }
                    else
                    {
                        thirdDLayers.Add(layer);
                        allCamera.Add(layer.LayerCamera);
                    }
                }
                int depth = 0;
                foreach (var layer in thirdDLayers)
                {
                    layer.LayerCamera.depth = depth++;
                }
                List<Camera> uiCameras = new List<Camera>();
                foreach (var layer in uiLayers)
                {
                    if (!uiCameras.Contains(layer.LayerCamera))
                    {
                        uiCameras.Add(layer.LayerCamera);
                        allCamera.Add(layer.LayerCamera);
                    }
                }
                foreach (var uiCamera in uiCameras)
                {
                    uiCamera.depth = depth++;
                }

                SetCameraType(allCamera);
            }
        }

        private void SetCameraType(List<Camera> allCamera)
        {
            Camera mainCamera = null;
            int mainCameraIndex = -1;
            for(int i = 0; i < allCamera.Count; i++)
            {
                var camera = allCamera[i];
                if (camera != null)
                {
                    var cameraData = camera.GetUniversalAdditionalCameraData();
                    //if(cameraData.renderType == CameraRenderType.Base && camera.targetTexture==null)
                    if (camera.targetTexture == null)
                    {
                        mainCamera = camera;
                        mainCameraIndex = i;
                        break;
                    }
                }
            }
            if (mainCamera == null)
            {
                Debug.LogError("SetCameraType mainCamera==null");
                return;
            }
            var mainCameraData = mainCamera.GetUniversalAdditionalCameraData();
            var stack = mainCameraData.cameraStack;
            if (stack == null)
            {
                Debug.LogError("SetCameraType mainCamera's cameraStack == null");
                stack = new List<Camera>();
            }
            stack.Clear();
            for(int i = mainCameraIndex+1; i < allCamera.Count; i++)
            {
                var camera = allCamera[i];
                if (camera != null)
                {
                    var cameraData = camera.GetUniversalAdditionalCameraData();
                    if (cameraData.renderType == CameraRenderType.Base)
                    {
                        cameraData.renderType = CameraRenderType.Overlay;
                    }
                    if (camera.targetTexture == null)
                    {
                        stack.Add(allCamera[i]);
                    }
                }
            }
        }

        /// <summary>
        /// 排序uiLayers
        /// </summary>
        private void SortUILayers()
        {
            if (m_usingLayerList.Count < 2)
            {
                return;
            }
            //冒泡排序
            for (int i = m_usingLayerList.Count - 1; i >= 0; i--)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    var layer1 = m_usingLayerList[i];
                    var layer2 = m_usingLayerList[j];
                    var p1 = layer1.GetSortPriority();
                    var p2 = layer2.GetSortPriority();
                    if (p2 > p1)
                    {
                        m_usingLayerList[i] = layer2;
                        m_usingLayerList[j] = layer1;
                    }
                }
            }
            for (int i = 0; i < m_usingLayerList.Count; i++)
            {
                m_usingLayerList[i].transform.SetAsLastSibling();
            }
        }

        public void Tick()
        {
            m_coroutineManager.Tick();
            if (m_isDirty)
            {
                SortUILayers();
                ResetCameraDepth();
                m_isDirty = false;
            }
        }

    }
}
