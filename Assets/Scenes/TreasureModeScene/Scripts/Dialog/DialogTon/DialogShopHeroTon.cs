using System;

using Analytics;

using App;

using BomberLand.Button;

using Constant;

using Cysharp.Threading.Tasks;

using Game.Manager;

using Senspark;

using Services.Server.Exceptions;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class DialogShopHeroTon : Dialog {
        [SerializeField]
        private Text amountTitle;
        
        [SerializeField]
        private Text heroBcoinPrice;

        [SerializeField]
        private Text heroTonPrice;

        [SerializeField]
        private Text heroStarCorePrice;

        [SerializeField]
        private Text buttonX;

        [SerializeField]
        private XButton[] buttonXs;

        [SerializeField]
        private Text heroTotalSaleLbl;

        private int _buyHeroIndex = 0;

        private readonly int[] _buyHeroAmount = {1, 5, 10, 15};
        // private float _ton;
        // private float _starCore;

        private ISoundManager _soundManager;
        private IStorageManager _storeManager;
        private ILanguageManager _languageManager;
        private IServerManager _serverManager;
        private IAnalytics _analytics;
        private IChestRewardManager _chestRewardManager;

        // private Action<int> _buyCallback;
        // private CancellationTokenSource _cancellation;
        // private ObserverHandle _handle;

        public static UniTask<DialogShopHeroTon> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogShopHeroTon>();
        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();

            // _handle = new ObserverHandle();
            // _handle.AddObserver(_serverManager, new ServerObserver {
            //     OnChestReward = OnChestReward
            // });
            var totalSale = _storeManager.HeroTotalSale;
            heroTotalSaleLbl.text = $"{totalSale:N0}";
        }

        // private void Start() {
        //     _cancellation = new CancellationTokenSource();
        //     UniTask.Void(async (token) => {
        //         await _serverManager.General.GetChestReward();
        //     }, _cancellation.Token);
        // }

        // protected override void OnDestroy() {
        //     base.OnDestroy();
        //     _handle.Dispose();
        //     _cancellation.Cancel();
        //     _cancellation.Dispose();
        // }

        public void Init() {
            buttonXs[_buyHeroIndex].SetActive(true);
            RenderPrice(_buyHeroIndex);
        }
        
        public void OnBuyWithBcoinBtnClicked() {
            var buyAmount = _buyHeroAmount[_buyHeroIndex];
            BuyHero(buyAmount, BuyHeroCategory.WithBcoin);
        }

        public void OnBuyWithTonBtnClicked() {
            var buyAmount = _buyHeroAmount[_buyHeroIndex];
            BuyHero(buyAmount, BuyHeroCategory.WithTon);
        }

        public void OnBuyWithStarCoreClicked() {
            var buyAmount = _buyHeroAmount[_buyHeroIndex];
            BuyHero(buyAmount, BuyHeroCategory.WithStarCore);
        }

        public void OnXButtonClicked(XButton button) {
            _soundManager.PlaySound(Audio.Tap);
            foreach (var iter in buttonXs) {
                if (iter == button) {
                    iter.SetActive(true);
                    _buyHeroIndex = iter.Index;
                    RenderPrice(_buyHeroIndex);
                } else {
                    iter.SetActive(false);
                }
            }
        }

        private void RenderPrice(int index) {
            var buyAmount = _buyHeroAmount[index];
            buttonX.text = $"x{buyAmount}";
            amountTitle.text = $"+{buyAmount} {_languageManager.GetValue(LocalizeKey.ui_hero)}";

            heroBcoinPrice.text = App.Utils.FormatBcoinValue(_storeManager.HeroPrice.BcoinDeposited * buyAmount);
            // heroStarCorePrice.text = App.Utils.FormatBcoinValue(_storeManager.HeroPrice.StarCore * buyAmount);
        }

        private async void BuyHero(int buyAmount, BuyHeroCategory category) {
            _soundManager.PlaySound(Audio.Tap);

            var result = await CheckEnoughResource(buyAmount, category);
            if (!result) {
                TrackBuyHeroFail();
                return;
            }
            PerformBuy(buyAmount, category);
        }

        private void PerformBuy( int buyAmount, BuyHeroCategory category) {
            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    var rewardType = category switch {
                        BuyHeroCategory.WithTon => BlockRewardType.TonDeposited,
                        BuyHeroCategory.WithStarCore => BlockRewardType.BLCoin,
                        BuyHeroCategory.WithBcoin => BlockRewardType.BCoinDeposited,
                        _ => throw new Exception($"Invalid BuyHeroCategory {category}")
                    };
                    await _serverManager.General.BuyHeroServer(buyAmount, (int) rewardType, true);
                    //Mua nhiều hơn 1 hero thì ẩn dialog mua hero luôn, sau khi tổng kết mở lại
                    if (buyAmount > 1) {
                        Hide();
                    }
                } catch (Exception e) {
                    if (e is ErrorCodeException) {
                        DialogError.ShowError(DialogCanvas, e.Message);
                    } else {
                        DialogOK.ShowError(DialogCanvas, e.Message);
                    }
                } finally {
                    waiting.End();
                }
            });
        }   
        

        private async UniTask<bool> CheckEnoughResource(int buyAmount, BuyHeroCategory category) {
            var isEnough = false;

            if (category == BuyHeroCategory.WithTon) {
                isEnough = _chestRewardManager.GetChestReward(BlockRewardType.TonDeposited) >=
                           _storeManager.HeroPrice.Ton * buyAmount;
                if (isEnough) {
                    return true;
                }
                
                var dialog = await DialogNotEnoughRewardAirdrop.Create(BlockRewardType.TonDeposited);
                dialog.Show(DialogCanvas);
            } else if (category == BuyHeroCategory.WithBcoin) {
                isEnough = _chestRewardManager.GetChestReward(BlockRewardType.BCoinDeposited) >=
                           _storeManager.HeroPrice.BcoinDeposited * buyAmount;
                if (isEnough) {
                    return true;
                }

                var dialog = await DialogNotEnoughRewardAirdrop.Create(BlockRewardType.BCoinDeposited);
                dialog.Show(DialogCanvas);
            } else {
                isEnough = _chestRewardManager.GetChestReward(BlockRewardType.BLCoin) >=
                           _storeManager.HeroPrice.StarCore * buyAmount;
                if (isEnough) {
                    return true;
                }
                DialogOK.ShowInfo(DialogCanvas, "Not Enough", $"Not Enough STAR CORE");
                return false;
            }
            return false;
        }

        private void TrackBuyHeroFail() {
            _analytics.TrackConversion(ConversionType.BuyHeroFiFail);
        }

        public void OnButtonHide() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }
    }
}