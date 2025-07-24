using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bluebean.Mugen3D;
using bluebean.UGFramework;
using bluebean.UGFramework.Asset;
using bluebean.UGFramework.DataStruct;

namespace bluebean.UGFramework.Sound
{
    public class SoundItem : IComparable<SoundItem>
    {
        public float delay { get; set; }
        public AudioClip clip { get; private set; }
        public float volume { get; private set; }

        public SoundItem(AudioClip clip, float delay, float volume)
        {
            this.clip = clip;
            this.delay = delay;
            this.volume = volume;
        }

        public int CompareTo(SoundItem other)
        {
            return delay.CompareTo(other.delay);
        }
    }

    public class SoundChannel : MonoBehaviour
    {
        public enum SoundChannelType
        {
            Default = 0,
            Player,
            HitSnd,
            Env,
            Bgm,
            Reserve1,
            Reserve2,
            Reserve3,
        }

        public SoundChannelType m_channelType;
        private AudioSource m_audioSource;
        private bool m_isPaused = false;
        private Coroutine m_volumeFadeCoroutine = null;
        private float m_targetVolume = 1f;
        private float m_lastSetVolume = 1f;

        private void Awake()
        {
            m_audioSource = gameObject.AddComponent<AudioSource>();    
             if(m_channelType == SoundChannelType.Bgm){
                m_audioSource.loop = true;
            }else{
                m_audioSource.loop = false;
            }
        }

        private PriorityQueue<SoundItem> m_playQueue = new PriorityQueue<SoundItem>();

        private void PlaySound(AudioClip clip, float volume)
        {
             if(m_isPaused){
                m_isPaused = false;
                m_audioSource.UnPause();
            }
            if (m_volumeFadeCoroutine != null)
                StopCoroutine(m_volumeFadeCoroutine);
            m_volumeFadeCoroutine = StartCoroutine(FadeVolume(m_audioSource.volume, 1f, 0.2f, ()=>{
                //m_audioSource.Play();
            }));
           
            m_audioSource.PlayOneShot(clip, volume * m_lastSetVolume);
        }

        public void Stop(){  
            m_playQueue.Clear();
             if (m_volumeFadeCoroutine != null)
                StopCoroutine(m_volumeFadeCoroutine);
            m_volumeFadeCoroutine = StartCoroutine(FadeVolume(m_audioSource.volume, 0f, 0.2f,()=>{
                m_audioSource.Stop();
            }));
        }

        public void Play(AudioClip clip, float volume, float delay)
        {
            m_playQueue.Enqueue(new SoundItem(clip, delay, volume));
        }

        public void Update()
        {

            if (m_playQueue.Count() == 0)
                return;
            var e = m_playQueue.GetEnumerator();
            while (e.MoveNext())
            {
                e.Current.delay -= Time.deltaTime;
            }
            while (m_playQueue.Count() > 0 && m_playQueue.Peek().delay <= 0)
            {
                var item = m_playQueue.Dequeue();
                PlaySound(item.clip, item.volume);
            }
        }

        private IEnumerator FadeVolume(float from, float to, float duration, Action onFinish)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                m_lastSetVolume = Mathf.Lerp(from, to, t);
                m_audioSource.volume = m_lastSetVolume;
                yield return null;
            }
            m_lastSetVolume = to;
            m_audioSource.volume = to;
            onFinish?.Invoke();
        }

        public void Puase(){
            if (m_isPaused) return;
            m_isPaused = true;
            if (m_volumeFadeCoroutine != null)
                StopCoroutine(m_volumeFadeCoroutine);
            m_volumeFadeCoroutine = StartCoroutine(FadeVolume(m_audioSource.volume, 0f, 0.2f,()=>{
                m_audioSource.Pause();
            }));
        }

        public void Resume(){
            if (!m_isPaused) return;
            m_isPaused = false;
            if (m_volumeFadeCoroutine != null)
                StopCoroutine(m_volumeFadeCoroutine);
            m_audioSource.UnPause();
            m_volumeFadeCoroutine = StartCoroutine(FadeVolume(m_audioSource.volume, 1f, 0.2f, ()=>{
                //m_audioSource.Play();
            }));
        }
    }

    public class SoundPlayer : Singleton<SoundPlayer>
    {
        private Dictionary<string, AudioClip> m_cache = new Dictionary<string, AudioClip>();
        private Dictionary<SoundChannel.SoundChannelType, SoundChannel> m_soundChannelDic = new Dictionary<SoundChannel.SoundChannelType, SoundChannel>();

        private void Start() { }
        private void Awake()
        {
            for(int i = 0; i < System.Enum.GetNames(typeof(SoundChannel.SoundChannelType)).Length; i++)
            {
                SoundChannel channel = gameObject.AddComponent<SoundChannel>();
                channel.m_channelType = (SoundChannel.SoundChannelType)i;
                m_soundChannelDic.Add((SoundChannel.SoundChannelType)i, channel);
            }    
        }

        public void Play(string key, string path, float delay = 0, float volume = 1, SoundChannel.SoundChannelType channel = SoundChannel.SoundChannelType.Default)
        {
            AudioClip clip = null;
            if (m_cache.ContainsKey(key))
            {
                clip = m_cache[key];
            }
            else
            {
                clip = AssetUtility.Instance.GetAsset<AudioClip>(path);
                if (clip != null)
                {
                    m_cache.Add(key, clip);
                }
                else
                {
                    Debug.LogError("can't load audioClip: " + path);
                }
            }
            if (clip == null)
                return;
            m_soundChannelDic[channel].Play(clip, volume, delay);
        }

        public void PauseChannel(SoundChannel.SoundChannelType channel){
            m_soundChannelDic[channel].Puase();
        }

        public void ResumeChannel(SoundChannel.SoundChannelType channel){
            m_soundChannelDic[channel].Resume();
        }

         public void StopChannel(SoundChannel.SoundChannelType channel){
            m_soundChannelDic[channel].Stop();
        }
        
    }
}
