using System;
using System.Globalization;

using App;

using Cysharp.Threading.Tasks;

using DG.Tweening;

using Game.Dialog;

using Senspark;

using Services.Rewards;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.FarmingScene.Scripts {
    public class DialogBridgeAmount : Dialog {
        public enum Mode { Deposit, Withdraw }

        public const string ChainBsc = "BSC";
        public const string ChainPolygon = "POLYGON";

        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text balanceLabelText;
        [SerializeField] private TMP_Text balanceText;
        [SerializeField] private Image tokenIcon;
        // Real BCOIN/SEN token icons — this dialog shows the wallet's token balance,
        // so it uses the plain token icon, not the *_BRIDGE row icon. Wired in Editor.
        [SerializeField] private Sprite bcoinIcon;
        [SerializeField] private Sprite senIcon;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button confirmButton;
        [SerializeField] private GameObject blockPanel;
        [SerializeField] private GameObject networkSelector;
        [SerializeField] private Toggle bnbToggle;
        [SerializeField] private Toggle polygonToggle;
        [SerializeField] private TMP_Text netAfterFeeText;
        [SerializeField] private Color balanceNormalColor = Color.white;
        [SerializeField] private Color balanceRedColor = Color.red;

        private const string AmountFormat = "0.#########";

        private ISoundManager _soundManager;
        private double _available;
        private Mode _mode;
        private double _feePercent;
        private string _defaultChain;
        private Canvas _canvas;
        private Action<double, bool, string> _onConfirm;
        private Action _onHide;
        private bool _isInit;
        private bool _awaitingConfirm;

        public static UniTask<DialogBridgeAmount> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogBridgeAmount>();
        }

        public void Show(Mode mode, string symbol, string networkName, double available, double feePercent,
            string defaultChain, Canvas canvas, Action<double, bool, string> onConfirm, Action onHide = null) {
            InitIfNeeded();
            _onConfirm = onConfirm;
            _onHide = onHide;
            _mode = mode;
            _feePercent = feePercent;
            _defaultChain = string.IsNullOrEmpty(defaultChain) ? ChainBsc : defaultChain;
            _available = available < 0 ? 0 : available;
            _awaitingConfirm = false;

            if (titleText) {
                var action = mode == Mode.Deposit ? "DEPOSIT" : "WITHDRAW";
                titleText.text = mode == Mode.Deposit && !string.IsNullOrEmpty(networkName)
                    ? $"{action} - {networkName}"
                    : action;
            }
            if (balanceLabelText) balanceLabelText.text = mode == Mode.Deposit ? "Your Wallet:" : "Your Balance:";
            if (tokenIcon) {
                var iconSprite = (symbol ?? "").ToUpperInvariant() == "SEN" ? senIcon : bcoinIcon;
                if (iconSprite) tokenIcon.sprite = iconSprite;
            }
            balanceText.color = balanceNormalColor;
            balanceText.text = _available.ToString(AmountFormat, CultureInfo.InvariantCulture);
            inputField.text = "";

            var isWithdraw = mode == Mode.Withdraw;
            if (networkSelector) networkSelector.SetActive(isWithdraw);
            if (isWithdraw) {
                if (bnbToggle) bnbToggle.isOn = _defaultChain == ChainBsc;
                if (polygonToggle) polygonToggle.isOn = _defaultChain != ChainBsc;
            }
            if (netAfterFeeText) netAfterFeeText.gameObject.SetActive(isWithdraw);
            UpdateNetLabel();

            if (blockPanel) blockPanel.SetActive(false);

            _canvas = canvas;
            base.Show(canvas);
        }

        private void InitIfNeeded() {
            if (_isInit) return;
            _isInit = true;
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            if (inputField) inputField.onValueChanged.AddListener(_ => UpdateNetLabel());
        }

        private string SelectedChain() {
            if (_mode != Mode.Withdraw) return _defaultChain;
            if (bnbToggle) return bnbToggle.isOn ? ChainBsc : ChainPolygon;
            return _defaultChain;
        }

        private void UpdateNetLabel() {
            if (!netAfterFeeText || _mode != Mode.Withdraw) return;
            var amount = ParseInput();
            var gross = amount > 0 ? amount : 0;
            var net = gross * (100 - _feePercent) / 100;
            netAfterFeeText.text = $"You receive ~ {net.ToString(AmountFormat, CultureInfo.InvariantCulture)}";
        }

        public void OnBtnMax() {
            _soundManager.PlaySound(Audio.Tap);
            var max = FlooredMax();
            inputField.text = max.ToString(AmountFormat, CultureInfo.InvariantCulture);
        }

        public void OnBtnConfirm() {
            _soundManager.PlaySound(Audio.Tap);
            var value = ParseInput();
            if (!IsValid(value)) return;

            var isMax = value >= FlooredMax();
            var chain = SelectedChain();
            ShowConfirmSummary(value, isMax, chain);
        }

        private void ShowConfirmSummary(double value, bool isMax, string chain) {
            if (_awaitingConfirm) return;
            _awaitingConfirm = true;
            var amountStr = value.ToString(AmountFormat, CultureInfo.InvariantCulture);
            string desc;
            if (_mode == Mode.Withdraw) {
                var net = value * (100 - _feePercent) / 100;
                var netStr = net.ToString(AmountFormat, CultureInfo.InvariantCulture);
                var network = chain == ChainBsc ? "BNB Chain" : "Polygon";
                desc = $"Withdraw {amountStr}\nNetwork: {network}\nYou receive ≈ {netStr} (fee {_feePercent}%)";
            } else {
                desc = $"Deposit {amountStr}";
            }
            UniTask.Void(async () => {
                var confirm = await DialogConfirm.Create();
                if (!this) return;
                confirm.SetInfo(desc, "Confirm", "Cancel",
                        () => { _awaitingConfirm = false; Proceed(value, isMax, chain); },
                        () => { _awaitingConfirm = false; })
                    .Show(_canvas);
            });
        }

        private void Proceed(double value, bool isMax, string chain) {
            var callback = _onConfirm;
            _onConfirm = null;
            Hide();
            callback?.Invoke(value, isMax, chain);
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
            return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var v)
                ? v
                : -1;
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
