using UnityEngine;
using UnityEngine.Playables;

namespace bluebean.UGFramework
{
    public class TransformTweenMixerBehaviour : PlayableBehaviour
    {
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            // 简单实现：只播放第一个clip
            int inputCount = playable.GetInputCount();
            for (int i = 0; i < inputCount; i++)
            {
                float inputWeight = playable.GetInputWeight(i);
                if (inputWeight > 0f)
                {
                    ScriptPlayable<TransformTweenBehaviour> inputPlayable = (ScriptPlayable<TransformTweenBehaviour>)playable.GetInput(i);
                    inputPlayable.GetBehaviour().ProcessFrame(inputPlayable, info, playerData);
                }
            }
        }
    }
} 