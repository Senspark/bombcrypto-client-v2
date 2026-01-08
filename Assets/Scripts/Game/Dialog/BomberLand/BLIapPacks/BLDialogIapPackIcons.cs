using System;
using System.Threading.Tasks;

using App;

using Cysharp.Threading.Tasks;

using Senspark;

using GroupMainMenu;

using Services;

using UnityEngine;

namespace Game.Dialog {
    public class BLDialogIapPackIcons : MonoBehaviour {
        [SerializeField]
        private Canvas canvasDialog;
        
        [SerializeField]
        private MMButton btStarterPack;
        
        [SerializeField]
        private MMButton btHeroPack;
        
        [SerializeField]
        private MMButton btPremiumPack;
        
        private ISoundManager _soundManager;
        private IIAPItemManager _iapItemManager;
        private IOfferPacksResult.IOffer _currentOffer;
        private BLDialogIapPack _currentDialog;
        private IState _state;

        private void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _iapItemManager = ServiceLocator.Instance.Resolve<IIAPItemManager>();

            // Init offers
            HideOffersButtons();

            btStarterPack.Button.onClick.AddListener(OpenDialogBuyStarterPack);
            btHeroPack.Button.onClick.AddListener(OpenDialogBuyHeroPack);
            btPremiumPack.Button.onClick.AddListener(OpenDialogBuyPremiumPack);
        }

        private void OnDestroy() {
            CancelInvoke(nameof(UpdateState));
        }

        public void SetCanvasDialog(Canvas canvas) {
            canvasDialog = canvas;
        }

        public void SetInteractable(bool value) {
            btStarterPack.Interactable = value;
            btHeroPack.Interactable = value;
            btPremiumPack.Interactable = value;
        }

        public void CheckAndShowOffers() {
#if UNITY_WEBGL || UNITY_IOS
            return;
#endif
            _state = new AutoShowDialogState(_iapItemManager, canvasDialog);
            InvokeRepeating(nameof(UpdateState), 0, 1);
        }

        private void UpdateState() {
            _state = _state.NextState();
            TryShowOffersButtons();
        }

        private void HideOffersButtons() {
            btStarterPack.gameObject.SetActive(false);
            btHeroPack.gameObject.SetActive(false);
            btPremiumPack.gameObject.SetActive(false);
        }

        private void TryShowOffersButtons() {
            btStarterPack.gameObject.SetActive(_iapItemManager.CanBuyOffer(IOfferPacksResult.OfferType.Starter));
            btHeroPack.gameObject.SetActive(_iapItemManager.CanBuyOffer(IOfferPacksResult.OfferType.Hero));
            btPremiumPack.gameObject.SetActive(_iapItemManager.CanBuyOffer(IOfferPacksResult.OfferType.Premium));
        }

        private void OpenDialogBuyStarterPack() {
            _soundManager.PlaySound(Audio.Tap);
            const IOfferPacksResult.OfferType t = IOfferPacksResult.OfferType.Starter;
            OpenDialogBuyOfferPack(t);
        }
        
        private void OpenDialogBuyHeroPack() {
            _soundManager.PlaySound(Audio.Tap);
            const IOfferPacksResult.OfferType t = IOfferPacksResult.OfferType.Hero;
            OpenDialogBuyOfferPack(t);
        }
        
        private void OpenDialogBuyPremiumPack() {
            _soundManager.PlaySound(Audio.Tap);
            const IOfferPacksResult.OfferType t = IOfferPacksResult.OfferType.Premium;
            OpenDialogBuyOfferPack(t);
        }

        private void OpenDialogBuyOfferPack(IOfferPacksResult.OfferType type) {
            if (!_iapItemManager.CanBuyOffer(type)) {
                // ignore
                return;
            }
            _state = new ShowDialogState(_iapItemManager, canvasDialog, type);
        }
        
        private interface IState {
            IState NextState();
        }

