using System;
using System.Globalization;

using App;

using Cysharp.Threading.Tasks;

using DG.Tweening;

using Game.Dialog;

using Senspark;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.FarmingScene.Scripts {
    // Amount entry for a native-coin (BNB / POL) deposit. Single chain (native is not cross-chain),
    // decimal input, no fee. Withdraw is withdraw-MAX so it needs no amount dialog. Modeled on
    // DialogBridgeAmount but trimmed to the native case. Prefab fields are wired in the Unity Editor.
    public class DialogNativeAmount : Dialog {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text balanceLabelText;
        [SerializeField] private TMP_Text balanceText;
        [SerializeField] private Image tokenIcon;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Color balanceNormalColor = Color.white;
        [SerializeField] private Color balanceRedColor = Color.red;

        private const string AmountFormat = "0.#########";

        private ISoundManager _soundManager;
        private double _available;
        private Canvas _canvas;
        private Action<double> _onConfirm;
        private Action _onHide;
        private bool _isInit;
        private bool _awaitingConfirm;

        public static UniTask<DialogNativeAmount> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogNativeAmount>();
        }

        // available = the user's native coin balance (display-only cap). icon may be null until the
        // LaunchPadResource icon is wired; the field simply keeps its prefab default in that case.
        public void Show(string symbol, string networkName, double available, Sprite icon, Canvas canvas,
            Action<double> onConfirm, Action onHide = null) {
            InitIfNeeded();
            _onConfirm = onConfirm;
            _onHide = onHide;
            _available = available < 0 ? 0 : available;
            _awaitingConfirm = false;

            if (titleText) {
                titleText.text = string.IsNullOrEmpty(networkName) ? "DEPOSIT" : $"DEPOSIT - {networkName}";
            }
            if (balanceLabelText) balanceLabelText.text = "Your Wallet:";
            if (tokenIcon && icon) tokenIcon.sprite = icon;
            balanceText.color = balanceNormalColor;
            balanceText.text = _available.ToString(AmountFormat, CultureInfo.InvariantCulture);
            inputField.text = "";

            _canvas = canvas;
            base.Show(canvas);
        }

        private void InitIfNeeded() {
            if (_isInit) return;
            _isInit = true;
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        }

        public void OnBtnMax() {
            _soundManager.PlaySound(Audio.Tap);
            inputField.text = FlooredMax().ToString(AmountFormat, CultureInfo.InvariantCulture);
        }

        public void OnBtnConfirm() {
            _soundManager.PlaySound(Audio.Tap);
            var value = ParseInput();
            if (!IsValid(value)) return;
            ShowConfirmSummary(value);
        }

        private void ShowConfirmSummary(double value) {
            if (_awaitingConfirm) return;
            _awaitingConfirm = true;
            var amountStr = value.ToString(AmountFormat, CultureInfo.InvariantCulture);
            var desc = $"Deposit {amountStr}";
            UniTask.Void(async () => {
                var confirm = await DialogConfirm.Create();
                if (!this) return;
                confirm.SetInfo(desc, "Confirm", "Cancel",
                        () => { _awaitingConfirm = false; Proceed(value); },
                        () => { _awaitingConfirm = false; })
                    .Show(_canvas);
            });
        }

        private void Proceed(double value) {
            var callback = _onConfirm;
            _onConfirm = null;
            Hide();
            callback?.Invoke(value);
        }

        public void OnBtnHide() {
            _soundManager.PlaySound(Audio.Tap);
            _onHide?.Invoke();
            _onHide = null;
            Hide();
        }

        private double FlooredMax() {
            return Math.Floor(_available * 1e9) / 1e9;
        }

        private double ParseInput() {
            var text = inputField.text;
            if (string.IsNullOrWhiteSpace(text)) return -1;
            text = text.Trim().Replace(',', '.');
            if (text.IndexOf('.') != text.LastIndexOf('.')) return -1;
            return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : -1;
        }

        private bool IsValid(double value) {
            if (value > 0 && value <= _available) return true;
            balanceText.DOColor(balanceRedColor, 0.5f)
                .OnComplete(() => balanceText.DOColor(balanceNormalColor, 1f));
            return false;
        }

        protected override void OnYesClick() {
            if (confirmButton && !confirmButton.IsInteractable()) return;
            OnBtnConfirm();
        }

        protected override void OnNoClick() {
            OnBtnHide();
        }
    }
}
