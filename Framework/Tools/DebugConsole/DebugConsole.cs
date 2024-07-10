using UnityEngine;
using UnityEngine.Assertions;
namespace DebugConsole
{
    public class DebugConsole : MonoBehaviour
    {
        DebugConsoleView _consoleView;

        static void LogPlatformDefine()
        {
            Debug.Log(string.Format("Application.platform = {0}", Application.platform));
#if UNITY_IOS
        Debug.Log("UNITY_IOS define."); 
#endif
#if UNITY_ANDROID
        Debug.Log("UNITY_ANDROID define.");
#endif
#if UNITY_STANDALONE
            Debug.Log("UNITY_STANDALONE define.");
#endif
#if UNITY_EDITOR
            Debug.Log("UNITY_EDITOR define.");
#endif
        }

        void Awake()
        {
            instance = this;
            Init();
        }

        void Start()
        {
            LogPlatformDefine();
        }

        public void Init()
        {
            _consoleView = DebugConsoleView.Create();
            DebugConsoleCtrl.Create(_consoleView, DebugConsoleMode.Create());
        }

        protected void OnGUI()
        {
            _consoleView.Show();
        }

        void OnDestroy()
        {
            instance = null;
        }

        public static DebugConsole instance = null;
    }
}