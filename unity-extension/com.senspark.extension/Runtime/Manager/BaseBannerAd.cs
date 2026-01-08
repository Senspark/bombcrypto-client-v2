using System.Threading.Tasks;

using JetBrains.Annotations;

using UnityEngine;

namespace Senspark.Internal {
    public class BaseBannerAd : ObserverManager<AdObserver>, IBannerAd {
        [Inject]
        private readonly ILogManager _logManager;

        [NotNull]
        private readonly IBannerAdBridge _bridge;

        private bool _initialized;
        private int _counter;
        private bool _useRelativePosition;
        private AdPosition? _relativePosition;
        private Vector2? _position;
        private Vector2? _anchor;

        public bool IsLoaded => _bridge.IsLoaded;

        public bool IsVisible {
            get => _counter > 0;
            set {
                if (value) {
                    ++_counter;
                } else {
                    --_counter;
                }
                UpdateVisibility();
            }
        }

        public AdPosition RelativePosition {
            get => _relativePosition ?? AdPosition.Bottom;
            set {
                if (_relativePosition == value) {
                    return;
                }
                _relativePosition = value;
                _useRelativePosition = true;
                UpdatePosition();
            }
        }

        public Vector2 Position {
            get => _position ?? Vector2.zero;
            set {
                if (_position == value) {
                    return;
                }
                _position = value;
                _useRelativePosition = false;
                UpdatePosition();
            }
        }

        public Vector2 Anchor {
            get => _anchor ?? Vector2.zero;
            set {
                if (_anchor == value) {
                    return;
                }
                _anchor = value;
                _useRelativePosition = false;
                UpdatePosition();
            }
        }

        public Vector2 Size => _bridge.Size;

        public BaseBannerAd(
            [NotNull] IServiceLocator serviceLocator,
            [NotNull] IBannerAdBridge bridge
        ) {
            serviceLocator.ResolveInjection(this);
            _bridge = bridge;
            _useRelativePosition = true;
        }

        public void Dispose() {
            _bridge.Dispose();
        }

        public void Initialize() {
            _bridge.Initialize((loaded, message) => {
                if (loaded) {
                    DispatchEvent(observer => observer.OnLoaded?.Invoke());
                    UpdatePosition();
                } else {
                    DispatchEvent(observer => observer.OnFailedToLoad?.Invoke(message));
                }
            });
            _initialized = true;
            UpdateVisibility();
            UpdatePosition();
        }

        public Task<bool> Load() {
            if (!_initialized) {
                return Task.FromResult(false);
            }
            // Auto-load mechanism.
            return Task.FromResult(true);
        }

        private void UpdateVisibility() {
            if (!_initialized) {
                return;
            }
            _bridge.SetVisible(IsVisible);
            UpdatePosition();
        }

        private void UpdatePosition() {
            if (!_initialized) {
                return;
            }
            if (!IsLoaded) {
                return;
            }
            if (!IsVisible) {
                return;
            }
            if (_useRelativePosition) {
                _bridge.SetPosition(RelativePosition);
            } else {
                var screenSize = new Vector2(Screen.width, Screen.height);
                var position = Position;
                var anchor = Anchor;
                var size = _bridge.Size;
                var anchoredPosition =
                    new Vector2(position.x, screenSize.y - position.y) -
                    new Vector2(anchor.x, 1 - anchor.y) * size;
                _bridge.SetPosition(anchoredPosition);
            }
        }
    }
}