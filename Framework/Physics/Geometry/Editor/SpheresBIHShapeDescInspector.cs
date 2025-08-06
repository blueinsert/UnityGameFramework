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
        private SpheresBIHShapeDesc m_meshShapeDesc;

        public virtual void OnEnable()
        {
            m_meshShapeDesc = target as SpheresBIHShapeDesc;
            m_editTarget = new SerializedObject(m_meshShapeDesc);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}
