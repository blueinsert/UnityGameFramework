using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace bluebean.UGFramework
{
    [Serializable]
    public class TransformTweenClip : PlayableAsset, ITimelineClipAsset
    {
        public Vector3 startScale = Vector3.one;
        public Vector3 endScale = Vector3.one;

        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<TransformTweenBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
            behaviour.startScale = startScale;
            behaviour.endScale = endScale;
            return playable;
        }
    }
} 