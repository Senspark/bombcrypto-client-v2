using System.Threading.Tasks;

using UnityEngine;

namespace App {
    public class NullSoundManager : ISoundManager {
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public float MusicVolume { get; set; }
        public float SoundVolume { get; set; }
        public bool MusicEnabled { get; set; }
        public bool SoundEnabled { get; set; }
        public void SetVolumeSound(float volume) {
        }

        public void SetVolumeMusic(float volume) {
        }

        public void PlayMusic(Audio audio) {
        }

        public void PlayOrResume(Audio audio) {
            
        }

        public void StopMusic() {
        }

        public void StopImmediateMusic() {
            
        }

        public void ChangeMusic(Audio audio) {
        }

        public void ChangeMusicImmediate(Audio audio, bool loop = true) {
        }

        public void PauseMusic() {
        }

        public void ResumeMusic() {
        }

        public void PlaySound(Audio audio, Transform trans = null, bool allowMulti = false) {
        }

        public void PlaySoundMoving(Audio audio, Transform trans = null, bool allowMulti = false) {
        }

        public int PlayLoopSound(Audio audio, Transform trans = null) {
            return 0;
        }

        public void StopLoopSound(int id) {
        }
    }
}