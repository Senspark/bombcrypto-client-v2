using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Assertions;

using DG.Tweening;

namespace App
{
    [DefaultExecutionOrder(-1)]
    [DisallowMultipleComponent]
    public class AudioPlayer : MonoBehaviour
    {
        private class Follower : MonoBehaviour
        {
            public Transform TargetTransform { get; set; }

            private void Update()
            {
                if (TargetTransform != null)
                {
                    var position = TargetTransform.position;
                    position.y = 0;
                    position.z = -10;
                    transform.position = position;
                }
            }
        }

        public static AudioPlayer Instance { get; private set; } = null;

        private bool _musicEnabled;
        private bool _soundEnabled;
        private AudioSource _bgmSource;
        private AudioSource _oneShotSfxSource;
        private readonly List<AudioSource> _loopSfxSources = new List<AudioSource>();
        private readonly List<AudioSource> _spatialSfxSources = new List<AudioSource>();

        private bool _pause = false;

        public bool MusicEnabled
        {
            get => _musicEnabled;
            set
            {
                _musicEnabled = value;
                _bgmSource.mute = !_musicEnabled;
            }
        }

        public bool SoundEnabled
        {
            get => _soundEnabled;
            set
            {
                _soundEnabled = value;
                if (!_soundEnabled)
                {
                    for (var i = 0; i < _loopSfxSources.Count; ++i)
                    {
                        StopLoopSource(i);
                    }
                    for (var i = 0; i < _spatialSfxSources.Count; ++i)
                    {
                        StopSpatialSfxSource(i);
                    }
                }
            }
        }

        public void PlayMusic(AudioClip clip, float volume, bool loop = true)
        {
            if (_bgmSource.isPlaying) {
                return;
            }
            
            _bgmSource.clip = clip;
            _bgmSource.volume = 0;
            _bgmSource.loop = loop;
            _bgmSource.Play();

            var step = 0;
            var increase = DOTween.To(() => step,
              SetMusicVolume,
              volume, 1).SetEase(Ease.InOutSine);

            DOTween.defaultTimeScaleIndependent = true;
            DOTween.Sequence()
                .Append(increase);

        }

        public void StopMusic()
        {
            if (!_bgmSource.isPlaying) {
                return;
            }
            
            var step = _bgmSource.volume;
            var decrease = DOTween.To(() => step,
                                      SetMusicVolume,
                                      0, 1).SetEase(Ease.InOutSine);
            
            DOTween.defaultTimeScaleIndependent = true;
            DOTween.Sequence()
                .Append(decrease)
                .AppendCallback(() =>
                {
                    if (_bgmSource.isPlaying) {
                        _bgmSource.Stop();
                    }
                });
        }
        
        public void StopImmediateMusic()
        {
            if (!_bgmSource.isPlaying) {
                return;
            }
            _bgmSource.Stop();
        }

        public void ChangeMusic(AudioClip clip, float volume)
        {
            var step1 = _bgmSource.volume;
            var decrease = DOTween.To(() => step1,
                                      SetMusicVolume,
                                      0, 1).SetEase(Ease.InOutSine);
            
            var step2 = 0;
            var increase = DOTween.To(() => step2,
                          SetMusicVolume,
                          volume, 1).SetEase(Ease.InOutSine);
            
            DOTween.defaultTimeScaleIndependent = true;
            DOTween.Sequence()
                .Append(decrease)
                .AppendCallback(() =>
                {
                    if (_bgmSource.isPlaying) {
                        _bgmSource.Stop();
                    }
                })
                .AppendCallback(() => {
                    if (_bgmSource.isPlaying) {
                        return;
                    }
                    _bgmSource.clip = clip;
                    _bgmSource.volume = 0;
                    _bgmSource.loop = true;
                    _bgmSource.Play();
                })
                .Append(increase);
        }

        public void ChangeMusicImmediate(AudioClip clip, float volume, bool loop = true)
        {
            _bgmSource.Stop();
            _bgmSource.clip = clip;
            _bgmSource.volume = volume;
            _bgmSource.loop = loop;
            _bgmSource.Play();
        }

