using bluebean.UGFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.Asset
{
    /// <summary>
    /// bundledata的帮助类
    /// </summary>
    public class BundleDataHelper
    {
        /// <summary>
        /// 绑定的bundleData, 在editor模式中会直接使用assetdatabase加载
        /// </summary>
        private BundleData m_internalBundleData;

        /// <summary>
        /// 资源路径是否忽略大小写
        /// </summary>
        private bool m_assetPathIgnoreCase;

        /// <summary>
        /// 根据绑定的bundleData的bundleList获取的dict
        /// </summary>
        private Dictionary<string, BundleData.SingleBundleData> m_bundleDataDict = new Dictionary<string, BundleData.SingleBundleData>();

        /// <summary>
        /// 资源路径和资源所属的bundle对应表
        /// </summary>
        private Dictionary<string, BundleData.SingleBundleData> m_assetPath2BundleDataDict = new Dictionary<string, BundleData.SingleBundleData>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 构造函数，用于 runtime
        /// </summary>
        /// <param name="bundleData"></param>
        public BundleDataHelper(BundleData bundleData, bool assetPathIgnoreCase)
        {
            m_internalBundleData = bundleData;
            m_assetPathIgnoreCase = assetPathIgnoreCase;

            m_internalBundleData.m_bundleList.ForEach((elem) => { m_bundleDataDict.Add(elem.m_bundleName, elem); });
            m_internalBundleData.m_bundleList.ForEach((elem) =>
            {
                elem.m_assetList.ForEach((assetPath) =>
                {
                    m_assetPath2BundleDataDict.Add(m_assetPathIgnoreCase ? assetPath.ToLower() : assetPath, elem);
                });
            });
        }

        /// <summary>
        /// 通过bundlename获取BundleData
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public BundleData.SingleBundleData GetBundleDataByName(string name)
        {
            BundleData.SingleBundleData data;
            if (!m_bundleDataDict.TryGetValue(name, out data))
            {
                return null;
            }
            return data;
        }

        /// <summary>
        /// 根据资源路径获取资源所属的bundle
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public BundleData.SingleBundleData GetBundleDataByAssetPath(string assetPath)
        {
            BundleData.SingleBundleData data;
            if (!m_assetPath2BundleDataDict.TryGetValue(m_assetPathIgnoreCase ? assetPath.ToLower() : assetPath, out data))
            {
                return null;
            }
            return data;
        }

    }

}