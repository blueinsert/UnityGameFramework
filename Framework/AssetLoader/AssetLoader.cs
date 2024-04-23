using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework
{
    public class BundleLoadingCtx
    {
        public string m_bundleName;
        //public AssetBundle m_bundle;
        public bool m_isEnd;
        public int m_ref;
    }

    public class BundleCacheItem
    {
        public string m_bundleName;
        public AssetBundle m_bundle;                // 被缓存的bundle
    }

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

        private BundleData m_bundleData;
        private BundleDataHelper m_bundleDataHelper;
        /// <summary>
        /// bundle的缓存
        /// </summary>
        protected Dictionary<string, BundleCacheItem> m_bundleCacheDict = new Dictionary<string, BundleCacheItem>();
        
        protected Dictionary<string, BundleLoadingCtx> m_bundleLoadingCtxDict = new Dictionary<string, BundleLoadingCtx>();

        public bool Initialize()
        {
            GameClientSetting clientSetting = GameClientSetting.Instance;
            if ((clientSetting.m_useAssetBundleInEditor && Application.isEditor)
               || !Application.isEditor)
            {
                var path = AssetPathHelper.GetBundleDataPathInResources();
                var bundleData = Resources.Load<BundleData>(path);
                if (bundleData == null)
                {
                    Debug.LogError(string.Format("AssetLoader:LoadBundleData failed,path:{0}", path));
                    return false;
                }
                Debug.Log("AssetLoader:LoadBundleData Success");
                m_bundleData = bundleData;
                m_bundleDataHelper = new BundleDataHelper(m_bundleData, true);
            } 
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
            
            if(assets != null && assets[0] != null)
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

        #region AssetBundle
        /// <summary>
        /// 将bundle缓存到cache中
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="bundle"></param>
        protected BundleCacheItem PushAssetBundleToCache(string bundleName, AssetBundle bundle)
        {   
            BundleCacheItem item;
            bool ret = m_bundleCacheDict.TryGetValue(bundleName, out item);
            if (!ret)
            {
                item = new BundleCacheItem();
                item.m_bundleName = bundleName;
                item.m_bundle = bundle;
                m_bundleCacheDict[bundleName] = item;
                //Debug.Log(string.Format("PushAssetBundleToCache name:{0}", bundleName));
            }
            return item;
        }

        /// <summary>
        /// 从cache获取bundle
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        protected BundleCacheItem GetAssetBundleFromCache(string bundleName)
        {
            BundleCacheItem item;
            bool ret = m_bundleCacheDict.TryGetValue(bundleName, out item);
            if (ret)
            {
                //Debug.Log("GetAssetBundleFromCache ok " + bundleName);
            }
            return item;
        }

        private IEnumerator LoadBundleInStreaming(BundleData.SingleBundleData singleBundleData, Action<AssetBundle> onLoadComplete)
        {
            var bundleName = singleBundleData.m_bundleName;
            var path = AssetPathHelper.GetAssetBundlePath(bundleName);
            var req = AssetBundle.LoadFromFileAsync(path);
            while (!req.isDone)
            {
                yield return null;
            }
            if (req.assetBundle == null)
            {
                Debug.LogError(string.Format("LoadBundleInStreaming LoadFromFileAsync  fail {0}", path));
                onLoadComplete(null);
                yield break;
            }
            var loadedBundle = req.assetBundle;
            //Debug.Log(string.Format("LoadBundleInStreaming success! path: {0}", path));
            onLoadComplete(loadedBundle);
        }

        /// <summary>
        /// 注册一个bundle加载现场
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="pipeCtx"></param>
        /// <returns></returns>
        protected bool RegBundleLoadingCtx(string bundleName, out BundleLoadingCtx ctx)
        {
            bool ret = m_bundleLoadingCtxDict.TryGetValue(bundleName, out ctx);
            if (!ret)
            {
                ctx = new BundleLoadingCtx();
                ctx.m_bundleName = bundleName;
                ctx.m_ref = 1;
                m_bundleLoadingCtxDict[bundleName] = ctx;
                return false;
            }
            else
            {
                ctx.m_ref++;
                return true;
            }
        }

        /// <summary>
        /// 注销加载现场
        /// </summary>
        /// <param name="bundleLoadingCtx"></param>
        protected void UnregBundleLoadingCtx(BundleLoadingCtx bundleLoadingCtx)
        {
            bundleLoadingCtx.m_ref--;
            if (bundleLoadingCtx.m_ref <= 0)
            {
                m_bundleLoadingCtxDict.Remove(bundleLoadingCtx.m_bundleName);
            }
        }

        private IEnumerator LoadBundle(BundleData.SingleBundleData singleBundleData, Action<AssetBundle> onLoadComplete)
        {
            var bundleName = singleBundleData.m_bundleName;
            BundleCacheItem cacheItem = GetAssetBundleFromCache(bundleName);
            if (cacheItem != null)
            {
                onLoadComplete(cacheItem.m_bundle);
                yield break;
            }
            // 注册一个加载现场
            BundleLoadingCtx bundleLoadingCtx;
            bool alreadyInLoading = RegBundleLoadingCtx(bundleName, out bundleLoadingCtx);

            // 如果已经有一个加载现场处于加载中，等待加载完成
            if (alreadyInLoading)
            {
                float startWaitTime = Time.time;
                while (!bundleLoadingCtx.m_isEnd)
                {
                    if (Time.time > startWaitTime + 20)
                    {
                        Debug.LogError(string.Format("LoadBundle Waiting LoadingCtx {0} time out!", bundleLoadingCtx.m_bundleName));
                        onLoadComplete(null);
                        yield break;
                    }
                    yield return null;
                }
                onLoadComplete(GetAssetBundleFromCache(bundleName).m_bundle);
                // 注销加载现场
                UnregBundleLoadingCtx(bundleLoadingCtx);
                yield break;
            }
            var iter = LoadBundleInStreaming(singleBundleData, (ab)=> {
                // 注销加载现场
                UnregBundleLoadingCtx(bundleLoadingCtx);
                PushAssetBundleToCache(bundleName, ab);
                // bundle加载成功
                bundleLoadingCtx.m_isEnd = true;
                if (onLoadComplete != null)
                {
                    onLoadComplete(ab);
                }
            });
            while (iter.MoveNext())
            {
                yield return null;
            }
        }

        private IEnumerator LoadAssetFromBundle(string path, Action<UnityEngine.Object[]> onLoadComplete)
        {
            var singleBundleData = m_bundleDataHelper.GetBundleDataByAssetPath(path);
            if (singleBundleData == null)
            {
                Debug.LogError(string.Format("GetBundleDataByAssetPath failed, path:{0}",path));
                onLoadComplete(null);
                yield break;
            }
            AssetBundle assetBundle = null;
            var iter = LoadBundle(singleBundleData, (ab) => { assetBundle = ab; });
            while (iter.MoveNext())
            {
                yield return null;
            }
            if (assetBundle == null)
            {
                onLoadComplete(null);
                yield break;
            }
            var assetName = System.IO.Path.GetFileName(path);
            var assets = assetBundle.LoadAssetWithSubAssets(assetName);
            if (assets != null && assets[0] != null)
            {
                Debug.Log("Load Asset Success by LoadAssetFromBundle AssetPath:" + assetName);
            }
            else
            {
                Debug.LogError("Load Asset Fail by LoadAssetFromBundle AssetPath:" + assetName);
            }
            onLoadComplete(assets);
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

        public IEnumerator LoadAsset<T>(string path, Action<string, T> onEnd) where T : UnityEngine.Object
        {
            GameClientSetting clientSetting = GameClientSetting.Instance;
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
            if((clientSetting.m_useAssetBundleInEditor && Application.isEditor)
                || !Application.isEditor)
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
            if (assets == null)
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


        public void StartLoadAssetCoroutine<T>(List<string> assetPaths, Action<Dictionary<string,T>> onComplete) where T : UnityEngine.Object
        {
            Dictionary<string, T> assetDic = new Dictionary<string, T>();
            int subCoroutineNum = 3;
            int subCoroutineCompleteCount = 0;
            for(int i = 0; i < subCoroutineNum; i++)
            {
                m_coroutineManager.StartCorcoutine(AssetLoadSubCoroutine(subCoroutineNum, i, assetPaths, assetDic, ()=> {
                    subCoroutineCompleteCount++;
                    if(subCoroutineCompleteCount == subCoroutineNum)
                    {
                        onComplete(assetDic);
                    }
                }));
            }
            
        }


        public void Tick()
        {
            m_coroutineManager.Tick();
        }
    }
}

