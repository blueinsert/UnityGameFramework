using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace bluebean.UGFramework.Asset
{
    public class AssetPathHelper
    {
        public static string BundleDataAssetBundleNamle = null;
        public static string GetBundleDataBundlePathInStreamingAssets()
        {
            string path = null;
            if (BundleDataAssetBundleNamle != null)
            {
                 path = string.Format("{0}/{1}", Application.streamingAssetsPath, $"AssetBundles/{BundleDataAssetBundleNamle}");

            }else
            {
                 path = string.Format("{0}/{1}", Application.streamingAssetsPath, "AssetBundles/bundledata_ab.b");

            }
            return path;
        }

        public static string GetAssetBundlePath(string bundleName)
        {
            return string.Format("{0}/{1}/{2}",Application.streamingAssetsPath,"AssetBundles",bundleName);
        }

        public static string GetRealBundleName(string bundleName, Hash128 hash,bool useHash = false)
        {
            if (!useHash)
                return bundleName;
            var index = bundleName.LastIndexOf(".b");
            if (index != -1)
            {
                var newBundleName = $"{bundleName.Substring(0, index)}_{hash}.b";
                return newBundleName;
            }else
            {
                var newBundleName = $"{bundleName}_{hash}";
                return newBundleName;
            }
        }
    }
}
