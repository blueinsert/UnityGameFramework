using bluebean.UGFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.Asset
{
    /// <summary>
    /// bundledata�İ�����
    /// </summary>
    public class BundleDataHelper
    {
        /// <summary>
        /// �󶨵�bundleData, ��editorģʽ�л�ֱ��ʹ��assetdatabase����
        /// </summary>
        private BundleData m_internalBundleData;

        /// <summary>
        /// ��Դ·���Ƿ���Դ�Сд
        /// </summary>
        private bool m_assetPathIgnoreCase;

        /// <summary>
        /// ���ݰ󶨵�bundleData��bundleList��ȡ��dict
        /// </summary>
        private Dictionary<string, BundleData.SingleBundleData> m_bundleDataDict = new Dictionary<string, BundleData.SingleBundleData>();

        /// <summary>
        /// ��Դ·������Դ������bundle��Ӧ��
        /// </summary>
        private Dictionary<string, BundleData.SingleBundleData> m_assetPath2BundleDataDict = new Dictionary<string, BundleData.SingleBundleData>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// ���캯�������� runtime
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
        /// ͨ��bundlename��ȡBundleData
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
        /// ������Դ·����ȡ��Դ������bundle
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