using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;
using System;
using System.Collections.Generic;


namespace bluebean.UGFramework.UI
{
    [CustomEditor(typeof(ToggleEx))]
    public class ToggleExEditor : ToggleEditor
    {
        public enum SelectState
        {
            Normal,
            Press,
            Disable,
        }

        public enum ActiveState
        {
            On,
            Off,
        }


        protected SelectState m_selectState = SelectState.Normal;
        protected bool m_isSelectStateFoldOut = true;
        protected ActiveState m_activeState = ActiveState.On;
        protected bool m_isActiveStateFoldOut = true;
        protected SerializedObject m_toggle;
        protected SerializedProperty m_childComponents;
        protected SerializedProperty m_normalStateContext;
        protected SerializedProperty m_pressedStateContext;
        protected SerializedProperty m_disableStateContext;
        protected SerializedProperty m_pressedAudioClip;
        protected SerializedProperty m_onStateHideList;
        protected SerializedProperty m_onStateColorList;
        protected SerializedProperty m_onStateTweenList;
        protected SerializedProperty m_offStateHideList;
        protected SerializedProperty m_offStateColorList;
        protected SerializedProperty m_offStateTweenList;


        protected override void OnEnable()
        {
            base.OnEnable();

            m_toggle = new SerializedObject(target);
            m_childComponents = m_toggle.FindProperty("m_childComponents");
            m_normalStateContext = m_toggle.FindProperty("m_normalStateColorList");
            m_pressedStateContext = m_toggle.FindProperty("m_pressedStateColorList");
            m_disableStateContext = m_toggle.FindProperty("m_disableStateColorList");
            m_pressedAudioClip = m_toggle.FindProperty("m_pressedAudioClip");
            m_onStateHideList = m_toggle.FindProperty("m_onStateShowObjectList");
            m_onStateColorList = m_toggle.FindProperty("m_onStateColorList");
            m_onStateTweenList = m_toggle.FindProperty("m_onStateTweenList");
            m_offStateHideList = m_toggle.FindProperty("m_offStateShowObjectList");
            m_offStateColorList = m_toggle.FindProperty("m_offStateColorList");
            m_offStateTweenList = m_toggle.FindProperty("m_offStateTweenList");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            m_toggle.Update();

            m_isSelectStateFoldOut = EditorGUILayout.Foldout(m_isSelectStateFoldOut, "SelectStateSetting");
            if (m_isSelectStateFoldOut)
            {

                EditorGUILayout.PropertyField(m_childComponents, true);

                EditorGUILayout.Space();

                m_selectState = (SelectState)EditorGUILayout.EnumPopup("SelectState", m_selectState);
                if (m_selectState == SelectState.Normal)
                {
                    EditorGUILayout.PropertyField(m_normalStateContext, true);
                    if (!Application.isPlaying)
                    {
                        (target as ToggleEx).SetNormalState();
                    }

                }
                else if (m_selectState == SelectState.Press)
                {
                    EditorGUILayout.PropertyField(m_pressedStateContext, true);
                    if (!Application.isPlaying)
                    {
                        (target as ToggleEx).SetPressedState();
                    }

                }
                else if (m_selectState == SelectState.Disable)
                {
                    EditorGUILayout.PropertyField(m_disableStateContext, true);
                    if (!Application.isPlaying)
                    {
                        (target as ToggleEx).SetDisableState();
                    }

                }

                EditorGUILayout.PropertyField(m_pressedAudioClip);
            }

            EditorGUILayout.Space();

            m_isActiveStateFoldOut = EditorGUILayout.Foldout(m_isActiveStateFoldOut, "ActiveStateSetting");
            if (m_isActiveStateFoldOut)
            {
                m_activeState = (ActiveState)EditorGUILayout.EnumPopup("ActiveState", m_activeState);
                if (m_activeState == ActiveState.On)
                {
                    EditorGUILayout.PropertyField(m_onStateHideList, true);
                    EditorGUILayout.PropertyField(m_onStateColorList, true);
                    EditorGUILayout.PropertyField(m_onStateTweenList, true);

                    if (!Application.isPlaying)
                    {
                        //(target as ToggleEx).SetIsOn(true);
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(m_offStateHideList, true);
                    EditorGUILayout.PropertyField(m_offStateColorList, true);
                    EditorGUILayout.PropertyField(m_offStateTweenList, true);

                    if (!Application.isPlaying)
                    {
                        //(target as ToggleEx).SetIsOn(false);
                    }
                }
            }


            m_toggle.ApplyModifiedProperties();

            SceneView.RepaintAll();


        }

      
    }
}