        private class ShowDialogState : IState {
            private readonly IIAPItemManager _iapItemManager;
            private readonly IOfferPacksResult.IOffer _currentOffer;
            private readonly Canvas _canvas;
            private BLDialogIapPack _currentDialog;
            private bool _done;

            public ShowDialogState(
                IIAPItemManager iapItemManager,
                Canvas canvas,
                IOfferPacksResult.OfferType offerType
            ) {
                _iapItemManager = iapItemManager;
                _canvas = canvas;
                _currentOffer = iapItemManager.GetOfferData(offerType);
                BLDialogIapPack.Create().ContinueWith(dialog => {
                    _currentDialog = dialog;
                    _currentDialog.OnDidHide(() => _done = true);
                    _currentDialog.Show(offerType, canvas);
                });
            }

            public IState NextState() {
                if (_done) {
                    return new GetNewOfferState(_iapItemManager, _canvas);
                }
                if (_currentOffer.IsExpired) {
                    _currentDialog.Hide();
                    _done = true;
                }
                return this;
            }
        }

        private class GetNewOfferState : IState {
            private readonly IIAPItemManager _iapItemManager;
            private readonly Canvas _canvas;
            private TaskCompletionSource<bool> _task;

            public GetNewOfferState(IIAPItemManager iapItemManager, Canvas canvas) {
                _iapItemManager = iapItemManager;
                _canvas = canvas;
            }

            public IState NextState() {
                if (_task != null) {
                    if (_task.Task.IsCompleted) {
                        if (_iapItemManager.GetAnyOffer() == null || !_canvas) {
                            return new DoNothingState();
                        }
                        return new AutoShowDialogState(_iapItemManager, _canvas);
                    }
                    return this;
                }
                if (_iapItemManager.GetAnyPurchasableOffer() != null) {
                    return new AutoShowDialogState(_iapItemManager, _canvas);
                }
                if (_task == null) {
                    _task = new TaskCompletionSource<bool>();
                    UniTask.Void(async () => {
                        await _iapItemManager.SyncOfferShops();
                        _task.SetResult(true);
                    });
                }
                return this;
            }
        }

        private class CountDownTimeState : IState {
            private readonly IIAPItemManager _iapItemManager;
            private readonly IOfferPacksResult.IOffer _currentOffer;
            private readonly Canvas _canvas;

            public CountDownTimeState(IIAPItemManager iapItemManager, Canvas canvas, IOfferPacksResult.OfferType type) {
                _iapItemManager = iapItemManager;
                _canvas = canvas;
                _currentOffer = iapItemManager.GetOfferData(type);
            }
            
            public IState NextState() {
                if (_currentOffer == null || _currentOffer.IsExpired) {
                    return new GetNewOfferState(_iapItemManager, _canvas);
                }
                return this;
            }
        }

        private class DoNothingState : IState {
            public IState NextState() {
                return this;
            }
        }

        private class AutoShowDialogState : IState {
            private readonly IIAPItemManager _iapItemManager;
            private readonly Canvas _canvas;

            public AutoShowDialogState(IIAPItemManager iapItemManager, Canvas canvas) {
                _iapItemManager = iapItemManager;
                _canvas = canvas;
            }

            public IState NextState() {
                var arr = (IOfferPacksResult.OfferType[])Enum.GetValues(typeof(IOfferPacksResult.OfferType));
                var canBuy = false;
                var validOffer = IOfferPacksResult.OfferType.Starter;
                foreach (var t in arr) {
                    if (_iapItemManager.CanBuyOffer(t)) {
                        canBuy = true;
                        validOffer = t;
                        break;
                    }
                }
                if (!canBuy) {
                    return new GetNewOfferState(_iapItemManager, _canvas);
                }
                foreach (var t in arr) {
                    if (_iapItemManager.CanAutoShowOffer(t)) {
                        return new ShowDialogState(_iapItemManager, _canvas, t);
                    }
                }
                return new CountDownTimeState(_iapItemManager, _canvas, validOffer);
            }
        }
    }
}