
//using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace bluebean.UGFramework
{

    internal abstract partial class LocalAccountConfigDataBase
    {

    }

    internal abstract partial class LocalAccountConfig<T> where T : LocalAccountConfigDataBase
    {
        /// <summary>
        /// 重置数据(于Load失败时调用)
        /// </summary>
        protected abstract void ResetLocalAccountConfigData();

        public T Data { get { return m_data; } }
        public static LocalAccountConfig<T> Instance { set { s_instance = value; } get { return s_instance; } }

        static LocalAccountConfig<T> s_instance;
        static string m_fileName;
        protected T m_data;

        public LocalAccountConfig()
        {
        }

        public void SetFileName(string name)
        {
            m_fileName = name;
        }

        public bool Save()
        {
            if (string.IsNullOrEmpty(m_fileName))
                return false;

            string saveText = JsonUtility.ToJson(m_data, true);

            return FileUtility.WriteText(m_fileName, saveText);

        }

        public bool Load()
        {
            if (string.IsNullOrEmpty(m_fileName))
            {
                ResetLocalAccountConfigData();
                return false;
            }

            if (!FileUtility.IsFileExist(m_fileName))
            {
                ResetLocalAccountConfigData();
                return false;
            }

            string saveText = FileUtility.ReadText(m_fileName);
            if (string.IsNullOrEmpty(saveText))
            {
                ResetLocalAccountConfigData();
                return false;
            }

            var data = JsonUtility.FromJson<T>(saveText);
            if (data == null)
            {
                Console.WriteLine(string.Format("LocalAccountConfig.Load {0} failed.", saveText));
                ResetLocalAccountConfigData();
                return false;
            }

            CompatibilityProcess(data);

            m_data = data;

            return true;
        }

        protected virtual void CompatibilityProcess(T data)
        {

        }
    }

}