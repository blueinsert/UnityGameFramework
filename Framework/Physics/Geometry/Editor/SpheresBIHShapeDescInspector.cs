using bluebean.UGFramework.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace bluebean.UGFramework.Geometry
{
    [CustomEditor(typeof(SpheresBIHShapeDesc))]
    public class SpheresBIHShapeDescInspector : Editor
    {
        private SerializedObject m_editTarget;
        private SpheresBIHShapeDesc m_bih;

        public virtual void OnEnable()
        {
            m_bih = target as SpheresBIHShapeDesc;
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
