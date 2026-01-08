using System;

using App;

using Constant;

using Senspark;

using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class SwapTokenButton : MonoBehaviour {
        [SerializeField]
        private Sprite polygonSenIcon, bscSenIcon, polygonBcoinIcon, bscBcoinIcon;

        [SerializeField]
        private GameObject changeTokenOption; 

        [SerializeField]
        private Image _iconToken, _iconTokenChange;
        
        [SerializeField]
        private Text _textToken, _textTokenChange;
        
        private INetworkConfig _network;
        private RewardType _currentType;
        private Action<RewardType> _onChangeTokenCallback;

        private void Awake() {
            _network = ServiceLocator.Instance.Resolve<INetworkConfig>();
            _currentType = RewardType.BCOIN;
            UpdateUI();
        }

        private void UpdateUI() {
            if (_currentType == RewardType.BCOIN) {
                _textToken.text = "BCOIN";
                _textTokenChange.text = "SEN";
                if (_network.NetworkType == NetworkType.Binance) {
                    _iconToken.sprite = bscBcoinIcon;
                    _iconTokenChange.sprite = bscSenIcon;
                } else {
                    _iconToken.sprite = polygonBcoinIcon;
                    _iconTokenChange.sprite = polygonSenIcon;
                }
            } else {
                _textToken.text = "SEN";
                _textTokenChange.text = "BCOIN";
                if (_network.NetworkType == NetworkType.Binance) {
                    _iconToken.sprite = bscSenIcon;
                    _iconTokenChange.sprite = bscBcoinIcon;
                } else {
                    _iconToken.sprite = polygonSenIcon;
                    _iconTokenChange.sprite = polygonBcoinIcon;
                }
            }
        }

        public void SetChangeTokenCallback(Action<RewardType> callback) {
            _onChangeTokenCallback = callback;
        }

        public void OnButtonChangeTokenClicked() {
            if (changeTokenOption.activeSelf) {
                changeTokenOption.SetActive(false);
            } else {
                changeTokenOption.SetActive(true);
            }
        }

        public void OnChangeToken() {
            changeTokenOption.SetActive(false);
            _currentType = _currentType == RewardType.BCOIN ? RewardType.Senspark : RewardType.BCOIN;
            _onChangeTokenCallback?.Invoke(_currentType);
            UpdateUI();
        }

        public void HideChangeToken() {
            changeTokenOption.SetActive(false);
        }
    }
}