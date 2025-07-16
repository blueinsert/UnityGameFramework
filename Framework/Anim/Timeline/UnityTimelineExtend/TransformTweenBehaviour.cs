using UnityEngine;
using UnityEngine.Playables;

namespace bluebean.UGFramework
{
    public class TransformTweenBehaviour : PlayableBehaviour
    {
        public Vector3 startScale = Vector3.one;
        public Vector3 endScale = Vector3.one;
        private Transform _transform;
        private Vector3 _originalScale;
        private bool _initialized = false;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (!_initialized)
            {
                _transform = playerData as Transform;
                if (_transform != null)
                {
                    _originalScale = _transform.localScale;
                    _initialized = true;
                }
            }
            if (_transform == null) return;
            double progress = playable.GetTime() / playable.GetDuration();
            _transform.localScale = Vector3.Lerp(startScale, endScale, (float)progress);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (_transform != null)
            {
                _transform.localScale = _originalScale;
            }
            _initialized = false;
        }
    }
} 