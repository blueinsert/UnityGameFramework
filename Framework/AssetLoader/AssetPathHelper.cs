using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace bluebean.UGFramework
{
    public class AssetPathHelper
    {
        public static string GetBundleDataPathInResources()
        {
            string platform = "StandaloneWindows64";
            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            {
                platform = "StandaloneWindows64";
            }
            var path = "BundleData" + platform;
            return path;
        }

        public static string GetAssetBundlePath(string bundleName)
        {
            return string.Format("{0}/{1}/{2}",Application.streamingAssetsPath,"AssetBundles",bundleName);
        }
    }
}
