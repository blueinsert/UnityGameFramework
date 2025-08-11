using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using System;
using System.Collections.Generic;

namespace bluebean.UGFramework.UI
{

    [AddComponentMenu("UIExtend/ToggleEx")]
    public class ToggleEx : Toggle
    {

        [Header("[颜色相关物体]")]
        [FormerlySerializedAs("ButtonComponent")]
        [SerializeField]
        protected List<GameObject> m_childComponents;

        [FormerlySerializedAs("NormalStateColorList")]
        [SerializeField]
        protected List<Color> m_normalStateColorList;

        [FormerlySerializedAs("PressedStateColorList")]
        [SerializeField]
        protected List<Color> m_pressedStateColorList;

        [FormerlySerializedAs("DisableStateColorList")]
        [SerializeField]
        protected List<Color> m_disableStateColorList;

        protected Dictionary<GameObject, Color> m_baseColorList;

        public AudioClip PressedAudioClip { get { return m_pressedAudioClip; } }
        [Header("[Unity按下音效]")]
        [FormerlySerializedAs("PressedAudioClip")]
        [SerializeField]
        protected AudioClip m_pressedAudioClip;

        [Header("[On状态需要显示对象列表]")]
        [FormerlySerializedAs("OnStateShowList")]
        [SerializeField]
        protected List<GameObject> m_onStateShowObjectList;

        [Header("[On状态颜色改变列表]")]
        [FormerlySerializedAs("OnStateColorList")]
        [SerializeField]
        protected List<ColorDesc> m_onStateColorList;

        [Header("[On状态Tween列表]")]
        [FormerlySerializedAs("OnStateTweenList")]
        [SerializeField]
        protected List<TweenMain> m_onStateTweenList;

        [Header("[Off状态需要显示对象列表]")]
        [FormerlySerializedAs("OffStateShowList")]
        [SerializeField]
        protected List<GameObject> m_offStateShowObjectList;

        [Header("[Off状态颜色改变列表]")]
        [FormerlySerializedAs("OffStateColorList")]
        [SerializeField]
        protected List<ColorDesc> m_offStateColorList;

        [Header("[Off状态Tween列表]")]
        [FormerlySerializedAs("OffStateTweenList")]
        [SerializeField]
        protected List<TweenMain> m_offStateTweenList;

        protected bool m_preIsOn;


        [Serializable]
        public class ColorDesc
        {
            public GameObject go;
            public Color color;

        }

        protected override void Awake()
        {
            m_preIsOn = isOn;
            onValueChanged.AddListener(OnValueChanged);
        }

        public void SetNormalState()
        {
            DoStateTransition(SelectionState.Normal, false);
        }

        public void SetPressedState()
        {
            DoStateTransition(SelectionState.Pressed, false);
        }

        public void SetDisableState()
        {
            DoStateTransition(SelectionState.Disabled, false);
        }

        public void SetIsOn(bool ison)
        {
            m_preIsOn = !ison;
            OnValueChanged(ison);
        }

        /// <summary>
        /// 外部逻辑设置按钮当前颜色方案
        /// </summary>
        public virtual void SetBaseColorList(Dictionary<GameObject, Color> baseColorList)
        {
            m_baseColorList = baseColorList;
        }

