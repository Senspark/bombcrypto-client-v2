using JetBrains.Annotations;

using UnityEngine;

namespace Senspark {
    [DisallowMultipleComponent]
    [AddComponentMenu("Senspark/Audio Player")]
    public class AudioPlayer : MonoBehaviour {
        [SerializeField]
        private AudioSource _audioSource;

        private bool _isMuted;
        private bool _isLoop;
        private float _volume;
        private float _musicVolume;

        [CanBeNull]
        private AudioClip _clip;

        public bool IsMuted {
            get => _isMuted;
            set {
                if (_isMuted == value) {
                    return;
                }
                _isMuted = value;
                UpdateAudio();
            }
        }

        public bool IsLoop {
            get => _isLoop;
            set {
                if (_isLoop == value) {
                    return;
                }
                _isLoop = value;
                UpdateAudio();
            }
        }

        public float Volume {
            get => _volume;
            set {
                if (Mathf.Approximately(_volume, value)) {
                    return;
                }
                _volume = value;
                UpdateVolume();
            }
        }

        private void Awake() {
            _musicVolume = 1f;
            UpdateAudio();
            UpdateVolume();
        }

        private void UpdateAudio() {
            _audioSource.mute = IsMuted;
            _audioSource.loop = IsLoop;
        }

        private void UpdateVolume() {
            _audioSource.volume = Volume * _musicVolume;
        }

        public void PlayMusic([NotNull] AudioClip clip, float volume) {
            if (_clip != clip) {
                // Update clip.
                _clip = clip;
                _audioSource.clip = clip;
            }
            if (!Mathf.Approximately(_musicVolume, volume)) {
                // Update volume.
                _musicVolume = volume;
                UpdateVolume();
            }
            _audioSource.Play();
        }

        public void PlaySound(AudioClip clip, float volume) {
            _audioSource.PlayOneShot(clip, volume);
        }

        public void Stop() {
            _audioSource.Stop();
        }
    }
}