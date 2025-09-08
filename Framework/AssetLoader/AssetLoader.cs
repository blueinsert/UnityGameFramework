using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.Asset
{

    public partial class AssetLoader : ITickable
    {

        #region 单例模式
        private AssetLoader() { }
        private static AssetLoader m_instance;
        public static AssetLoader Instance
        {
            get
            {
                return m_instance;
            }
        }
        public static AssetLoader CreateInstance()
        {
            m_instance = new AssetLoader();
            return m_instance;
        }
        #endregion

        private readonly CoroutineScheduler m_coroutineManager = new CoroutineScheduler();

        public bool Initialize()
        {
            return true;
        }

        #region For Editor
        private void LoadAssetByAssetDatabase(string path, bool hasSubAsset, Action<UnityEngine.Object[]> onEnd)
        {
#if UNITY_EDITOR
            UnityEngine.Object[] assets;
            if (hasSubAsset)
            {
                assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
            }
            else
            {
                assets = new UnityEngine.Object[1];
                assets[0] = UnityEditor.AssetDatabase.LoadAssetAtPath(path,typeof(UnityEngine.Object));
            }
            
            if(assets != null && assets.Length>0 && assets[0] != null)
            {
                Debug.Log("Load Asset Success by AssetDataBase AssetPath:" + path);
            }
            else
            {
                Debug.LogError("Load Asset Fail by AssetDataBase AssetPath:" + path);
            }
            onEnd(assets);
#endif
        }
        #endregion
         
        #region Resources
        private IEnumerator LoadAssetByResourceLoad(string path, bool hasSubAsset, Action<UnityEngine.Object[]> onEnd)
        {
            //从全路径中解析出相对Resources的路径
            string pathInResoruces = PathUtility.GetSubPathInResources(path);
            //去掉文件扩展名
            pathInResoruces = PathUtility.RemoveExtension(pathInResoruces);
            UnityEngine.Object[] assets;
            if (hasSubAsset)
            {
                assets = Resources.LoadAll(pathInResoruces);
            }
            else
            {
                assets = new UnityEngine.Object[1];
                assets[0] = Resources.Load(pathInResoruces);
            }
            yield return null;
            if (assets != null && assets[0] != null)
            {
                //Debug.Log("Load Asset Success by ResourceLoad AssetPath:" + path);
            }
            onEnd(assets);
        }
        #endregion

        #region 外部接口
        public IEnumerator LoadAsset<T>(string path, Action<string, T> onEnd) where T : UnityEngine.Object
        {
            AppClientSetting clientSetting = AppClientSetting.Instance;
            UnityEngine.Object[] assets = null;
            bool hasSubAsset = false;
            string mainAssetPath, subAssetPath = "";
            int atIndex = path.IndexOf("@");
            if (atIndex != -1)
            {
                mainAssetPath = path.Substring(0, atIndex);
                subAssetPath = path.Substring(atIndex + 1);
                hasSubAsset = true;
            }
            else
            {
                mainAssetPath = path;
            }
            bool isUseResources = path.Contains("Resources/");
            if(((clientSetting.m_useAssetBundleInEditor && Application.isEditor)
                || !Application.isEditor) && !isUseResources)
            {
                var iter = LoadAssetFromBundle(mainAssetPath, (lassets) => { assets = lassets; });
                while (iter.MoveNext())
                {
                    yield return null;
                }
            }
            //3.使用assetDataBase加载
            if (!clientSetting.m_useAssetBundleInEditor && Application.isEditor && assets == null)
            {
                LoadAssetByAssetDatabase(mainAssetPath, hasSubAsset, (loadAssets) => { assets = loadAssets; });
            }
            //4.使用resource加载
            if (isUseResources || assets == null)
            {
                yield return LoadAssetByResourceLoad(mainAssetPath, hasSubAsset, (loadAssets) => { assets = loadAssets; });
            }
            LoadEnd:
            UnityEngine.Object asset = null;
            if (hasSubAsset)
            {
                if(assets[0].GetType() == typeof(Texture2D) || assets[0].GetType() == typeof(Sprite))
                {
                    foreach (var obj in assets)
                    {
                        if (obj.name == subAssetPath && obj.GetType() == typeof(Sprite))
                        {
                            asset = obj;
                            break;
                        }
                    }
                }
                
            }
            else
            {
                asset = assets[0];
            }
            onEnd(path, asset as T);
        }

        private IEnumerator AssetLoadSubCoroutine<T>(int subCoroutineCount, int subCoroutineIndex, List<string> assetPaths, Dictionary<string, T> assetDic, Action onComplete) where T : UnityEngine.Object
        {
            for (int i = 0; i < assetPaths.Count; i++)
            {
                if ((i % subCoroutineCount) == subCoroutineIndex)
                {
                    var coroutine = LoadAsset<T>(assetPaths[i], (p, asset) =>
                    {
                        assetDic.Add(p, asset);
                    });
                    yield return coroutine;
                }
            }
            onComplete();
        }

        public void StartLoadAssetCoroutine<T>(List<string> assetPaths, Dictionary<string, T> assetDic, Action onComplete) where T : UnityEngine.Object
        {
            int subCoroutineNum = 3;
            int subCoroutineCompleteCount = 0;
            for(int i = 0; i < subCoroutineNum; i++)
            {
                m_coroutineManager.StartCorcoutine(AssetLoadSubCoroutine(subCoroutineNum, i, assetPaths, assetDic, ()=> {
                    subCoroutineCompleteCount++;
                    if(subCoroutineCompleteCount == subCoroutineNum)
                    {
                        onComplete?.Invoke();
                    }
                }));
            }
            
        }

        public IEnumerator StartAssetBundleManifestLoading(Action<bool> onEnd)
        {
            m_assetBundleManifest = null;
            var iter = AssetBundleManifestLoadingWorker((res) => {
                onEnd(res);
            });
            while (iter.MoveNext())
            {
                yield return null;
            }
        }

        public bool StartLoadingBundleData()
        {
            m_bundleData = null;
            m_bundleDataHelper = null;
            return LoadingBundleData();
        }
            #endregion

            public void Tick()
        {
            m_coroutineManager.Tick();
        }
    }
}

