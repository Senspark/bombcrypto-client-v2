using UnityEngine;

namespace Senspark.Internal {
    public interface IBannerAd : IAd {
        /// <summary>
        /// Gets or set the visibility of this ad.
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>
        /// Gets or set the relative position.
        /// </summary>
        AdPosition RelativePosition { get; set; }

        /// <summary>
        /// Gets or set the absolute position.
        /// </summary>
        Vector2 Position { get; set; }

        /// <summary>
        /// Gets or set the anchor.
        /// </summary>
        Vector2 Anchor { get; set; }

        /// <summary>
        /// Gets the size of the ad.
        /// </summary>
        Vector2 Size { get; }
    }
}