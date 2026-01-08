using System;
using App;
using BomberLand.Button;
using Cysharp.Threading.Tasks;
using Game.Dialog;
using Senspark;
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

        private BuyOption _buyOption = BuyOption.Buy7Days;
        private HouseType _houseType;
        private float _price;
        private Action<int> _btnRenHouseCallback;
        private BlockRewardType _rewardType;
        private float _availableValue;

        public static UniTask<DialogRentLandAirdrop> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogRentLandAirdrop>();
        }

        private void Start() {
            _houseStore = ServiceLocator.Instance.Resolve<IStorageManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            InitAirdrop();
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
            description.text = DefaultHouseStoreManager.GetHouseName(houseType) + " Land for 7 days";
            buttonXs[(int)_buyOption].SetActive(true);
            _price = GetPriceRentHouse(_houseType, ConvertBuyOptionToDay(_buyOption));
            //DevHoang_20250715: Another format for Base
            if (AppConfig.IsBase()) {
                price.text = App.Utils.FormatBaseValue(_price);
            } else {
                price.text = App.Utils.FormatBcoinValue(_price);
            }
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
            _price = GetPriceRentHouse(_houseType, ConvertBuyOptionToDay(_buyOption));
            //DevHoang_20250715: Another format for Base
            if (AppConfig.IsBase()) {
                price.text = App.Utils.FormatBaseValue(_price);
            } else {
                price.text = App.Utils.FormatBcoinValue(_price);
            }
            
            description.text = DefaultHouseStoreManager.GetHouseName(_houseType) +
                               $" Land for {ConvertBuyOptionToDay(_buyOption)} days";
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

        private float GetPriceRentHouse(HouseType houseType, int numDays) {
            var packages = _houseStore.RentHousePackConfigs.Packages;
            foreach (var package in packages) {
                if (package.NumDays == numDays && package.Rarity == (int)houseType) {
                    return package.Price;
                }
            }
            return 0f;
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