        public void PauseMusic()
        {
            _pause = true;

            if (!SoundEnabled)
            {
                return;
            }
            _bgmSource.Pause();
        }

        public void ResumeMusic()
        {
            _pause = false;

            if (!SoundEnabled)
            {
                return;
            }
            _bgmSource.UnPause();
        }

        public void SetMusicVolume(float volume)
        {
            _bgmSource.volume = volume;
        }

        public void PlaySound(AudioClip clip, float volume, Transform trans)
        {
            if (_pause)
            {
                return;
            }

            if (!SoundEnabled)
            {
                return;
            }
            
            _oneShotSfxSource.clip = clip;
            _oneShotSfxSource.volume = volume;
            _oneShotSfxSource.loop = false;
            _oneShotSfxSource.Play();
            
        }

        public int PlayLoopSound(AudioClip clip, float volume, Transform trans)
        {
            if (!SoundEnabled)
            {
                return -1;
            }
            if (trans == null)
            {
                var channel = GetAvailableLoopChannel();
                var source = _loopSfxSources[channel];
                source.clip = clip;
                source.volume = volume;
                source.Play();
                return channel << 1;
            }
            else
            {
                var channel = GetAvailableSpatialChannel();
                var source = _spatialSfxSources[channel];
                source.gameObject.SetActive(true);
                source.GetComponent<Follower>().TargetTransform = trans;
                source.loop = true;
                source.clip = clip;
                source.volume = volume;
                source.Play();
                return channel << 1 | 1;
            }
        }

        public void StopLoopSound(int id)
        {
            if (id == -1)
            {
                return;
            }
            var channel = id >> 1;
            if ((id & 1) == 0)
            {
                StopLoopSource(channel);
            }
            else
            {
                StopSpatialSfxSource(channel);
            }
        }

        private void StopLoopSource(int channel)
        {
            Assert.IsTrue(channel < _loopSfxSources.Count);
            var source = _loopSfxSources[channel];
            source.Stop();
            source.clip = null;
        }

        private void StopSpatialSfxSource(int channel)
        {
            Assert.IsTrue(channel < _spatialSfxSources.Count);
            var source = _spatialSfxSources[channel];
            source.Stop();
            source.gameObject.SetActive(false);
        }

        private int GetAvailableLoopChannel()
        {
            for (var i = 0; i < _loopSfxSources.Count; ++i)
            {
                var source = _loopSfxSources[i];
                if (source.clip == null)
                {
                    return i;
                }
            }
            var component = gameObject.AddComponent<AudioSource>();
            component.loop = true;
            _loopSfxSources.Add(component);
            return _loopSfxSources.Count - 1;
        }

        private int GetAvailableSpatialChannel()
        {
            for (var i = 2; i < _spatialSfxSources.Count; ++i)
            {
                var source = _spatialSfxSources[i];
                if (!source.gameObject.activeSelf)
                {
                    return i;
                }
            }
            var channel = _spatialSfxSources.Count;
            var go = new GameObject($"Channel {channel}");
            go.AddComponent<Follower>();
            go.transform.SetParent(transform, false);
            var component = go.AddComponent<AudioSource>();
            component.spatialBlend = 1.0f;
            component.maxDistance = 6.0f; // 50.0f;
            component.minDistance = 2.0f; // 6.0f;

            _spatialSfxSources.Add(component);
            return channel;
        }

        private void Awake() {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            } else {
                Destroy(gameObject);
                return;
            }

            var sources = GetComponents<AudioSource>().ToList();
            _bgmSource = sources[0];
            _oneShotSfxSource = sources[1];

            // channel 0: Background music.
            // channel 1: One shot sfx at camera position.
            // other channels: One shot/loop sfx at desired position.
        }

        private void OnDestroy() {
            if (Instance == this) {
                StopMusic();
                Instance = null;
            }
        }
    }
}