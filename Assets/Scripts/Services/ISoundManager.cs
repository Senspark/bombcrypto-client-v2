using Senspark;

using UnityEngine;

namespace App {
    [Service(nameof(ISoundManager))]
    public interface ISoundManager : IService {
        float MusicVolume { set; get; }
        float SoundVolume { set; get; }
        bool MusicEnabled { get; set; }
        bool SoundEnabled { get; set; }
        void SetVolumeSound(float volume);
        void SetVolumeMusic(float volume);
        void PlayMusic(Audio audio);
        void PlayOrResume(Audio audio);
        void StopMusic();
        void StopImmediateMusic();
        void ChangeMusic(Audio audio);
        void ChangeMusicImmediate(Audio audio, bool loop = true);
        void PauseMusic();
        void ResumeMusic();
        void PlaySound(Audio audio, Transform trans = null, bool allowMulti = false);
        void PlaySoundMoving(Audio audio, Transform trans = null, bool allowMulti = false);
        int PlayLoopSound(Audio audio, Transform trans = null);
        void StopLoopSound(int id);
    }
}