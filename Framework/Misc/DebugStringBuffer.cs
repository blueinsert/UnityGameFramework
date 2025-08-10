using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;

namespace bluebean.UGFramework
{
    public class DebugStringBuffer
    {
        public static readonly int MaxCacheNum = 3;
        //public static StringBuilder s_stringBuilder = null;
        public string[] m_histroyInfos = null;
        public  string m_stringNewestInfo;

        private object m_lock = new object();

        public static DebugStringBuffer m_instance = null;
        public static DebugStringBuffer Instance
        {
            get
            {
                if(m_instance == null)
                {
                    m_instance = new DebugStringBuffer();
                    m_instance.Initialize(MaxCacheNum);
                }
                return m_instance;
            }
        }

        public void Initialize(int maxCacheNum)
        {
            m_histroyInfos = new string[MaxCacheNum];
        }

        public DebugStringBuffer() { }

        public  void CacheInfo(string info)
        {
            lock (m_lock)
            {
                //Console.WriteLine($"Debug:CacheInfo {info}");
                m_stringNewestInfo = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff")} [{Thread.CurrentThread.ManagedThreadId}] {info}";
                for (int i = 0; i < MaxCacheNum - 1; i++)
                {
                    m_histroyInfos[i] = m_histroyInfos[i + 1];
                }
                m_histroyInfos[MaxCacheNum - 1] = m_stringNewestInfo;
            }
        }

        public  string GetInfos()
        {
            string res = "";
            lock (m_lock)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < MaxCacheNum; i++)
                {
                    if (!string.IsNullOrEmpty(m_histroyInfos[i]))
                    {
                        sb.Append(m_histroyInfos[i]);
                        sb.Append("\n");
                    }
                }
                res = sb.ToString();
            }
            return res;
        }
    }
}
