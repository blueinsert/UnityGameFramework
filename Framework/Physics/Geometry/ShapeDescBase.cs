using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.Geometry
{
    [ExecuteInEditMode]
    public abstract class ShapeDescBase : MonoBehaviour
    {
        protected bool m_isDirty = true;
        private Matrix4x4 m_matrixCache =  Matrix4x4.identity;

        private void Awake()
        {
            SetDirty();
            UpdateShapeIfNeeded();
        }

        public abstract void UpdateShapeImpl();
        public virtual void UpdateShapeIfNeeded()
        {
            if (IsNeededUpdate())
            {
                m_isDirty = false;
                UpdateShapeImpl();
            }
        }
        public virtual bool IsNeededUpdate()
        {
            var m = this.transform.localToWorldMatrix;
            if(m != m_matrixCache)
            {
                m_matrixCache = m;
                return true;
            }
            return m_isDirty;
        }

        public void SetDirty()
        {
            m_isDirty = true;
        }

        // Update is called once per frame
        void Update()
        {
            UpdateShapeIfNeeded();
        }
    }
}
