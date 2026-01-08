using System;

using JetBrains.Annotations;

using UnityEngine;

namespace Senspark.Internal {
    public interface IBannerAdBridge : IDisposable {
        /// <summary>
        /// Initializes the ad bridge.
        /// </summary>
        void Initialize([CanBeNull] Action<bool, string> loadCallback);

        /// <summary>
        /// Checks whether the ad is loaded.
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        /// Gets the size of the ad.
        /// </summary>
        Vector2 Size { get; }

        /// <summary>
        /// Set ad visibility.
        /// </summary>
        /// <param name="visible"></param>
        void SetVisible(bool visible);

        /// <summary>
        /// Sets relative position.
        /// </summary>
        void SetPosition(AdPosition position);

        /// <summary>
        /// Sets absolute position.
        /// </summary>
        void SetPosition(Vector2 position);
    }
}