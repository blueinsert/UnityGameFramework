using bluebean.UGFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;

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
        public AssetBundle m_bundle;                // �������bundle

        public int m_refCount;                      // ���������õĴ���

        /// <summary>
        /// ������
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
        /// bundle�Ļ���
        /// </summary>
        protected Dictionary<string, BundleCacheItem> m_bundleCacheDict = new Dictionary<string, BundleCacheItem>();

        protected Dictionary<string, BundleLoadingCtx> m_bundleLoadingCtxDict = new Dictionary<string, BundleLoadingCtx>();

        /// <summary>
        /// ��bundle���浽cache��
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
        /// ��cache��ȡbundle
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
            return SystemUtility.GetCurrentTargetPlatform();
        }

        private bool LoadingBundleData()
        {
            var path = AssetPathHelper.GetBundleDataBundlePathInStreamingAssets();
            var bundle = AssetBundle.LoadFromFile(path);
            var bundleData = bundle.LoadAsset<BundleData>("BundleData.asset");
            if (bundleData == null)
            {
                Debug.LogError(string.Format("AssetLoader:LoadBundleData failed,path:{0}", path));
                return false;
            }
            Debug.Log("AssetLoader:LoadBundleData Success");
            bundle.Unload(false);
            m_bundleData = bundleData;
            m_bundleDataHelper = new BundleDataHelper(m_bundleData, true);
            return true;
        }

        protected IEnumerator AssetBundleManifestLoadingWorker(Action<bool> onEnd)
        {
            var bundleName = GetAssetBundleManifestBundleName();

            // ����bundle
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

            // ����AssetBundleManifest
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
            // �ͷ�bundle
            manifestBundle.Unload(false);
        }

        private IEnumerator LoadSingleBundleFromStreaming(BundleData.SingleBundleData singleBundleData, Action<AssetBundle> onLoadComplete)
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

        private IEnumerator LoadSingleBundleFromStreaming(string bundleName, Action<AssetBundle> onLoadComplete)
        {
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
        /// ע��һ��bundle�����ֳ�
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
        /// ע�������ֳ�
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
            // ע��һ�������ֳ�
            BundleLoadingCtx bundleLoadingCtx;
            bool alreadyInLoading = RegBundleLoadingCtx(bundleName, out bundleLoadingCtx);

            // ����Ѿ���һ�������ֳ����ڼ����У��ȴ��������
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
                // ע�������ֳ�
                UnregBundleLoadingCtx(bundleLoadingCtx);
                yield break;
            }

            // ��ȡ���е�ֱ������
            var dependenceList = m_assetBundleManifest.GetDirectDependencies(singleBundleData.m_bundleName);
            List<BundleCacheItem> dependBundleCacheList = null;

            // ����������bundle
            if (dependenceList != null && dependenceList.Length != 0)
            {
                //Debug.LogWarning("LoadBundle load dependence bundles for " + bundleData.m_bundleName);

                foreach (string dependence in dependenceList)
                {
                    AssetBundle dependBundle = null;
                    var iter = LoadBundle(dependence, (lbundle) => { dependBundle = lbundle; });
                    while (iter.MoveNext())
                    {
                        yield return null;
                    }
                    if (dependBundle == null)
                    {
                        // �κ�һ��������bundle����ʧ��,��bundle����ʧ��
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

                        // �ȼ�¼�������������Ļ���
                        dependBundleCacheList.Add(GetAssetBundleFromCache(dependence));
                    }
                }
            }

            // ���ر�bundle
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
                // ע�������ֳ�
                UnregBundleLoadingCtx(bundleLoadingCtx);
                yield break;
            }

            // ע�������ֳ�
            bundleLoadingCtx.m_isEnd = true;
            UnregBundleLoadingCtx(bundleLoadingCtx);
           
            cacheItem =  PushAssetBundleToCache(bundleName, loadedBundle);

            cacheItem.m_dependBundleCacheList = dependBundleCacheList;

            // �����м������֮�󣬽�������Ŀ�Ļ���ͨ��ref�͵�ǰbundle��������
            // ��������Ĺ��̶ദ����ʧ�ܣ������������ֻ���������
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

        protected IEnumerator LoadBundle(string bundleName, Action<AssetBundle> onComplete)
        {
            var bundleData = m_bundleDataHelper.GetBundleDataByName(bundleName);
            if (bundleData == null)
            {
                Debug.LogError(string.Format("LoadBundle fail {0},singleBundleData == null", bundleName)); 
                onComplete(null);
                yield break;
            }
            var iter = LoadBundle(bundleData, onComplete);
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
    }
}
