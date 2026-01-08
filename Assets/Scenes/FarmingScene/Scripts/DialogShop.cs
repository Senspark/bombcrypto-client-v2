using System;
using System.Collections.Generic;
using App;
using Cysharp.Threading.Tasks;
using Game.Dialog;
using Senspark;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.FarmingScene.Scripts {
    public class DialogShop : Dialog {
        [SerializeField]
        private Text houseTotalSaleLbl;
        
        [SerializeField]
        private List<Text> heroTotalSaleLbl;

        [SerializeField]
        private GameObject shopHero;
        
        [SerializeField]
        private GameObject shopHeroS;

        [SerializeField]
        private GameObject shopBomb;

        [SerializeField]
        private Text tonValue;

        [SerializeField]
        private Text starCoreValue;
        
        private ISoundManager _soundManager;
        private IStorageManager _storeManager;
        private IServerManager _serverManager;
        private IChestRewardManager _chestRewardManager;
        private ObserverHandle _handle;

        public static UniTask<DialogShop> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogShop>();
        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            var featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
            var acc = ServiceLocator.Instance.Resolve<IUserAccountManager>().GetRememberedAccount();
            shopHero.SetActive(featureManager.EnableBHero);
            shopHeroS.SetActive(featureManager.EnableBHeroS);
            shopBomb.SetActive(featureManager.EnableBuyBombToken && acc.loginType == LoginType.Wallet && acc.network == NetworkType.Polygon);

            var totalHouseMinted = 0;
            var totalHouseMintLimits = 0;
            Array.ForEach(_storeManager.HouseMinAvailable, e => totalHouseMinted += e);
            Array.ForEach(_storeManager.HouseMintLimits, e => totalHouseMintLimits += e);
            houseTotalSaleLbl.text = $"{totalHouseMinted:N0}/{totalHouseMintLimits:N0}";
            heroTotalSaleLbl.ForEach(e => e.text = $"{_storeManager.HeroTotalSale:N0}");

            if (!AppConfig.IsTon()) {
                return;
            }
            _handle = new ObserverHandle();
            _handle.AddObserver(_serverManager, new ServerObserver {
                OnChestReward = OnChestReward
            });
            OnChestReward(null);
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            _handle?.Dispose();
        }

        private void OnChestReward(IChestReward _) {
            var ton = _chestRewardManager.GetChestReward(BlockRewardType.BCoinDeposited);
            var starCore = _chestRewardManager.GetChestReward(BlockRewardType.BLCoin);
            tonValue.text = ton > 0 ? $"{ton:#,0.####}" : "0";
            starCoreValue.text = starCore > 0 ? $"{starCore:#,0.####}" : "0";
        }

        public async void OnBuyHouseClicked() {
            _soundManager.PlaySound(Audio.Tap);
            if (AppConfig.IsTon() || AppConfig.IsSolana()) {
                var dialogAirdrop = await DialogShopHouseAirdrop.Create();
                dialogAirdrop.Show(DialogCanvas);
            } else if (AppConfig.IsWebAirdrop()) {
                var dialog = await DialogShopHouseWebAirdrop.Create();
                dialog.Show(DialogCanvas);
            } else {
                var dialog = await DialogShopHouseCreator.Create();
                dialog.Show(DialogCanvas);
            }
        }

        public void OnBuyHeroClicked() {
            _soundManager.PlaySound(Audio.Tap);
            DialogShopHero.Create().ContinueWith(dialog => {
                dialog.Init(false);
                dialog.Show(DialogCanvas);
            });
        }

        public async void OnBuyHeroSClicked() {
            _soundManager.PlaySound(Audio.Tap);
            if (AppConfig.IsTon()) {
                var dialog = await DialogShopHeroTon.Create();
                dialog.Init();
                dialog.Show(DialogCanvas);
            } else if (AppConfig.IsSolana()) {
                var dialog = await DialogShopHeroSolana.Create();
                dialog.Init();
                dialog.Show(DialogCanvas);
            } else if (AppConfig.IsWebAirdrop()) {
                var dialog = await DialogShopHeroWebAirdrop.Create();
                dialog.Init();
                dialog.Show(DialogCanvas);
            } else {
                var dialog = await DialogShopHero.Create();
                dialog.Init(true);
                dialog.Show(DialogCanvas);
            }
        }

        public void OnBuyCoinClicked() {
            _soundManager.PlaySound(Audio.Tap);
            DialogShopCoin.Create().ContinueWith(dialog => {
                dialog.Show(DialogCanvas);                
            });
        }

        public void OnCloseBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }

        protected override void OnYesClick() {
            // Do nothing
        }
    }
}