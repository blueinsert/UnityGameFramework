using System.Collections.Generic;
using UnityEngine;

namespace bluebean.Framework.CurveEditor
{
    [System.Serializable]
    public class CurveSegment
    {
        public List<Vector3> points = new List<Vector3>();
        public bool isHidden = false;
    }

    [CreateAssetMenu(fileName = "CurveData", menuName = "Custom/CurveData", order = 1)]
    public class CurveData : ScriptableObject
    {
        public List<CurveSegment> curves = new List<CurveSegment>();
    }
} 