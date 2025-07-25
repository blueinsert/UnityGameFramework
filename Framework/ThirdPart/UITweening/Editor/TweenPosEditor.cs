using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections;

namespace UnityEngine.UI
{
    [CustomEditor(typeof(TweenPos))]
    public class TweenPosEditor : TweenMainEditor
    {
        public override void OnInspectorGUI()
        {
            TweenPos self = (TweenPos)target;

            EditorGUILayout.BeginHorizontal();
            self.from = EditorGUILayout.Vector3Field("From", self.from);
            if (GUILayout.Button("\u25C0", GUILayout.Width(24f)))
                self.FromCurrentValue();
            if (GUILayout.Button("Record", GUILayout.Width(50f)))
            {
                self.from = self.transform.localPosition;
            }
            EditorGUILayout.EndHorizontal();
            self.fromOffset = EditorGUILayout.Toggle("Offset", self.fromOffset);


            EditorGUILayout.BeginHorizontal();
            self.to = EditorGUILayout.Vector3Field("To", self.to);
            if (GUILayout.Button("\u25C0", GUILayout.Width(24f)))
                self.ToCurrentValue();
            if (GUILayout.Button("Record", GUILayout.Width(50f)))
            {
                self.to = self.transform.localPosition;
            }
            EditorGUILayout.EndHorizontal();
            self.toOffset = EditorGUILayout.Toggle("Offset", self.toOffset);

            BaseTweenerProperties();
        }
    }
}
