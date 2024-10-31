using System;
using System.Collections;
using System.Collections.Generic;
using bluebean.UGFramework.Asset;
using bluebean.UGFramework.ConfigData;
using bluebean.UGFramework.Log;
using bluebean.UGFramework.UI;

namespace bluebean.UGFramework
{
    public class AppManager : ITickable, IDelayExecMgr
    {
        private AppManager() { }
        public static AppManager m_instance;
        public static AppManager Instance {
            get {
                return m_instance;
            }
        }

        public static AppManager CreateAndInitializeInstance()
        {
            var gameManager = new AppManager();
            if (gameManager.Intialize())
            {
                m_instance = gameManager;
                return m_instance;
            }
            else
            {
                Debug.LogError("GameManager Initialize Failed!");
                return null;
            }
        }

        private CoroutineScheduler m_coroutineHelper;
        private ConfigDataLoader m_configDataLoader;
        private AssetLoader m_assetLoader;
        private TaskManager m_taskManager;
        private UIManager m_uiManager;
        private SceneTreeManager m_sceneTree;

        private DelayExecMgrComp m_delayExecMgrComp;

        public ConfigDataLoader ConfigDataLoader { get { return m_configDataLoader; } }

        public CoroutineScheduler CoroutineHelper { get { return m_coroutineHelper; } }
        public TaskManager TaskManager { get { return m_taskManager; } }
        public AssetLoader ResourceManager { get { return m_assetLoader; } }
        public UIManager UIManager { get { return m_uiManager; } }
        public SceneTreeManager SceneTree { get { return m_sceneTree; } }

        public bool Intialize()
        {
            var clientSetting = AppClientSetting.Instance;
            var logMgr = LogManager.CreateLogManager();
            logMgr.Initlize(true, true, UnityEngine.Application.persistentDataPath, clientSetting.m_logConfg.LogPrefix);
            m_configDataLoader = ConfigDataLoader.CreateInstance();
            m_coroutineHelper = new CoroutineScheduler();
            m_assetLoader = AssetLoader.CreateInstance();
            if (!m_assetLoader.Initialize())
            {
                Debug.LogError("AssetLoader Initialize Failed");
                return false;
            }
            m_taskManager = TaskManager.CreateInstance();
            m_uiManager = UIManager.CreateInstance();
            m_sceneTree = SceneTreeManager.CreateInstance();
            if (!m_sceneTree.Initialize())
            {
                Debug.LogError("SceneTree Initialize Failed");
                return false;
            }
            return true;
        }

        public void Deintialize()
        {
            Debug.Log("GameManager:Deintialize Start");
            m_uiManager.StopAllUITask();
            Debug.Log("GameManager:Deintialize Complete");
        }

        public void OnApplicationQuit()
        {
            Debug.Log("GameManager:OnApplicationQuit");
            Deintialize();
        }

        public void Tick()
        {
            Timer.Tick();

            if (m_coroutineHelper != null)
            {
                m_coroutineHelper.Tick();
            }
            if (m_assetLoader != null)
            {
                m_assetLoader.Tick();
            }
            if(m_taskManager != null)
            {
                m_taskManager.Tick();
            }
            if (m_sceneTree != null)
            {
                m_sceneTree.Tick();
            }
        }

        protected virtual void ConstructComps()
        {
            m_delayExecMgrComp = new DelayExecMgrComp();
        }

        public void PostDelayTimeExecuteAction(Action action, float delaySeconds)
        {
            ((IDelayExecMgr)m_delayExecMgrComp)?.PostDelayTimeExecuteAction(action, delaySeconds);
        }

        public void PostDelayTicksExecuteAction(Action action, ulong delayTickCount)
        {
            ((IDelayExecMgr)m_delayExecMgrComp)?.PostDelayTicksExecuteAction(action, delayTickCount);
        }

        public void StartLoadAllConfigData(Action onEnd)
        {
            m_coroutineHelper.StartCorcoutine(m_configDataLoader.LoadAllConfigData((res) => {
                if (!res)
                {
                    Debug.Log("ConfigDataLoader Init Error");
                }
                if (onEnd != null)
                {
                    onEnd();
                }
            }));
        }

        public void RunCoroutine(IEnumerator enumerator)
        {
            m_coroutineHelper.StartCorcoutine(enumerator);
        }
    }
}
