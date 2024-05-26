using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace bluebean.UGFramework
{
    [CustomEditor(typeof(CommonStateController))]
    public class CommonStateControllerInspector : Editor
    {
        private SerializedObject m_editTarget;
        private CommonStateController m_stateController;

        public virtual void OnEnable()
        {
            m_stateController = target as CommonStateController;
            m_editTarget = new SerializedObject(m_stateController);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.Label(string.Format("Current State:{0}", m_stateController.CurState));
            if (GUILayout.Button("SwichToNextState"))
            {
                m_stateController.SwichToNextState();
                m_stateController.gameObject.SetActive(false);
                m_stateController.gameObject.SetActive(true);
            }
            if (GUILayout.Button("Save"))
            {
                m_stateController.CollectResources();
                m_editTarget.ApplyModifiedProperties();
            }
        }
    }
}
