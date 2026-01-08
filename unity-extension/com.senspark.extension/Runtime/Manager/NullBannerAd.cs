using System.Threading.Tasks;

using UnityEngine;

namespace Senspark.Internal {
    public class NullBannerAd : ObserverManager<AdObserver>, IBannerAd {
        public bool IsLoaded => false;
        public bool IsVisible { get; set; }
        public AdPosition RelativePosition { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Anchor { get; set; }
        public Vector2 Size => Vector2.zero;
        public void Dispose() { }
        public void Initialize() { }
        public Task<bool> Load() => Task.FromResult(false);
    }
}