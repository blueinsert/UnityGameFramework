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

        [Header("[��ɫ�������]")]
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
        [Header("[Unity������Ч]")]
        [FormerlySerializedAs("PressedAudioClip")]
        [SerializeField]
        protected AudioClip m_pressedAudioClip;

        [Header("[On״̬��Ҫ��ʾ�����б�]")]
        [FormerlySerializedAs("OnStateShowList")]
        [SerializeField]
        protected List<GameObject> m_onStateShowObjectList;

        [Header("[On״̬��ɫ�ı��б�]")]
        [FormerlySerializedAs("OnStateColorList")]
        [SerializeField]
        protected List<ColorDesc> m_onStateColorList;

        [Header("[On״̬Tween�б�]")]
        [FormerlySerializedAs("OnStateTweenList")]
        [SerializeField]
        protected List<TweenMain> m_onStateTweenList;

        [Header("[Off״̬��Ҫ��ʾ�����б�]")]
        [FormerlySerializedAs("OffStateShowList")]
        [SerializeField]
        protected List<GameObject> m_offStateShowObjectList;

        [Header("[Off״̬��ɫ�ı��б�]")]
        [FormerlySerializedAs("OffStateColorList")]
        [SerializeField]
        protected List<ColorDesc> m_offStateColorList;

        [Header("[Off״̬Tween�б�]")]
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
        /// �ⲿ�߼����ð�ť��ǰ��ɫ����
        /// </summary>
        public virtual void SetBaseColorList(Dictionary<GameObject, Color> baseColorList)
        {
            m_baseColorList = baseColorList;
        }

        /// <summary>
        /// ״̬�仯ʱ��������ش���
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
                        // ����ⲿ���õ���ʽ���������ڣ� ��ֱ�����ó�Ĭ��ֵ
                        if (m_baseColorList == null)
                        {
                            SetChildComponentColor(m_normalStateColorList);
                        }
                        // �����Ȱ����ⲿ�����ָ������������ⲿ�����еĲŰ���Ĭ��ֵ�ָ�
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
            // ������ɫ��disable
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
            // �����ⲿ���õ���ʽ������ɫ���ҵ���ֱ��ʹ��
            Color color = Color.clear;
            if (m_baseColorList.TryGetValue(go, out color))
            {
                return color;
            }

            // δ�ҵ�����ʹ��normal״̬��ʽ����ɫ
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

            // ֪ͨѡ��״̬��ɫ�ı���
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
            // ���ֻ���¼���������ison��״̬û�仯����donothing
            if (m_preIsOn == isOn)
            {
                return;
            }

            // ��¼ison״̬
            m_preIsOn = isOn;

            // ���ݵ�ǰ״̬�����ػ���ʾ����
            ShowOrHideObjectWhenIsOnChanged(isOn);

            // ������ɫ������tween����
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
