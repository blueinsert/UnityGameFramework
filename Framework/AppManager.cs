using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using bluebean.UGFramework.Asset;
using bluebean.UGFramework.ConfigData;
using bluebean.UGFramework.Log;
using bluebean.UGFramework.UI;
using UnityEngine;

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

        public static string BuildSystemInfoText(bool textColor)
        {
            var sb = new StringBuilder();
            AppendLine(sb, textColor, "SystemInfo", string.Empty);
            AppendLine(sb, textColor, "Quality Setting", QualitySettings.names[QualitySettings.GetQualityLevel()]);
            AppendLine(sb, textColor, "Resolution", string.Format("{0} x {1}", Screen.width, Screen.height));
            AppendLine(sb, textColor, "Refresh Rate", Screen.currentResolution.refreshRate.ToString());
            AppendLine(sb, textColor, "Safe Area", Screen.safeArea.ToString());
            AppendLine(sb, textColor, "BatteryLevel", SystemInfo.batteryLevel.ToString());
            AppendLine(sb, textColor, "BatteryStatus", SystemInfo.batteryStatus.ToString());
            AppendLine(sb, textColor, "CopyTextureSupport", SystemInfo.copyTextureSupport.ToString());
            AppendLine(sb, textColor, "DeviceModel", SystemInfo.deviceModel);
            AppendLine(sb, textColor, "DeviceName", SystemInfo.deviceName);
            AppendLine(sb, textColor, "DeviceType", SystemInfo.deviceType.ToString());
            AppendLine(sb, textColor, "DeviceUniqueIdentifier", SystemInfo.deviceUniqueIdentifier);
            AppendLine(sb, textColor, "GraphicsDeviceID", SystemInfo.graphicsDeviceID.ToString());
            AppendLine(sb, textColor, "GraphicsDeviceName", SystemInfo.graphicsDeviceName);
            AppendLine(sb, textColor, "GraphicsDeviceType", SystemInfo.graphicsDeviceType.ToString());
            AppendLine(sb, textColor, "GraphicsDeviceVendor", SystemInfo.graphicsDeviceVendor);
            AppendLine(sb, textColor, "GraphicsDeviceVendorID", SystemInfo.graphicsDeviceVendorID.ToString());
            AppendLine(sb, textColor, "GraphicsDeviceVersion", SystemInfo.graphicsDeviceVersion.ToString());
            AppendLine(sb, textColor, "GraphicsMemorySize", SystemInfo.graphicsMemorySize.ToString());
            AppendLine(sb, textColor, "GraphicsMultiThreaded", SystemInfo.graphicsMultiThreaded.ToString());
            AppendLine(sb, textColor, "GraphicsShaderLevel", SystemInfo.graphicsShaderLevel.ToString());
            AppendLine(sb, textColor, "GraphicsUVStartsAtTop", SystemInfo.graphicsUVStartsAtTop.ToString());
            AppendLine(sb, textColor, "MaxCubemapSize", SystemInfo.maxCubemapSize.ToString());
            AppendLine(sb, textColor, "MaxTextureSize", SystemInfo.maxTextureSize.ToString());
            AppendLine(sb, textColor, "NpotSupport", SystemInfo.npotSupport.ToString());
            AppendLine(sb, textColor, "OperatingSystem", SystemInfo.operatingSystem);
            AppendLine(sb, textColor, "OperatingSystemFamily", SystemInfo.operatingSystemFamily.ToString());
            AppendLine(sb, textColor, "ProcessorCount", SystemInfo.processorCount.ToString());
            AppendLine(sb, textColor, "ProcessorFrequency", SystemInfo.processorFrequency.ToString());
            AppendLine(sb, textColor, "ProcessorType", SystemInfo.processorType.ToString());
            AppendLine(sb, textColor, "SupportedRenderTargetCount", SystemInfo.supportedRenderTargetCount.ToString());
            AppendLine(sb, textColor, "Supports2DArrayTextures", SystemInfo.supports2DArrayTextures.ToString());
            AppendLine(sb, textColor, "Supports3DTextures", SystemInfo.supports3DTextures.ToString());
            AppendLine(sb, textColor, "Supports3DRenderTextures", SystemInfo.supports3DRenderTextures.ToString());
            AppendLine(sb, textColor, "SupportsAcceleromete", SystemInfo.supportsAccelerometer.ToString());
            AppendLine(sb, textColor, "SupportsAsyncCompute", SystemInfo.supportsAsyncCompute.ToString());
            AppendLine(sb, textColor, "SupportsAudio", SystemInfo.supportsAudio.ToString());
            AppendLine(sb, textColor, "SupportsComputeShaders", SystemInfo.supportsComputeShaders.ToString());
            AppendLine(sb, textColor, "SupportsCubemapArrayTextures", SystemInfo.supportsCubemapArrayTextures.ToString());
            AppendLine(sb, textColor, "SupportsGPUFence", SystemInfo.supportsGraphicsFence.ToString());
            AppendLine(sb, textColor, "SupportsGyroscope", SystemInfo.supportsGyroscope.ToString());
            AppendLine(sb, textColor, "SupportsImageEffects", SystemInfo.supportsImageEffects.ToString());
            AppendLine(sb, textColor, "SupportsInstancing", SystemInfo.supportsInstancing.ToString());
            AppendLine(sb, textColor, "SupportsLocationService", SystemInfo.supportsLocationService.ToString());
            AppendLine(sb, textColor, "SupportsMotionVectors", SystemInfo.supportsMotionVectors.ToString());
            AppendLine(sb, textColor, "SupportsMultisampledTextures", SystemInfo.supportsMultisampledTextures.ToString());
            AppendLine(sb, textColor, "SupportsRawShadowDepthSampling", SystemInfo.supportsRawShadowDepthSampling.ToString());
            AppendLine(sb, textColor, "SupportsRenderToCubemap", SystemInfo.supportsRenderToCubemap.ToString());
            AppendLine(sb, textColor, "Supports HDR RenderTexturesFormat", SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.DefaultHDR).ToString());
            AppendLine(sb, textColor, "Supports Texture Format ETC2_RGBA8", SystemInfo.SupportsTextureFormat(TextureFormat.ETC2_RGBA8).ToString());
            AppendLine(sb, textColor, "Supports Texture Format ASTC_RGBA_4x4", SystemInfo.SupportsTextureFormat(TextureFormat.ASTC_4x4).ToString());
            AppendLine(sb, textColor, "SupportsShadows", SystemInfo.supportsShadows.ToString());
            AppendLine(sb, textColor, "SupportsSparseTextures", SystemInfo.supportsSparseTextures.ToString());
            AppendLine(sb, textColor, "SupportsTextureWrapMirrorOnce", SystemInfo.supportsTextureWrapMirrorOnce.ToString());
            AppendLine(sb, textColor, "SupportsVibration", SystemInfo.supportsVibration.ToString());
            AppendLine(sb, textColor, "SystemMemorySize", SystemInfo.systemMemorySize.ToString());
            AppendLine(sb, textColor, "UnsupportedIdentifie", SystemInfo.unsupportedIdentifier);
            AppendLine(sb, textColor, "UsesReversedZBuffe", SystemInfo.usesReversedZBuffer.ToString());
            AppendLine(sb, textColor, "DataPath", Application.dataPath);
            AppendLine(sb, textColor, "PersistentDataPath", Application.persistentDataPath);
            AppendLine(sb, textColor, "StreamingAssetsPath", Application.streamingAssetsPath);
            AppendLine(sb, textColor, "TemporaryCachePath", Application.temporaryCachePath);
            return sb.ToString();
        }

        static void AppendLine(StringBuilder sb, bool textColor, string name, string value)
        {
            if (sb.Length > 0)
                sb.Append("\n");
            if (textColor)
                sb.Append(UIUtility.AddColorTag(name, new Color(1, 0.5f, 0)));
            else
                sb.Append(name);
            sb.Append(": ");
            sb.Append(value);
        }


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
            Debug.Log(BuildSystemInfoText(false));
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
