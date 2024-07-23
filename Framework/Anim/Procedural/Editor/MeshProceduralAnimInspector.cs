using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace bluebean.UGFramework.Anim
{
    [CustomEditor(typeof(MeshProceduralAnim), true)]
    public class MeshProceduralAnimInspector : UnityEditor.Editor
    {
        private MeshProceduralAnim m_data;
        private float m_normalizeTime = 0;

        public void OnEnable()
        {
            m_data = (MeshProceduralAnim)target;
            m_normalizeTime = 0;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(10);


            if (GUILayout.Button("GenerateMesh", GUILayout.Width(150)))
            {
                m_data.GenerateMesh();
            }
            var last = m_normalizeTime;
            m_normalizeTime = GUILayout.HorizontalSlider(m_normalizeTime, 0, 1);
            if (last != m_normalizeTime)
            {
                m_data.SetNormalizedTime(m_normalizeTime);
                m_data.AnimSample();
            }
           
            EditorGUILayout.Space(10);
            GUILayout.Label(string.Format("normalizedTime:{0:F2}-{1:F2}", m_normalizeTime, m_data.m_normalizedTime));
            EditorGUILayout.Space(10);
            EditorGUILayout.Space(10);

            EditorGUILayout.EndVertical();
        }
    }
}