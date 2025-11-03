using bluebean.UGFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Networking;

namespace bluebean.UGFramework.Asset
{
    public class BundleLoadingCtx
    {
        public string m_bundleName;
        public bool m_isEnd;
        public int m_ref;
    }

    public class BundleCacheItem
    {
        public string m_bundleName;
        public AssetBundle m_bundle;                // 被缓存的bundle

        public int m_refCount;                      // 被依赖引用的次数

        /// <summary>
        /// 依赖项
        /// </summary>
        public List<BundleCacheItem> m_dependBundleCacheList;

        public void AddRefrence()
        {
            m_refCount++;
        }
        public void RemoveRefrence()
        {
            m_refCount--;
        }
    }

    public partial class AssetLoader
    {

        protected AssetBundleManifest m_assetBundleManifest = null;
        private BundleData m_bundleData;
        private BundleDataHelper m_bundleDataHelper;
        /// <summary>
        /// bundle的缓存
        /// </summary>
        protected Dictionary<string, BundleCacheItem> m_bundleCacheDict = new Dictionary<string, BundleCacheItem>();

        protected Dictionary<string, BundleLoadingCtx> m_bundleLoadingCtxDict = new Dictionary<string, BundleLoadingCtx>();

        public string m_platformAssetBundleName = null;

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
            BundleCacheItem item = null;
            bool ret = m_bundleCacheDict.TryGetValue(bundleName, out item);
            if (ret)
            {
                //Debug.Log("GetAssetBundleFromCache ok " + bundleName);
            }
            return item;
        }

        protected string GetAssetBundleManifestBundleName()
        {
            if (m_platformAssetBundleName != null)
                return m_platformAssetBundleName;
            return SystemUtility.GetCurrentTargetPlatform();
        }

