using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Analytics;

using App;

using Cysharp.Threading.Tasks;

using Data;

using Senspark;

using Game.Dialog.BomberLand.BLFrameShop;
using Game.Dialog.BomberLand.BLGacha;
using Game.Manager;

using GroupMainMenu;

using Services;
using Services.IapAds;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using Utils;

namespace Game.Dialog {
    public class BLDialogIapPack : Dialog {
        [SerializeField]
        private BLGachaRes resources;
        
        [SerializeField]
        private BlDialogIapPackItem itemPrefab;

        [SerializeField]
        private GameObject costumePanel;
        
        [SerializeField]
        private BLShopCostumeInfo costumeInfo;

        [SerializeField]
        private Image titleIconImg;

        [SerializeField]
        private List<Sprite> titleIcons;

        [SerializeField]
        private Sprite noAdsIcon;

        [SerializeField]
        private Transform content;

        [SerializeField]
        private TextMeshProUGUI timeTxt;
        
        [SerializeField]
        private TextMeshProUGUI beforeDiscountTxt;
        
        [SerializeField]
        private TextMeshProUGUI afterDiscountTxt;

        [SerializeField]
        private Button buyBtn;
        
        private IProductItemManager _productItemManager;
        private IServerManager _serverManager;
        private IIAPItemManager _iapItemManager;
        private IUnityPurchaseManager _purchaseManager;
        private IAnalytics _analytics;
        private ILogManager _logManager;
        private ISoundManager _soundManager;
        
        private IOfferPacksResult.IOffer _offerData;
        private bool _isProcessing;
        private bool _expired;

        public static UniTask<BLDialogIapPack> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<BLDialogIapPack>();
        }

        protected override void Awake() {
            base.Awake();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
            _iapItemManager = ServiceLocator.Instance.Resolve<IIAPItemManager>();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _purchaseManager = ServiceLocator.Instance.Resolve<IUnityPurchaseManager>();
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            CancelInvoke(nameof(UpdateTime));
        }

        public void Show(IOfferPacksResult.OfferType offerType, Canvas canvas) {
            base.Show(canvas);
            Init(offerType);
        }

