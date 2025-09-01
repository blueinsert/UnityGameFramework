using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework
{
    public class ThreeDSceneLayer : SceneLayer
    {
        public override Camera LayerCamera
        {
            get
            {
                var cameras = GetComponentsInChildren<Camera>();
                if(cameras.Length == 0)
                {
                    Debug.LogError("ThreeDSceneLayer:LayerCamera is Null!");
                    return null;
                }
                return cameras[0];
            }
        }

    }
}
