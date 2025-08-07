using bluebean.UGFramework.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace bluebean.UGFramework.Geometry
{
    [CustomEditor(typeof(BIHSpheresShapeDesc))]
    public class BIHSpheresShapeDescInspector : Editor
    {
        private SerializedObject m_editTarget;
        private BIHSpheresShapeDesc m_bih;

        public virtual void OnEnable()
        {
            m_bih = target as BIHSpheresShapeDesc;
            m_editTarget = new SerializedObject(m_bih);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Build"))
            {
                var start = DateTime.Now;
                m_bih.Build();
                Debug.Log($"MeshShapeUpdate:consume {(DateTime.Now - start).TotalMilliseconds}ms");
            }
        }
    }
}
