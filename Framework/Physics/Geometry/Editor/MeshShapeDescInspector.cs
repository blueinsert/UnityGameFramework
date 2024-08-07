using bluebean.UGFramework.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace bluebean.UGFramework.Geometry
{
    [CustomEditor(typeof(MeshShapeDesc))]
    public class MeshShapeDescInspector : Editor
    {
        private SerializedObject m_editTarget;
        private MeshShapeDesc m_meshShapeDesc;

        public virtual void OnEnable()
        {
            m_meshShapeDesc = target as MeshShapeDesc;
            m_editTarget = new SerializedObject(m_meshShapeDesc);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("UpdateShape"))
            {
                var start = DateTime.Now;
                m_meshShapeDesc.UpdateShapeIfNeeded();
                Debug.Log($"MeshShapeUpdate:consume {(DateTime.Now - start).TotalMilliseconds}ms");
            } 
        }
    }
}
