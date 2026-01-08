using System;
using System.Threading.Tasks;

using App;

using Senspark;

using UnityEngine;

namespace BomberLand.Tutorial {
    public class TutorialSelectJoystick : MonoBehaviour {
        [SerializeField]
        private UnityEngine.UI.Button[] buttons;

        private Action<int> _onChooseCallback;

        private void Awake() {
            foreach (var bt in buttons) {
                bt.interactable = false;
            }
        }

        public void Initialized(System.Action<int> chooseCallback) {
            _onChooseCallback = chooseCallback;
        }

        public void OnChooseButtonClicked(int type) {
            _onChooseCallback?.Invoke(type);
        }

        public Task<bool> CloseAsync(ISoundManager soundManager) {
            var task = new TaskCompletionSource<bool>();
            foreach (var bt in buttons) {
                bt.interactable = true;
            }
            _onChooseCallback = (int type) => {
                soundManager.PlaySound(Audio.Tap);
                var storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
                storageManager.IsJoyStickChoice = (type == 1);
                Destroy(gameObject);
                task.SetResult(true);
            };
            return task.Task;
        }
    }
}