        public void Close() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }

        private void Init(IOfferPacksResult.OfferType offerType) {
            if (_isProcessing) {
                return;
            }
            _isProcessing = true;
            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            buyBtn.gameObject.SetActive(false);
            UniTask.Void(async () => {
                try {
                    if (await TryRestore()) {
                        return;
                    }
                    _offerData = _iapItemManager.GetOfferData(offerType);
                    if (_offerData == null || _offerData.IsExpired) {
                        return;
                    }

                    var price = _purchaseManager.GetItemPrice(_offerData.Name);
                    beforeDiscountTxt.text = $"{App.Utils.FormatBcoinValue(price.Price / 0.1f)} {price.Currency}";
                    afterDiscountTxt.text = $"{App.Utils.FormatBcoinValue(price.Price)} {price.Currency}";
                    titleIconImg.sprite = titleIcons[(int)offerType];
                    foreach (var e in _offerData.Items) {
                        await InstantiateItem(e);
                    }
                    if (_offerData.WillRemoveAds) {
                        InstantiateNoAdsItem();
                    }
                    buyBtn.gameObject.SetActive(true);
                    InvokeRepeating(nameof(UpdateTime), 0, 1);
                    TrackShowOffer();
                } catch (Exception e) {
                    _logManager.Log(e.Message);
                    Hide();
                } finally {
                    _isProcessing = false;
                    waiting.End();
                }
            });
        }

        private void UpdateTime() {
            if (!_offerData.IsExpired) {
                
                var endDate = _offerData.SaleEnd - DateTime.Now;
                if (endDate.Days >= 1) {
                    const string timeFormat =
                        "<color=#43dd00>ONLY UNTIL:</color> <color=#f9c002>{0}D {1}H {2}M</color>";
                    timeTxt.text = string.Format(timeFormat, endDate.Days, endDate.Hours, endDate.Minutes);
                } else {
                    const string timeFormat =
                        "<color=#43dd00>ONLY UNTIL:</color> <color=#f9c002>{0}H {1}M {2}S</color>";
                    timeTxt.text = string.Format(timeFormat, endDate.Hours, endDate.Minutes, endDate.Seconds);
                }
            } else {
                timeTxt.text = "<color=#f9c002>LIMITED EVENT</color>";
                CancelInvoke(nameof(UpdateTime));
            } 
        }

        private async Task InstantiateItem(IOfferPacksResult.IItem data) {
            var obj = Instantiate(itemPrefab, content);
            var product = await _productItemManager.GetItemAsync(data.ItemId);
            var spr = await resources.GetSpriteByItemId(data.ItemId);
            var expireTime = !data.IsNeverExpired ? $"{data.ExpiresAfter.TotalDays}D" : null;
            var productName = $"{product.Name.ToUpperInvariant()} {expireTime}";
            var desc = data.ItemQuantity <= 1 ? productName : $"+{data.ItemQuantity} {productName}";
            var isPremium = product.ItemKind == ProductItemKind.Premium;
            obj.SetData(spr, desc, isPremium, () => OnItemClicked(product));
            obj.gameObject.SetActive(true);
        }

        private void InstantiateNoAdsItem() {
            var obj = Instantiate(itemPrefab, content);
            obj.SetData(noAdsIcon, "REMOVE ADS", false, null);
            obj.gameObject.SetActive(true);
        }

        private void OnItemClicked(ProductItemData data) {
            _soundManager.PlaySound(Audio.Tap);
            var costume = new CostumeData {
                ItemId = data.ItemId,
                Prices = Array.Empty<CostumeData.PriceData>()
            };
            costumePanel.SetActive(true);
            costumeInfo.SetData(resources, data, costume);
        }

        private void OnBuyBtnClicked() {
            if (_isProcessing && _offerData != null) {
                return;
            }
            _soundManager.PlaySound(Audio.Tap);
            UniTask.Void(async () => {
                var waiting = new WaitingUiManager(DialogCanvas);
                waiting.Begin();
                try {
                    if (await TryRestore()) {
                        return;
                    }
                    var result = await _iapItemManager.BuyIap(_offerData.Name, () => IapHelper.OnBeforeConsume(DialogCanvas));
                    TrackHelper.TrackBuyIap(_analytics, null, result, 0);
                    switch (result.State) {
                        case PurchaseState.Done: {
                            await _serverManager.General.GetChestReward();
                            await _iapItemManager.SyncOfferShops();
                            FindObjectOfType<MMHeroSelector>(true)?.UpdateListPvpHeros();
                            Hide();
                            DialogOK.ShowInfo(DialogCanvas, "Purchase Successful");
                            break;
                        }
                        case PurchaseState.Cancel:
                            // ignore
                            break;
                        case PurchaseState.Error:
                            throw new Exception("Purchase Failed");
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                } catch (Exception e) {
                    var msg = $"{e.Message}. Please try again later.";
                    DialogOK.ShowError(DialogCanvas, msg);
                } finally {
                    waiting.End();
                }
            });
            TrackTapBtnBuy();
        }

        private void TrackShowOffer() {
            if (_offerData == null) {
                return;
            }
            var n = _offerData.Type switch {
                IOfferPacksResult.OfferType.Starter => ConversionType.ShowStarterPack,
                IOfferPacksResult.OfferType.Premium => ConversionType.ShowPremiumPack,
                IOfferPacksResult.OfferType.Hero => ConversionType.ShowHeroPack,
                _ => throw new ArgumentOutOfRangeException()
            };
            _analytics.TrackConversion(n);
        }

        private void TrackTapBtnBuy() {
            if (_offerData == null) {
                return;
            }
            var n = _offerData.Type switch {
                IOfferPacksResult.OfferType.Starter => ConversionType.TapBtnBuyStarterPack,
                IOfferPacksResult.OfferType.Premium => ConversionType.TapBtnBuyPremiumPack,
                IOfferPacksResult.OfferType.Hero => ConversionType.TapBtnBuyHeroPack,
                _ => throw new ArgumentOutOfRangeException()
            };
            _analytics.TrackConversion(n);
        }

        private async Task<bool> TryRestore() {
            var restored = await IapHelper.TryRestore(_iapItemManager, _serverManager, _analytics, DialogCanvas);
            await _iapItemManager.SyncOfferShops();
            if (restored) {
                Hide();
                return true;
            }
            return false;
        }
    }
}