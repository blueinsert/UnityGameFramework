using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace bluebean.UGFramework.Asset
{
    public class AssetPathHelper
    {
        public static string GetBundleDataBundlePathInStreamingAssets()
        {
            var path = string.Format("{0}/{1}", Application.streamingAssetsPath, "AssetBundles/bundledata_ab.b");
            return path;
        }

        public static string GetAssetBundlePath(string bundleName)
        {
            return string.Format("{0}/{1}/{2}",Application.streamingAssetsPath,"AssetBundles",bundleName);
        }
    }
}
