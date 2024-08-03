using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace bluebean.UGFramework.UI
{
    public enum GradientDirection
    {
        Vertical,
        Horizontal,
    }

    /// <summary>
    /// UI顶点色动画
    /// </summary>
    [AddComponentMenu("UI/Effects/ColorGradient")]
    public class Gradient : BaseMeshEffect
    {
        [Serializable]
        public class ColorKeyPoint
        {
            [SerializeField]
            public float m_pos;
            [SerializeField]
            public Color m_color;
        }

        public class ColorSpan
        {
            public ColorKeyPoint m_from;
            public ColorKeyPoint m_to;
        }

        public GradientDirection m_gradientDir = GradientDirection.Vertical;

        public List<ColorKeyPoint> m_colors = new List<ColorKeyPoint>();

        private List<ColorSpan> m_colorSpans = null;

        public Color m_startColor = Color.white;
        public Color m_endColor = Color.black;

        //protected override void OnValidate()
        //{
        //    CollectColorSpans(true);
        //}

        private void CollectColorSpans(bool force = false)
        {
            if (!force && m_colorSpans != null)
                return;
            
            m_colorSpans = new List<ColorSpan>();
            if (m_colors == null || m_colors.Count == 0)
            {
                m_colorSpans.Add(new ColorSpan()
                {
                    m_from = new ColorKeyPoint() { m_pos = 0, m_color = m_startColor },
                    m_to = new ColorKeyPoint() { m_pos = 1, m_color = m_endColor },
                });
                return;
            }
            var temp = new List<ColorKeyPoint>();
            temp.AddRange(m_colors);
            temp.Sort((e1,e2) => {
                return e1.m_pos.CompareTo(e2.m_pos);
            });
            if (temp[0].m_pos != 0)
            {
                m_colorSpans.Add(new ColorSpan() { 
                    m_from = new ColorKeyPoint() {m_pos=0,m_color = m_startColor },
                    m_to = temp[0],
                });
            }
            for(int i = 0; i < temp.Count-1; i++)
            {
                m_colorSpans.Add(new ColorSpan()
                {
                    m_from = temp[i],
                    m_to = temp[i+1],
                });
            }
            if (temp[temp.Count - 1].m_pos < 1)
            {
                m_colorSpans.Add(new ColorSpan()
                {
                    m_from = temp[temp.Count - 1],
                    m_to = new ColorKeyPoint() { m_pos = 1, m_color = m_endColor },
                });
            }
        }

        private Color LerpColor(float value)
        {
            ColorSpan target = null;
            if (value < 0)
            {
                target = m_colorSpans[0];
            }else if(value >= 1)
            {
                target = m_colorSpans[m_colorSpans.Count - 1];
            }
            else
            {
                foreach (var span in m_colorSpans)
                {
                    if (span.m_from.m_pos <= value && value < span.m_to.m_pos)
                    {
                        target = span; break;
                    }
                }
            }
            
            var value2 = (value - target.m_from.m_pos) / (target.m_to.m_pos - target.m_from.m_pos);
            var color = Color.Lerp(target.m_from.m_color, target.m_to.m_color, value2);
            Debug.Log($"value:{value} value2:{value2} color:{color}");
            return color;
        }

        /// <summary>
        /// todo 顶点数太少
        /// </summary>
        /// <param name="vh"></param>
        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive())
                return;
            CollectColorSpans();

            UIVertex uiVertex = UIVertex.simpleVert;
            float min = float.MaxValue;
            float max = float.MinValue;

            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref uiVertex, i);
                float p = (m_gradientDir == GradientDirection.Vertical) ? uiVertex.position.y : uiVertex.position.x;
                min = Mathf.Min(min, p);
                max = Mathf.Max(max, p);
            }

            float h = max - min;
            if (h > 0)
            {
                for (int i = 0; i < vh.currentVertCount; i++)
                {
                    vh.PopulateUIVertex(ref uiVertex, i);
                    float value = ((m_gradientDir == GradientDirection.Vertical ? uiVertex.position.y : uiVertex.position.x) - min) / h;
                    var color = LerpColor(value);
                    uiVertex.color *= color;
                    vh.SetUIVertex(uiVertex, i);
                }
            }
        }
    }
}