        private IEnumerator LoadingBundleData(Action<bool> onEnd)
        {
            bool res = false;
            var path = AssetPathHelper.GetBundleDataBundlePathInStreamingAssets();
            Debug.Log($"LoadingBundleData path:{path}");
            AssetBundle bundle = null;
            if (path.StartsWith("http"))
            {
                UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(path);
                yield return request.SendWebRequest();
                while (request.result == UnityWebRequest.Result.InProgress)
                {
                    yield return null;
                }
                yield return new WaitUntil(() => {
                    return request.isDone;
                });
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(string.Format("LoadingBundleData UnityWebRequest  fail {0} {1}", path,request.result));              
                }
                else
                {
                    bundle = (request.downloadHandler as DownloadHandlerAssetBundle).assetBundle;
                }
            }else
            {
                bundle = AssetBundle.LoadFromFile(path);
            }
            var bundleData = bundle.LoadAsset<BundleData>("BundleData.asset");
            if (bundleData == null)
            {
                Debug.LogError(string.Format("AssetLoader:LoadBundleData failed,path:{0}", path));
                res = false;
            }
            Debug.Log("AssetLoader:LoadBundleData Success");
            bundle.Unload(false);
            m_bundleData = bundleData;
            m_bundleDataHelper = new BundleDataHelper(m_bundleData, true);
            res = true;
            if (onEnd != null)
            {
                onEnd(res);
            }
        }

        protected IEnumerator AssetBundleManifestLoadingWorker(Action<bool> onEnd)
        {
            var bundleName = GetAssetBundleManifestBundleName();

            // 加载bundle
            AssetBundle manifestBundle = null;
            var lbIter = LoadSingleBundleFromStreaming(bundleName, (lbundle) => { manifestBundle = lbundle; });
            while (lbIter.MoveNext())
            {
                yield return null;
            }

            if (manifestBundle == null)
            {
                Debug.LogError("AssetBundleManifestLoadingWorker load AssetBundleManifest failed!");
                onEnd(false);
                yield break;
            }

            Debug.Log("AssetBundleManifestLoadingWorker load AssetBundleManifest ok");

            // 加载AssetBundleManifest
            var assets = manifestBundle.LoadAllAssets<AssetBundleManifest>();
            if (assets.Length <= 0)
            {
                Debug.LogError("BundleDataLoadingWorker can not find AssetBundleManifest in bundle ");
                onEnd(false);
                yield break;
            }
            m_assetBundleManifest = assets[0];
            Debug.Log("AssetBundleManifestLoadingWorker m_assetBundleManifest load ok");
            onEnd(true);
            // 释放bundle
            manifestBundle.Unload(false);
        }

        private IEnumerator LoadSingleBundleFromStreaming(BundleData.SingleBundleData singleBundleData, Action<AssetBundle> onLoadComplete)
        {
            var bundleName = singleBundleData.m_realBundleName;
            var path = AssetPathHelper.GetAssetBundlePath(bundleName);
            Debug.Log($"LoadSingleBundleFromStreaming2 bundleName:{bundleName} path:{path}");
            AssetBundle loadedBundle = null;
            if (path.StartsWith("http"))
            {
                UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(path);
                yield return request.SendWebRequest();
                while (request.result == UnityWebRequest.Result.InProgress)
                {
                    yield return null;
                }
                yield return new WaitUntil(() => {
                    return request.isDone;
                });
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(string.Format("LoadSingleBundleFromStreaming UnityWebRequest  fail {0} {1}", path, request.result));
                }
                else
                {
                    loadedBundle = (request.downloadHandler as DownloadHandlerAssetBundle).assetBundle;
                }
            }else
            {
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
                loadedBundle = req.assetBundle;
            }
            
            //Debug.Log(string.Format("LoadBundleInStreaming success! path: {0}", path));
            onLoadComplete(loadedBundle);
        }

        private IEnumerator LoadSingleBundleFromStreaming(string bundleName, Action<AssetBundle> onLoadComplete)
        {
            var path = AssetPathHelper.GetAssetBundlePath(bundleName);
            AssetBundle loadedBundle = null;
            Debug.Log($"LoadSingleBundleFromStreaming1 bundleName:{bundleName} path:{path}");
            if (path.StartsWith("http"))
            {
                UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(path);
                yield return request.SendWebRequest();
                while(request.result == UnityWebRequest.Result.InProgress)
                {
                    yield return null;
                }
                yield return new WaitUntil(() => {
                    return request.isDone;
                });
                if(request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(string.Format("LoadBundleInStreaming UnityWebRequestAssetBundle  fail {0}", path));
                    Debug.LogError(GetType() + "/ERROR/" + request.error + " " + request.result);
                    onLoadComplete(null);
                    request.Dispose();
                    yield break;
                }
                else{
                    loadedBundle = (request.downloadHandler as DownloadHandlerAssetBundle).assetBundle;
                    Debug.Log($"loadedBundle: {loadedBundle}");
                    request.Dispose();
                }
            }
            else
            {
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
                loadedBundle = req.assetBundle;
            }
            Debug.Log(string.Format("LoadBundleInStreaming success! path: {0}", path));
            onLoadComplete(loadedBundle);
        }

        private IEnumerator LoadSingleBundleImpl(BundleData.SingleBundleData singleBundleData, Action<AssetBundle> onLoadComplete)
        {
            var iter = LoadSingleBundleFromStreaming(singleBundleData, (ab) => {
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
                ctx.m_isEnd = false;
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

            var realBundleName = AssetPathHelper.GetRealBundleName(singleBundleData.m_bundleName, singleBundleData.m_bundleHash, BuildSetting.Instance.IsAppendHashToAssetBundleName);
            // 获取所有的直接依赖
            var dependenceList = m_assetBundleManifest.GetDirectDependencies(realBundleName);
            List<BundleCacheItem> dependBundleCacheList = null;

            // 加载依赖的bundle
            if (dependenceList != null && dependenceList.Length != 0)
            {
                Debug.Log("LoadBundle load dependence bundles for " + singleBundleData.m_bundleName + " \n depends:"+ string.Join("/", dependenceList));

                foreach (string dependence in dependenceList)
                {
                    AssetBundle dependBundle = null;
                    BundleData.SingleBundleData bundleData = null;
                    bundleData = m_bundleDataHelper.GetBundleDataByRealName(dependence);
                    if(bundleData == null)
                    {
                        Debug.LogError(string.Format("load dependence bundle:{0} failed! bundleData == null", bundleName));
                        continue;
                    }
                    var iter = LoadBundle(bundleData, (lbundle) => { dependBundle = lbundle; });
                    while (iter.MoveNext())
                    {
                        yield return null;
                    }
                    if (dependBundle == null)
                    {
                        // 任何一个依赖的bundle加载失败,本bundle加载失败
                        Debug.LogError(string.Format("LoadBundle fail by load dependence fail {0} {1}", bundleName, dependence));
                        onLoadComplete(null);
                        yield break;
                    }
                    else
                    {
                        if (dependBundleCacheList == null)
                        {
                            dependBundleCacheList = new List<BundleCacheItem>();
                        }

                        // 先记录下来所有依赖的缓存
                        dependBundleCacheList.Add(GetAssetBundleFromCache(bundleData.m_bundleName));
                    }
                }
            }
            else
            {
                Debug.Log("dependence bundles for " + singleBundleData.m_bundleName + " is null");
            }
            // 加载本bundle
            AssetBundle loadedBundle = null;
            var iter2 = LoadSingleBundleImpl(singleBundleData, (ab) => {
                loadedBundle = ab;
            });
            while (iter2.MoveNext())
            {
                yield return null;
            }
            if (loadedBundle == null)
            {
                bundleLoadingCtx.m_isEnd = true;
                onLoadComplete(null);
                // 注销加载现场
                UnregBundleLoadingCtx(bundleLoadingCtx);
                yield break;
            }

            // 注销加载现场
            bundleLoadingCtx.m_isEnd = true;
            UnregBundleLoadingCtx(bundleLoadingCtx);
           
            cacheItem =  PushAssetBundleToCache(bundleName, loadedBundle);

            cacheItem.m_dependBundleCacheList = dependBundleCacheList;

            // 在所有加载完成之后，将依赖项目的缓存通过ref和当前bundle关联起来
            // 由于上面的过程多处可能失败，所以这个处理只能留到最后
            if (dependBundleCacheList != null)
            {
                foreach (var item in dependBundleCacheList)
                {
                    item.AddRefrence();
                }
            }

            if (onLoadComplete != null)
            {
                onLoadComplete(loadedBundle);
            }
        }

        //protected IEnumerator LoadBundle(string bundleName, Action<AssetBundle> onComplete)
        //{
        //    var bundleData = m_bundleDataHelper.GetBundleDataByName(bundleName);
        //    if (bundleData == null)
        //    {
        //        Debug.LogError(string.Format("LoadBundle fail {0},singleBundleData == null", bundleName)); 
        //        onComplete(null);
        //        yield break;
        //    }
        //    var iter = LoadBundle(bundleData, onComplete);
        //    while (iter.MoveNext())
        //    {
        //        yield return null;
        //    }
        //}

        private IEnumerator LoadAssetFromBundle(string path, Action<UnityEngine.Object[]> onLoadComplete)
        {
            var singleBundleData = m_bundleDataHelper.GetBundleDataByAssetPath(path);
            if (singleBundleData == null)
            {
                Debug.LogError(string.Format("GetBundleDataByAssetPath failed, path:{0}", path));
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


        public IEnumerator InitializeAssetBundlePipeline(Action<bool> onEnd)
        {
            //获取BundleData.json中的内容
            var url = string.Format("{0}/{1}", Application.streamingAssetsPath, "AssetBundles/BundleData.json");
            UnityWebRequest request = UnityWebRequest.Get(url);// new UnityWebRequest(url);
            yield return request.SendWebRequest();
            while (request.result == UnityWebRequest.Result.InProgress)
            {
                yield return null;
            }
            yield return new UnityEngine.WaitUntil(() => {
                return request.isDone;
            });
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("AssetLoader get BundleData.txt Failed!");
                onEnd(false);
                yield break;
            }
            else
            {
                var content = request.downloadHandler.text;
                Debug.Log($"AssetLoader get BundleData.txt: {content}");
                AssetPathHelper.BundleDataAssetBundleNamle = content;
            }

            //获取Platform.json中的内容
            url = string.Format("{0}/{1}", Application.streamingAssetsPath, "AssetBundles/Platform.json");
            request = UnityWebRequest.Get(url);// new UnityWebRequest(url);
            yield return request.SendWebRequest();
            while (request.result == UnityWebRequest.Result.InProgress)
            {
                yield return null;
            }
            yield return new UnityEngine.WaitUntil(() => {
                return request.isDone;
            });
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("AssetLoader get Platform.json Failed!");
                onEnd(false);
                yield break;
            }
            else
            {
                var content = request.downloadHandler.text;
                Debug.Log($"AssetLoader get Platform.json: {content}");
                m_platformAssetBundleName = content;
            }

            bool res = false;
            yield return AssetLoader.Instance.StartLoadingBundleData((bundleDataRes) => { res = bundleDataRes; });
            if (!res)
            {
                Debug.LogError("AssetLoader.Instance.StartLoadingBundleData() Failed!");
                onEnd(false);
                yield break;
            }

            bool isAssetBundleManifestLoadSuccess = false;
            yield return AssetLoader.Instance.StartAssetBundleManifestLoading((res) => { isAssetBundleManifestLoadSuccess = res; });
            if (!isAssetBundleManifestLoadSuccess)
            {
                Debug.LogError("AssetLoader.Instance.StartAssetBundleManifestLoading Failed!");
                onEnd(false);
                yield break;
            }
            onEnd(true);
        }
    }
}
