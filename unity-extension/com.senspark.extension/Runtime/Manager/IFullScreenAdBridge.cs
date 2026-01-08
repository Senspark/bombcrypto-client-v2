using System;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace Senspark.Internal {
    public interface IFullScreenAdBridge : IDisposable {
        /// <summary>
        /// Initializes the ad bridge.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Checks whether the ad is loaded.
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        /// Attempts to load the ad.
        /// </summary>
        [MustUseReturnValue]
        [NotNull]
        Task<(bool, string)> Load();

        /// <summary>
        /// Attempts to show the ad.
        /// </summary>
        [MustUseReturnValue]
        [NotNull]
        Task<(AdResult, string)> Show();
    }
}