using App;

using Senspark;

using Share.Scripts.Dialog;

using UnityEngine;

namespace Scenes.FarmingScene.Scripts {
    public class ClaimReadyPrompt : MonoBehaviour {
        [SerializeField]
        private Canvas dialogCanvas;

        private ObserverHandle _handle;
        private IClaimTokenManager _claimManager;

        private void Awake() {
            _claimManager = ServiceLocator.Instance.Resolve<IClaimTokenManager>();
            _handle = new ObserverHandle();
            _handle.AddObserver(_claimManager, new ClaimTokenObserver {
                OnClaimFinalized = OnClaimFinalized,
            });
        }

        private void OnDestroy() {
            _handle?.Dispose();
        }

        private void OnClaimFinalized(ClaimEndReason reason) {
            switch (reason) {
                case ClaimEndReason.PushTimeout:
                case ClaimEndReason.ConnectionLost:
                    DialogOK.ShowInfo(dialogCanvas, "Claim Expired",
                        "Your claim expired. Please retry.");
                    break;
                case ClaimEndReason.ServerError:
                    DialogOK.ShowInfo(dialogCanvas, "Claim Failed",
                        "Your claim failed. Please retry.");
                    break;
            }
        }
    }
}
