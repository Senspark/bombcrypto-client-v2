using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;

using Senspark;

using Services.Rewards;

using Share.Scripts.PrefabsManager;
using Share.Scripts.Social;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.FarmingScene.Scripts {
    public class DialogBCoinReward : Dialog {
        [SerializeField]
        private Text tokenTxt;

        [SerializeField]
        private Image tokenIcon;

        private ISoundManager _soundManager;

        public static UniTask<DialogBCoinReward> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogBCoinReward>();

        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        }

        // canvas: dùng cho DialogOK báo lỗi share ở WebGL. Bơm context cho nút Share on X ngay tại
        // đây vì SetReward là data-sink tự nhiên (đã giữ đúng token + amount) -> không nhân đôi wiring
        // ở các call site bên BLDialogReward.
        public DialogBCoinReward SetReward(TokenData data, double amount, Canvas canvas) {
            if (data != null) {
                tokenTxt.text = $"{App.Utils.FormatBcoinValue(amount)} {data.displayName}";
                tokenIcon.sprite = data.icon;

                var shareButton = GetComponentInChildren<ShareOnXButton>(true);
                if (shareButton) {
                    shareButton.SetContext(canvas, SharePayload.ForToken(data.displayName, amount));
                }
            }
            return this;
        }

        public void OnOkBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }
    }
}