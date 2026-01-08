using System;
using Analytics;
using App;
using Cysharp.Threading.Tasks;
using Game.Dialog;
using Game.Dialog.BomberLand.BLGacha;
using Game.Manager;
using Senspark;
using Services.Server;
using Services.Server.Exceptions;
using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.MarketplaceScene.Scripts {
    public class DialogOrderItemMarket : Dialog
    {
        [SerializeField] private Button btnBuy;
        [SerializeField] private TMP_Text priceText, quantityText, expirationText;
        
        [SerializeField] private Image avatar;
        [SerializeField] private BLGachaRes gachaRes;
        private ItemData _itemData;
        private bool _canBuy;
        private IAnalytics _analytics;
        private IMarketplace _marketplace;
        private ISoundManager _soundManager;
        public Action<bool> OnHideDialogBuy;

        public static UniTask<DialogOrderItemMarket> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogOrderItemMarket>();
        }

        protected override void Awake() {
            base.Awake();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _marketplace = ServiceLocator.Instance.Resolve<IServerManager>().Marketplace;
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        }

        public void SetInfo(OrderDataResponse data) {
            _canBuy = data.IsSuccess;
            btnBuy.interactable = _canBuy;
            priceText.text = data.TotalPrice.ToString();
            quantityText.text = $"x {data.TotalQuantity}";

            if (data.Type is ExpirationMarketType.Expiration7Days) {
                expirationText.gameObject.SetActive(true);
                expirationText.text = "DURATION <color=green>7 DAYS</color> AFTER EQUIPPED";
            
            }
            else if (data.Type is ExpirationMarketType.Expiration30Days) {
                expirationText.gameObject.SetActive(true);
                expirationText.text = "DURATION <color=green>30 DAYS</color> AFTER EQUIPPED";
            }
            else {
                expirationText.gameObject.SetActive(false);
            }
            UniTask.Void(async () => {
                avatar.sprite = await gachaRes.GetSpriteByItemId(data.ItemId);
            });
        }

        private bool _isClicked;

        public void OnBuyButtonClicked() {
            _soundManager.PlaySound(Audio.Tap);

            // Ko đủ gem 
            if (!_canBuy) {
                return;
            }
            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    await _marketplace.BuyItemMarket();
                    OnHideDialogBuy?.Invoke(false);
                } catch (Exception e) {
                    _analytics.TrackConversion(ConversionType.BuyMarketItemFail);
                    if (e is ErrorCodeException) {
                        await DialogError.ShowError(DialogCanvas, e.Message, () => { _isClicked = false; });
                    } else {
                        DialogOK.ShowError(DialogCanvas, e.Message, () => { _isClicked = false; });
                    }
                } finally {
                    waiting.End();
                    Hide();
                }
            });

        }

        public void OnNoButtonClicked() {
            _soundManager.PlaySound(Audio.Tap);
            if (_canBuy) {
                _marketplace.CancelOrderItemMarket();
            }
            Hide();
        }
        
        protected override void OnYesClick() {
            if(!btnBuy.IsInteractable())
                return;
            
            if (_isClicked)
                return;
            _isClicked = false;
            OnBuyButtonClicked();
        }

        protected override void OnNoClick() {
            OnNoButtonClicked();
        }
    }
}