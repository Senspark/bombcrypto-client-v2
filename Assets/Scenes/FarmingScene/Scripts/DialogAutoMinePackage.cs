using System;
using Analytics;
using App;
using Cysharp.Threading.Tasks;
using Game.Dialog;
using Game.Manager;
using Game.UI.Information;
using Senspark;
using Server.Models;
using Services.Rewards;
using Services.Server.Exceptions;
using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.FarmingScene.Scripts {
    public class DialogAutoMinePackage : Dialog {
        [SerializeField]
        private Text pricePackage7DayTxt;

        [SerializeField]
        private Text pricePackage30DayTxt;

        [SerializeField]
        private Button package7DayBtn;

        [SerializeField]
        private Button package30DayBtn;
        
        //DevHoang_20250715: Only for web airdrop
        [SerializeField]
        private AirdropRewardTypeResource airdropRewardRes;

        [SerializeField]
        private Image[] airdropIcon;
        
        private const int NumOfPackage7Day = 7;
        private const int NumOfPackage30Day = 30;

        private IAutoMinePackageDetail _packageDetail7Days;
        private IAutoMinePackageDetail _packageDetail30Days;
        private BlockRewardType _rewardType;

        private IStorageManager _storageManager;
        private ISoundManager _soundManager;
        private IServerManager _serverManager;
        private IChestRewardManager _chestRewardManager;
        private ILanguageManager _languageManager;
        private ILaunchPadManager _launchPadManager;
        private IUserAccountManager _userAccountManager;
        private IAnalytics _analytics;
        private ObserverHandle _handle;
        
        public static UniTask<DialogAutoMinePackage> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogAutoMinePackage>();
        }

        protected override void Awake() {
            base.Awake();
            _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            _languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
            _launchPadManager = ServiceLocator.Instance.Resolve<ILaunchPadManager>();
            _userAccountManager = ServiceLocator.Instance.Resolve<IUserAccountManager>();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
        }

        private void Start() {
            Init();
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            _handle?.Dispose();
        }
        
        private void Init() {
            InitAirdrop();
            var packages = _storageManager.AutoMinePackages;
            _packageDetail7Days = packages[0];
            _packageDetail30Days = packages[1];

            if (AppConfig.IsAirDrop()) {
                //DevHoang_20250715: Another format for Base
                if (AppConfig.IsBase()) {
                    pricePackage7DayTxt.text = App.Utils.FormatBaseValue(_packageDetail7Days.Price);
                    pricePackage30DayTxt.text = App.Utils.FormatBaseValue(_packageDetail30Days.Price);
                } else {
                    pricePackage7DayTxt.text = App.Utils.FormatBcoinValue(_packageDetail7Days.Price);
                    pricePackage30DayTxt.text = App.Utils.FormatBcoinValue(_packageDetail30Days.Price);
                }
                package7DayBtn.interactable = CanBuy(_packageDetail7Days);
                package30DayBtn.interactable = CanBuy(_packageDetail30Days);
                
                foreach (var airdrop in airdropIcon) {
                    airdrop.sprite = airdropRewardRes.GetAirdropIcon(_rewardType);
                }
            } else {
                pricePackage7DayTxt.text = $"{_packageDetail7Days.Price}";
                pricePackage30DayTxt.text = $"{_packageDetail30Days.Price}";
                package7DayBtn.interactable = CanBuy(_packageDetail7Days);
                package30DayBtn.interactable = CanBuy(_packageDetail30Days);
            }
        }
        
        private void InitAirdrop() {
            //DevHoang: Add new airdrop
            _rewardType = BlockRewardType.BCoin;
            if (AppConfig.IsSolana()) {
                _rewardType = BlockRewardType.SolDeposited;
            }
            if (AppConfig.IsTon()) {
                _rewardType = BlockRewardType.TonDeposited;
            }
            if (AppConfig.IsRonin()) {
                _rewardType = BlockRewardType.RonDeposited;
            }
            if (AppConfig.IsBase()) {
                _rewardType = BlockRewardType.BasDeposited;
            }
            if (AppConfig.IsViction()) {
                _rewardType = BlockRewardType.VicDeposited;
            }
        }
        
        public void OnBuyPackage7DayClicked() {
            _soundManager.PlaySound(Audio.Tap);
            var package = _packageDetail7Days;
            if (!CanBuy(package)) {
                ShowNotEnoughMoney();
                return;
            }
            ConfirmBuy(NumOfPackage7Day, () => BuyPackage(package));
        }

        public void OnBuyPackage30DayClicked() {
            _soundManager.PlaySound(Audio.Tap);
            var package = _packageDetail30Days;
            if (!CanBuy(package)) {
                ShowNotEnoughMoney();
                return;
            }
            ConfirmBuy(NumOfPackage30Day, () => BuyPackage(package));
        }
        private void BuyPackage(IAutoMinePackageDetail detail) {
            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    var enableAutoMine = false;
                    if (AppConfig.IsSolana()) {
                        await _serverManager.UserSolanaManager.BuyAutoMineSol(detail.Package, _rewardType);
                        enableAutoMine = await _serverManager.UserSolanaManager.StartAutoMineSol();
                    } else {
                        await _serverManager.General.BuyAutoMine(detail.Package, _rewardType);
                        enableAutoMine = await _serverManager.General.StartAutoMine();
                    }
                    _storageManager.EnableAutoMine = enableAutoMine;
                    _storageManager.CanBuyAutoMine = _storageManager.AutoMineInfo.CanBuyAutoMine;
                    if (enableAutoMine) {
                        _analytics.TrackConversion(ConversionType.UseAutoMine);
                    }
                    DialogOK.ShowInfo(DialogCanvas, "Successfully");
                    Hide();
                } catch (Exception e) {
                    if (e is ErrorCodeException) {
                        DialogError.ShowError(DialogCanvas, e.Message);    
                    } else {
                        DialogOK.ShowError(DialogCanvas, e.Message);
                    }
                }
                waiting.End();
            });
        }

        private bool CanBuy(IAutoMinePackageDetail package) {
            if (AppConfig.IsAirDrop()) {
                var value = _chestRewardManager.GetChestReward(_rewardType, RewardUtils.GetDataTypeStarCore());
                return value >= package.Price;
            }
            
            var depositedCoin = _chestRewardManager.GetChestReward(BlockRewardType.BCoinDeposited);
            var bombCoin = _chestRewardManager.GetChestReward(BlockRewardType.BCoin);
            return bombCoin + depositedCoin >= package.Price;
        }

        private void ShowNotEnoughMoney() {
            var title = _languageManager.GetValue(LocalizeKey.ui_not_enough);
            var message = _languageManager.GetValue(LocalizeKey.info_not_enough);
            var acc = _userAccountManager.GetRememberedAccount();
            var tokenData =
                _launchPadManager.GetData(new RewardType(_rewardType), NetworkSymbol.Convert(acc.network));
            var str = string.Format(message, tokenData.displayName);
            DialogOK.ShowInfo(DialogCanvas, title, str);
        }

        private async void ConfirmBuy(int day, Action yesAction) {
            var info = _languageManager.GetValue(LocalizeKey.ui_info_buy_automine);
            var str = string.Format(info, day);
            var dialog = await DialogConfirm.Create();
                dialog
                .SetInfo(str, "Yes", "No", yesAction, null)
                .Show(DialogCanvas);
        }
        
        public async void OnInformationAutoMineClicked() {
            _soundManager.PlaySound(Audio.Tap);
            var dialog = await DialogInformation.Create();
            BasicInformationTabType tab;
            //DevHoang: Add new airdrop
            if (AppConfig.IsTon()) {
                tab = BasicInformationTabType.AutoMineTon;
            } else if (AppConfig.IsSolana()) {
                tab = BasicInformationTabType.AutoMineSol;
            } else if (AppConfig.IsRonin()) {
                tab = BasicInformationTabType.AutoMineRon;
            } else if (AppConfig.IsBase()) {
                tab = BasicInformationTabType.AutoMineBas;
            } else if (AppConfig.IsViction()) {
                tab = BasicInformationTabType.AutoMineVic;
            } else {
                tab = BasicInformationTabType.AutoMine;
            }
            dialog.OpenTab(tab);
            dialog.Show(DialogCanvas);
        }

        public void OnButtonClose() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }
    }
}