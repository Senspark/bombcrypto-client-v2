using System.Collections.Generic;

using Analytics;

using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;

using Senspark;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.FarmingScene.Scripts {

    public interface IDialogShopHouse {
        void Show(Canvas canvas);
    }

    public static class DialogShopHouseCreator {
        public static async UniTask<IDialogShopHouse> Create() {
            if (ScreenUtils.IsIPadScreen()) {
                return await DialogShopHousePad.Create();
            }
            return await DialogShopHouse.Create();
        }
    }
    
    
    public class DialogShopHouse : Dialog, IDialogShopHouse {
        [SerializeField]
        private HouseItem itemPrefab;

        [SerializeField]
        private ScrollRect scrollRect;

        [SerializeField]
        private Text houseName;

        [SerializeField]
        private Text supply;

        [SerializeField]
        private Text housePrice;

        [SerializeField]
        private Image soldOutImg;

        [SerializeField]
        private Button buttonBuy;

        private IBlockchainManager _blockchainManager;
        private IServerManager _serverManager;
        private IStorageManager _storeManager;
        private IBlockchainStorageManager _blockchainStorageManager;
        private IHouseStorageManager _houseStorageManager;
        private ILanguageManager _languageManager;
        private ISoundManager _soundManager;
        private IAnalytics _analytics;

        private List<HouseItem> _houseData;
        private int _indexChoose;
        private RpcTokenCategory _currency;

        public static async UniTask<IDialogShopHouse> Create() {
            return await ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogShopHouse>();
        }
        
        // public static DialogShopHouse Create() {
        //     if (ScreenUtils.IsIPadScreen()) {
        //         return ServiceLocator.Instance.Resolve<IDialogManager>().GetDialog<DialogShopHouse>("DialogShopHouse-IPad");
        //     } else {
        //         return ServiceLocator.Instance.Resolve<IDialogManager>().GetDialog<DialogShopHouse>();
        //     }
        // }

        protected override void Awake() {
            base.Awake();
            _blockchainManager = ServiceLocator.Instance.Resolve<IBlockchainManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _houseStorageManager = ServiceLocator.Instance.Resolve<IHouseStorageManager>();
            _blockchainStorageManager = ServiceLocator.Instance.Resolve<IBlockchainStorageManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            
            var networkConfig = ServiceLocator.Instance.Resolve<INetworkConfig>();
            _currency = networkConfig.NetworkType == NetworkType.Binance
                ? RpcTokenCategory.Bcoin
                : RpcTokenCategory.Bomb;
        }

        private void Start() {
            Init();
        }

        private void Init() {
            App.Utils.ClearAllChildren(scrollRect.content);
            var count = _storeManager.HousePrice.Length;
            _houseData = new List<HouseItem>();
            for (var i = 0; i < count; i++) {
                var item = Instantiate(itemPrefab, scrollRect.content);
                var data = DefaultHouseStoreManager.GetHouseInfo(i);
                item.SetInfo(i, data, OnItemClicked, _soundManager);
                _houseData.Add(item);
            }
            OnItemClicked(0);
        }

        private void OnItemClicked(int index) {
            _indexChoose = index;
            foreach (var t in _houseData) {
                t.SetActive(false);
            }

            _houseData[index].SetActive(true);

            var data = _houseData[index].Data;
            houseName.text = DefaultHouseStoreManager.GetHouseName(data.HouseType);
            supply.text = $"{data.Supply}/{data.MintLimits}";

            var value = data.Price;
            housePrice.text = $"{value:0.##}";

            var isSoldOut = data.Supply == 0;
            buttonBuy.interactable = !isSoldOut;
            soldOutImg.gameObject.SetActive(isSoldOut);
        }

        public async void OnBuyButtonClicked() {
            _soundManager.PlaySound(Audio.Tap);
            if (!CheckEnoughBCoin()) {
                TrackBuyHouseFail();
                return;
            }

            if (!CheckLimit()) {
                TrackBuyHouseFail();
                return;
            }

            if (DialogWarningBeforeBuyHouse.CanShow()) {
                var dialogWarning = await DialogWarningBeforeBuyHouse.Create();
                dialogWarning.SetAcceptCallback(accepted => {
                    if (accepted) {
                        Process();
                    } else {
                        TrackBuyHouseFail();
                    }
                });
                dialogWarning.Show(DialogCanvas);
            } else {
                Process();
            }
        }

        private void Process() {
            UniTask.Void(async () => {
                var waiting = await DialogWaiting.Create();
                waiting.Show(DialogCanvas);
                waiting.ShowLoadingAnim();
                
                var coins = _blockchainStorageManager.GetBalance(_currency);
                var price = _storeManager.HousePrice[_indexChoose];
                
                if (await _blockchainManager.BuyHouse(_indexChoose)) {
                    _analytics.TrackConversion(ConversionType.BuyHouse);
                    var (changed, coinAfter) =
                        await App.Utils.WaitForBalanceChange(_currency, _blockchainManager, _blockchainStorageManager);
                    var p = changed ? coins - coinAfter : price;
                    await _serverManager.General.SyncHouse();
                    Hide();
                } else {
                    TrackBuyHouseFail();
                }

                waiting.Hide();
            });
            
        }

        private bool CheckLimit() {
            var houseLimit = _storeManager.HouseLimit;
            var houseCount = _houseStorageManager.GetHouseCount();

            if (houseCount < houseLimit) {
                return true;
            }
            
            var tit = _languageManager.GetValue(LocalizeKey.ui_house_limit);
            var desc = string.Format(_languageManager.GetValue(LocalizeKey.info_cant_buy_houses), houseLimit);
            DialogOK.ShowInfo(DialogCanvas, tit, desc);
            return false;
        }

        private bool CheckEnoughBCoin() {
            var coins = _blockchainStorageManager.GetBalance(_currency);
            var price = _storeManager.HousePrice[_indexChoose];
            if (coins < price) {
                DialogOK.ShowInfo(DialogCanvas, LocalizeKey.ui_not_enough, LocalizeKey.info_not_enough_bcoin);
                return false;
            }
            return true;
        }

        public void OnCloseBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }

        private void TrackBuyHouseFail() {
            _analytics.TrackConversion(ConversionType.BuyHouseFail);
        }

        protected override void OnYesClick() {
            // Do nothing
        }
    }
}