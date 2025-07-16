using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace bluebean.UGFramework
{
    [TrackColor(0.1f, 0.6f, 0.8f)]
    [TrackClipType(typeof(TransformTweenClip))]
    [TrackBindingType(typeof(Transform))]
    public class TransformTweenTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<TransformTweenMixerBehaviour>.Create(graph, inputCount);
        }
    }
} 