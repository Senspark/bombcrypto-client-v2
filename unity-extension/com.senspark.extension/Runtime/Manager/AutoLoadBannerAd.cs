using JetBrains.Annotations;

using UnityEngine;

namespace Senspark.Internal {
    public class AutoLoadBannerAd : AutoLoadAd, IBannerAd {
        [NotNull]
        private readonly IBannerAd _ad;

        public AutoLoadBannerAd([NotNull] IBannerAd ad) : base(ad) {
            _ad = ad;
        }

        public bool IsVisible {
            get => _ad.IsVisible;
            set => _ad.IsVisible = value;
        }

        public AdPosition RelativePosition {
            get => _ad.RelativePosition;
            set => _ad.RelativePosition = value;
        }

        public Vector2 Position {
            get => _ad.Position;
            set => _ad.Position = value;
        }

        public Vector2 Anchor {
            get => _ad.Anchor;
            set => _ad.Anchor = value;
        }

        public Vector2 Size => _ad.Size;
    }
}