using System;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace Senspark {
    [Service(typeof(IAudioManager))]
    public interface IAudioManager {
        /// <summary>
        /// Initializes this manager.
        /// </summary>
        [NotNull]
        Task Initialize();

        /// <summary>
        /// Gets or sets whether music is enabled.
        /// </summary>
        bool IsMusicEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether sound is enabled.
        /// </summary>
        bool IsSoundEnabled { get; set; }

        /// <summary>
        /// Gets or sets the global music volume.
        /// </summary>
        float MusicVolume { get; set; }

        /// <summary>
        /// Gets or sets the global sound volume.
        /// </summary>
        float SoundVolume { get; set; }

        /// <summary>
        /// Plays the specified music.
        /// </summary>
        void PlayMusic([CanBeNull] Enum id = null, float volume = 1f);

        /// <summary>
        /// Stops the current music.
        /// </summary>
        void StopMusic();

        /// <summary>
        /// Plays the specified sound.
        /// </summary>
        void PlaySound(Enum id, float volume = 1f);
    }
}