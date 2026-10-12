using System;
using App;
using BomberLand.Button;
using Cysharp.Threading.Tasks;
using Game.Dialog;
using Senspark;
using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.FarmingScene.Scripts {
    public class DialogRentLandAirdrop : Dialog {
        private enum BuyOption {
            Buy7Days,
            Buy14Days,
            Buy30Days
        }

        [SerializeField]
        private Text description;

        [SerializeField]
        private Text price;

        [SerializeField]
        private XButton[] buttonXs;
        
        // Change icon
        [SerializeField]
        private Image priceIcon;
    
        [SerializeField]
        private AirdropRewardTypeResource airdropRewardRes;

        private IStorageManager _houseStore;
        private ISoundManager _soundManager;
        private IChestRewardManager _chestRewardManager;
        private ILanguageManager _languageManager;

        private BuyOption _buyOption = BuyOption.Buy7Days;
        private HouseType _houseType;
        private double _price;
        private Action<int> _btnRenHouseCallback;
        private BlockRewardType _rewardType;
        private float _availableValue;
        private NativeTokenInfo _nativeToken;

        public static UniTask<DialogRentLandAirdrop> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogRentLandAirdrop>();
        }

        protected override void Awake() {
            base.Awake();
            _houseStore = ServiceLocator.Instance.Resolve<IStorageManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            _languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
            _nativeToken = NativeTokenInfo.Of(
                ServiceLocator.Instance.Resolve<INetworkConfig>(),
                ServiceLocator.Instance.Resolve<ILaunchPadManager>());
            if (_nativeToken != null) {
                InitNative();
            } else {
                InitAirdrop();
            }
        }

        // BSC / POLYGON pay the rent in the chain's native deposit
        private void InitNative() {
            _rewardType = _nativeToken.Type;
            if (_nativeToken.Icon) {
                priceIcon.sprite = _nativeToken.Icon;
            }
            _availableValue = _chestRewardManager.GetChestReward(_nativeToken.Type, _nativeToken.Network);
        }

        private void InitAirdrop() {
            //DevHoang: Add new airdrop
            _rewardType = BlockRewardType.BCoin;
            if (AppConfig.IsRonin()) {
                _rewardType = BlockRewardType.RonDeposited;
                priceIcon.sprite = airdropRewardRes.GetAirdropIcon(_rewardType);
            }
            if (AppConfig.IsBase()) {
                _rewardType = BlockRewardType.BasDeposited;
                priceIcon.sprite = airdropRewardRes.GetAirdropIcon(_rewardType);
            }
            if (AppConfig.IsViction()) {
                _rewardType = BlockRewardType.VicDeposited;
                priceIcon.sprite = airdropRewardRes.GetAirdropIcon(_rewardType);
            }
            
            switch (_rewardType) {
                case BlockRewardType.BCoin:
                    _availableValue = _chestRewardManager.GetChestReward(BlockRewardType.BCoinDeposited);
                    break;
                case BlockRewardType.RonDeposited:
                    _availableValue = _chestRewardManager.GetChestReward(BlockRewardType.RonDeposited);
                    break;
                case BlockRewardType.BasDeposited:
                    _availableValue = _chestRewardManager.GetChestReward(BlockRewardType.BasDeposited);
                    break;
                case BlockRewardType.VicDeposited:
                    _availableValue = _chestRewardManager.GetChestReward(BlockRewardType.VicDeposited);
                    break;
            }
        }

        public void SetInfo(HouseType houseType, Action<int> callback) {
            _houseType = houseType;
            buttonXs[(int)_buyOption].SetActive(true);
            UpdatePrice();
            _btnRenHouseCallback = callback;
        }

        public void OnBuyOptionClicked(XButton button) {
            _soundManager.PlaySound(Audio.Tap);
            foreach (var iter in buttonXs) {
                if (iter == button) {
                    iter.SetActive(true);
                    _buyOption = (BuyOption)iter.Index;
                } else {
                    iter.SetActive(false);
                }
            }
            UpdatePrice();
        }

        private void UpdatePrice() {
            var numDays = ConvertBuyOptionToDay(_buyOption);
            _price = GetPriceRentHouse(_houseType, numDays);
            if (_nativeToken != null) {
                price.text = ServerRewardTypes.FormatAmount(_price);
            } else if (AppConfig.IsBase()) {
                //DevHoang_20250715: Another format for Base
                price.text = App.Utils.FormatBaseValue(_price);
            } else {
                price.text = App.Utils.FormatBcoinValue(_price);
            }

            description.text = DefaultHouseStoreManager.GetHouseName(_houseType) + $" Land for {numDays} days";
            if (_nativeToken != null) {
                // The server cancels the rent when the house is transferred and can only find out at the next sync
                description.text += "\nRent ends if this land leaves your wallet. No refund.";
            }
        }

        public void OnRentHouseClicked() {
            _soundManager.PlaySound(Audio.Tap);
            UniTask.Void(async () => {
                var result = await CheckEnoughResource();
                if (result) {
                    _btnRenHouseCallback.Invoke(ConvertBuyOptionToDay(_buyOption));
                    Hide();
                }
            });
        }
        
        private async UniTask<bool> CheckEnoughResource() {
            if (_availableValue < _price) {
                if (_nativeToken != null) {
                    var message = string.Format(_languageManager.GetValue(LocalizeKey.info_not_enough),
                        _nativeToken.DisplayName);
                    DialogOK.ShowInfo(DialogCanvas, _languageManager.GetValue(LocalizeKey.ui_not_enough), message);
                    return false;
                }
                switch (_rewardType) {
                    //DevHoang: Add new airdrop
                    case BlockRewardType.BCoin:
                        var dialogBcoin = await DialogNotEnoughRewardAirdrop.Create(BlockRewardType.BCoinDeposited);
                        dialogBcoin.Show(DialogCanvas);
                        break;
                    case BlockRewardType.RonDeposited:
                        var dialogRon = await DialogNotEnoughRewardAirdrop.Create(BlockRewardType.RonDeposited);
                        dialogRon.Show(DialogCanvas);
                        break;
                    case BlockRewardType.BasDeposited:
                        var dialogBas = await DialogNotEnoughRewardAirdrop.Create(BlockRewardType.BasDeposited);
                        dialogBas.Show(DialogCanvas);
                        break;
                    case BlockRewardType.VicDeposited:
                        var dialogVic = await DialogNotEnoughRewardAirdrop.Create(BlockRewardType.VicDeposited);
                        dialogVic.Show(DialogCanvas);
                        break;
                }
                return false;
            }
            return true;
        }

        private double GetPriceRentHouse(HouseType houseType, int numDays) {
            var packages = _houseStore.RentHousePackConfigs.Packages;
            foreach (var package in packages) {
                if (package.NumDays == numDays && package.Rarity == (int)houseType) {
                    return _nativeToken != null ? package.Prices.FindNative()?.Price ?? 0 : package.Price;
                }
            }
            return 0;
        }

        private int ConvertBuyOptionToDay(BuyOption buyOption) {
            return buyOption switch {
                BuyOption.Buy7Days => 7,
                BuyOption.Buy14Days => 14,
                BuyOption.Buy30Days => 30,
                _ => 7
            };
        }

        public void OnCloseButtonClicked() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }
    }
}