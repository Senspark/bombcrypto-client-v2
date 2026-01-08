using System;
using System.Threading.Tasks;

namespace Senspark {
    public class NullAudioManager : IAudioManager {
        public bool IsMusicEnabled { get; set; }
        public bool IsSoundEnabled { get; set; }
        public float MusicVolume { get; set; }
        public float SoundVolume { get; set; }
        public Task Initialize() => Task.CompletedTask;
        public void PlayMusic(Enum id = null, float volume = 1) { }
        public void StopMusic() { }
        public void PlaySound(Enum id, float volume = 1) { }
    }
}