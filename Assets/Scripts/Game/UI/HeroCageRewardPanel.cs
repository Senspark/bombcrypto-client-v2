using System;

using App;

using Cysharp.Threading.Tasks;

using Game.Manager;

using Senspark;

using Share.Scripts.Dialog;

using UnityEngine;

namespace Game.UI {
    public class HeroCageRewardPanel : MonoBehaviour {
        private IServerManager _serverManager;
        private Canvas _dialogCanvas;
        private bool _isClaiming;

        public bool IsAnswered { get; private set; }

        private void Awake() {
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
        }

        public void Show(Canvas dialogCanvas) {
            if (IsAnswered) {
                return;
            }
            _dialogCanvas = dialogCanvas;
            gameObject.SetActive(true);
        }

        public void Claim(NetworkTypeInServer network) {
            if (_isClaiming || IsAnswered) {
                return;
            }
            _isClaiming = true;
            var waiting = new WaitingUiManager(_dialogCanvas);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    await _serverManager.General.ClaimHeroCage(network);
                } catch (Exception e) {
                    DialogOK.ShowError(_dialogCanvas, e);
                } finally {
                    waiting.End();
                    // Answered even on failure (e.g. the 5-minute offer expired) so the host dialog never gets stuck.
                    IsAnswered = true;
                    gameObject.SetActive(false);
                }
            });
        }
    }
}
