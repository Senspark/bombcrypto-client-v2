using System;
using System.Threading.Tasks;

using App;

using UnityEngine;

namespace BomberLand.Tutorial {
    public class TutorialFullRankBooster : MonoBehaviour {
        [SerializeField]
        private UnityEngine.UI.Button btClaim;

        public UnityEngine.UI.Button BtClaim => btClaim;

        private Action _onClaimCallback;

        private void Awake() {
            btClaim.interactable = false;
        }

        public void Initialized(System.Action claimCallback) {
            _onClaimCallback = claimCallback;
        }

        public void OnClaimButtonClicked() {
            _onClaimCallback?.Invoke();
        }

        public Task<bool> WaitClose(ISoundManager soundManager) {
            btClaim.interactable = true;
            var task = new TaskCompletionSource<bool>();
            _onClaimCallback = () => {
                soundManager.PlaySound(Audio.Tap);
                task.SetResult(true);
            };
            return task.Task;
        }
    }
}