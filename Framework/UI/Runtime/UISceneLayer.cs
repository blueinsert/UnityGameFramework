using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.UI
{
    public class UISceneLayer : SceneLayer
    {
        public override Camera LayerCamera
        {
            get
            {
                return GetComponentInParent<Camera>();
            }
        }

        public override int GetSortPriority()
        {
            var desc = GetComponentInChildren<UILayerDesc>();
            if (desc != null)
            {
                if (desc.m_isOnTop)
                {
                    return 100;
                }
            }
            return 0;
        }

    }
}
