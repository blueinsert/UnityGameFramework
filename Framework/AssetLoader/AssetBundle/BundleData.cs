using System;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.Asset
{
    /// <summary>
    /// 用来存放Bundle的信息，为资源加载提供帮助
    /// </summary>
    [System.Serializable]
    public class BundleData : ScriptableObject
    {
        [Serializable]
        public class SingleBundleData
        {
            /// <summary>
            /// bundle的名称
            /// </summary>
            public string m_bundleName = null;
            /// <summary>
            /// 通过BuildPipeline.GetHashForAssetBundle的到的
            /// </summary>
            public string m_bundleHash = null;
            /// <summary>
            /// CRC校验码
            /// </summary>
            public uint m_bundleCRC = 0;
            /// <summary>
            /// bundle的大小，byte
            /// </summary>
            public long m_size = 0;  
            /// <summary>
            /// bundle包含的资源列表
            /// </summary>
            public List<string> m_assetList = new List<string>();
        }

        /// <summary>
        /// bundle列表
        /// </summary>
        public List<SingleBundleData> m_bundleList = new List<SingleBundleData>();
    }

}