        /// <summary>
        /// 状态变化时，进行相关处理
        /// </summary>
        /// <param name="state"></param>
        /// <param name="instant"></param>
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);
            //Debug.Log($"ToggleEx:DoStateTransition {this.gameObject.name} {state} {instant}");
            if (gameObject.activeInHierarchy)
            {
                switch (state)
                {
                    case SelectionState.Normal:
                    case SelectionState.Highlighted:
                        // 如果外部设置的样式方案不存在， 则直接设置成默认值
                        if (m_baseColorList == null)
                        {
                            SetChildComponentColor(m_normalStateColorList);
                        }
                        // 否则，先按照外部方案恢复，不存在于外部方案中的才按照默认值恢复
                        else
                        {
                            ResetChildCompontentToBaseColor();
                        }
                        break;
                    case SelectionState.Pressed:
                        SetChildComponentColor(m_pressedStateColorList);
                        break;
                    case SelectionState.Disabled:
                        SetChildComponentColor(m_disableStateColorList);
                        break;

                }
            }
        }

        protected override void InstantClearState()
        {
            base.InstantClearState();
            // 设置颜色到disable
            SetChildComponentColor(m_disableStateColorList);
        }

        protected void SetChildComponentColor(List<Color> stateColorList)
        {
            if (stateColorList == null)
            {
                return;
            }

            for (int i = 0; i < m_childComponents.Count; ++i)
            {
                if (i >= stateColorList.Count)
                {
                    break;
                }

                var go = m_childComponents[i];
                if (go == null)
                {
                    continue;
                }

                SetGameObjectColor(go, stateColorList[i]);

            }

        }

        protected void ResetChildCompontentToBaseColor()
        {
            for (int i = 0; i < m_childComponents.Count; ++i)
            {
                var go = m_childComponents[i];
                if (go == null)
                {
                    continue;
                }

                Color color = GetBaseStateColor(go, i);

                SetGameObjectColor(go, color);
            }
        }
        protected Color GetBaseStateColor(GameObject go, int index)
        {
            // 先在外部设置的样式中找颜色，找到了直接使用
            Color color = Color.clear;
            if (m_baseColorList.TryGetValue(go, out color))
            {
                return color;
            }

            // 未找到，则使用normal状态样式的颜色
            if (index >= m_normalStateColorList.Count)
            {
                return Color.clear;
            }

            return m_normalStateColorList[index];
        }

        protected void ShowOrHideObjectWhenIsOnChanged(bool isOn)
        {
            if (isOn)
            {
                if (m_offStateShowObjectList != null)
                {
                    foreach (var go in m_offStateShowObjectList)
                    {
                        if (go != null)
                            go.SetActive(false);
                    }
                }

                if (m_onStateShowObjectList != null)
                {
                    foreach (var go in m_onStateShowObjectList)
                    {
                        if (go != null)
                            go.SetActive(true);
                    }
                }

            }
            else
            {
                if (m_onStateShowObjectList != null)
                {
                    foreach (var go in m_onStateShowObjectList)
                    {
                        if (go != null)
                            go.SetActive(false);
                    }
                }

                if (m_offStateShowObjectList != null)
                {
                    foreach (var go in m_offStateShowObjectList)
                    {
                        if (go != null)
                            go.SetActive(true);
                    }
                }

            }
        }

        protected void SetObjectColorWhenIsOnChanged(List<ColorDesc> stateColorList)
        {
            if (stateColorList == null)
            {
                return;
            }

            Dictionary<GameObject, Color> dict = new Dictionary<GameObject, Color>();

            foreach (var colorDesc in stateColorList)
            {
                if (colorDesc.go != null)
                {
                    SetGameObjectColor(colorDesc.go, colorDesc.color);
                    dict[colorDesc.go] = colorDesc.color;
                }
            }

            // 通知选中状态颜色改变了
            SetBaseColorList(dict);
        }

        protected void PlayTweenListWhenIsOnChanged(List<TweenMain> tweenList)
        {
            if (tweenList == null)
            {
                return;
            }

            foreach (var tween in tweenList)
            {
                if (tween != null)
                {
                    tween.ResetToBeginning();
                    tween.PlayForward();
                }
            }
        }

        protected void SetGameObjectColor(GameObject go, Color color)
        {
            if (color == Color.clear)
            {
                return;
            }

            var image = go.GetComponent<Image>();
            if (image != null)
            {
                image.color = color;
            }
            else
            {
                var text = go.GetComponent<Text>();
                if (text != null)
                {
                    text.color = color;
                }
            }
        }

        protected void OnValueChanged(bool isOn)
        {
            // 如果只是事件发生，而ison的状态没变化，则donothing
            if (m_preIsOn == isOn)
            {
                return;
            }

            // 记录ison状态
            m_preIsOn = isOn;

            // 根据当前状态，隐藏或显示对象
            ShowOrHideObjectWhenIsOnChanged(isOn);

            // 设置颜色，播放tween动画
            if (isOn)
            {
                SetObjectColorWhenIsOnChanged(m_onStateColorList);
                PlayTweenListWhenIsOnChanged(m_onStateTweenList);
            }
            else
            {
                SetObjectColorWhenIsOnChanged(m_offStateColorList);
                PlayTweenListWhenIsOnChanged(m_offStateTweenList);
            }
        }


    }
}
