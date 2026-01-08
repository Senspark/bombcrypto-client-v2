using System.Threading.Tasks;

using App;

using UnityEngine;

namespace BomberLand.Tutorial {
    public class TutorialActivatedBooster : MonoBehaviour {
        private System.Action _onOkCallback;

        public void Initialized(System.Action okCallback) {
            _onOkCallback = okCallback;
        }

        public void OnOkButtonClicked() {
            _onOkCallback?.Invoke();
        }

        public Task<bool> WaitClose(ISoundManager soundManager) {
            var task = new TaskCompletionSource<bool>();
            _onOkCallback = () => {
                soundManager.PlaySound(Audio.Tap);
                Destroy(gameObject);
                task.SetResult(true);
            };
            return task.Task;
        }
    }
}