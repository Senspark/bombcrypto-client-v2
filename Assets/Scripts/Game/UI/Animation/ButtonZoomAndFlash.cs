using Coffee.UIEffects;

using Engine.Utils;

using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Animation {
    public class ButtonZoomAndFlash : MonoBehaviour {
        [SerializeField]
        private Button button;

        [SerializeField]
        private float shinyWidth = 0.1f;

        [SerializeField]
        private float shinyRotation = 135f;

        [SerializeField]
        private float duration = 1f;

        [SerializeField]
        private bool loop;

        [SerializeField]
        private float loopDelay = 3f;

        private UIShiny _shiny;
        private AnimationZoom _zoom;
        private bool _isPlaying;

        public bool Interactable {
            get => button.interactable;
            set {
                button.interactable = value;
                if (value) {
                    Play();
                } else {
                    Stop();
                }
            }
        }

        private void Awake() {
            AddShiny();
            AddZoom();
            Interactable = true;
        }

        public void SetActive(bool value) {
            button.gameObject.SetActive(value);
            if (!value) {
                return;
            }
            if (button.interactable) {
                Play();
            }
        }

        private void AddShiny() {
            _shiny = gameObject.AddComponent<UIShiny>();
            _shiny.width = shinyWidth;
            _shiny.rotation = shinyRotation;
            _shiny.effectPlayer.duration = duration;
            _shiny.effectPlayer.loopDelay = loopDelay;
            _shiny.effectPlayer.loop = loop;
        }

        private void AddZoom() {
            _zoom = gameObject.AddComponent<AnimationZoom>();
            _zoom.duration = duration;
            _zoom.loopDelay = loopDelay;
            _zoom.loop = loop;
        }

        private void Play() {
            if (_isPlaying) {
                return;
            }
            _shiny.Play(true);
            _zoom.Play();
            _isPlaying = true;
        }

        private void Stop() {
            if (_shiny == null || _zoom == null) {
                return;
            }
            _shiny.Stop();
            _zoom.Stop();
            _isPlaying = false;
        }
    }
}