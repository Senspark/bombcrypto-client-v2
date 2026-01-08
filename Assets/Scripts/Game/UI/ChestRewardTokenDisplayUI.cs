using System;
using System.Collections.Generic;
using System.Linq;
using App;
using Senspark;
using GroupMainMenu;

using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public enum TokenMineType {
        Unknown,
        Tr,
        Network
    }
    /// <summary>
    /// Class này display giá trị tổng của các tokens đã chọn
    /// </summary>
    public class ChestRewardTokenDisplayUI : MonoBehaviour {
        [SerializeField]
        private Text coinTxt;

        [SerializeField]
        private List<BlockRewardType> tokens;
        
        [SerializeField]
        private TokenMineType tokenMineType;

        [SerializeField]
        private Button button;

        [SerializeField]
        private MMButton buttonPlus;
        
        [SerializeField]
        private bool formatIntegral;

        [SerializeField]
        private bool formatFloat = false;
        
        private IChestRewardManager _chestRewardManager;
        private INetworkConfig _networkConfig;
        private ObserverHandle _handle;

        private Action<BlockRewardType> _onClickedCallback;

        public bool Interactable {
            get => button != null && button.interactable;
            set {
                if (button != null) {
                    button.interactable = value;
                    buttonPlus.Interactable = value;
                }
            }
        }

        private void Awake() {
            InitWebAirdrop();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            _networkConfig =  ServiceLocator.Instance.Resolve<INetworkConfig>();
            _handle = new ObserverHandle();
            _handle.AddObserver(_chestRewardManager, new ChestRewardManagerObserver() {
                    OnRewardChanged = UpdateValue,
            });
            UpdateValue();
        }

        private void OnDestroy() {
            _handle?.Dispose();
        }

        private void InitWebAirdrop() {
            //DevHoang: Add new airdrop
            if (tokens.Contains(BlockRewardType.RonDeposited) && !AppConfig.IsRonin()) {
                this.gameObject.SetActive(false);
                return;
            }
            
            if (tokens.Contains(BlockRewardType.BasDeposited) && !AppConfig.IsBase()) {
                this.gameObject.SetActive(false);
                return;
            }
            
            if (tokens.Contains(BlockRewardType.VicDeposited) && !AppConfig.IsViction()) {
                this.gameObject.SetActive(false);
                return;
            }
        }

        private void UpdateValue(BlockRewardType type, double value) {
            if (!tokens.Contains(type)) {
                return;
            }
            UpdateValue();
        }

        private void UpdateValue() {
            var value = 0f;
            if (tokenMineType == TokenMineType.Network) {
                var network = _networkConfig.NetworkType;
                var dataType = RewardUtils.ConvertNetworkToDatatype(network);
                value = tokens.Sum(t => _chestRewardManager.GetChestReward(t, dataType.ToString()));

            } else {
                value = tokens.Sum(t => _chestRewardManager.GetChestReward(t));
            }
            var formatValue = "";
            //DevHoang_20250715: Another format for Base
            if (AppConfig.IsBase()) {
                formatValue = App.Utils.FormatBaseValue(value);
            } else {
                formatValue = App.Utils.FormatBcoinValue(value);
            }
            if (formatFloat) {
                coinTxt.text = value > 0 ? formatValue : "0";
                return;
            }
            // var str = formatIntegral ? App.Utils.ConvertToShortString((int) value) : App.Utils.FormatBcoinValue(value);
            // Tạm thời không convert short với K,M mà giữ Best Fit.
            var str = formatIntegral ? Math.Truncate(value).ToString("N0") : formatValue;
            coinTxt.text = str;
        }

        public void SetOnClickedCallback(Action<BlockRewardType> callback) {
            if (button == null) {
                return;
            }
            _onClickedCallback = callback;
            button.onClick.AddListener(OnClicked);
        }
        
        private void OnClicked() {
            _onClickedCallback?.Invoke(tokens[0]);
        }
        
    }
}