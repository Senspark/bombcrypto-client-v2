using System;
using System.Collections.Generic;
using System.Threading;

using App;

using Cysharp.Threading.Tasks;

using DG.Tweening;

using Game.Manager;

using Senspark;

using Game.UI;

using Services.Server.Exceptions;

using Share.Scripts.Dialog;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class DialogAirDrop : Dialog {
        [SerializeField]
        private AirDropEventPanel panelPrefab;

        [SerializeField]
        private AirDropEventButton activeEventBtn;

        [SerializeField]
        private AirDropEventButton closedEventBtn;

        [SerializeField]
        private ScrollRect scrollView;

        [SerializeField]
        private AirDropEventPanel currentEventPanel;

        [SerializeField]
        private Text currentEventAbout;

        [SerializeField]
        private Text currentEventHomePage;

        [SerializeField]
        private GameObject groupListEvents;
        
        [SerializeField]
        private GameObject groupAboutEvent;

        [SerializeField]
        private Text senTxt;
        
        [SerializeField]
        private Text bcoinTxt;

        [SerializeField]
        private WaitingPanel waitingPanel;

        private ISoundManager _soundManager;
        private IServerManager _serverManager;
        private IBlockchainManager _blockchainManager;
        private IAirDropManager _airDropManager;
        private ILanguageManager _languageManager;
        private IStorageManager _storageManager;
        private IChestRewardManager _chestRewardManager;
        
        private CancellationTokenSource _cancellation;
        private IAirDropResponse _airDropResponse;
        private ObserverHandle _handle;

        public static DialogAirDrop Create() {
            var prefab = Resources.Load<DialogAirDrop>("Prefabs/Dialog/DialogAirDrop");
            return Instantiate(prefab);
        }

        protected override void Awake() {
            base.Awake();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _airDropManager = ServiceLocator.Instance.Resolve<IAirDropManager>();
            _languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
            _blockchainManager = ServiceLocator.Instance.Resolve<IBlockchainManager>();
            _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            
            _handle = new ObserverHandle();
            _handle.AddObserver(_serverManager, new ServerObserver {
                OnChestReward = UpdateChestReward
            });
            
            waitingPanel.gameObject.SetActive(true);
            ClearList();
            ViewListEvents(false);
            UpdateChestReward(null);
            
            _cancellation = new CancellationTokenSource();
            UniTask.Void(async (token) => {
                try {
                    _airDropResponse = await _serverManager.General.GetAirDrop();
                    await _airDropManager.SyncRemoteData();
                    OpenActiveEvents();
                    waitingPanel.gameObject.SetActive(false);
                } catch (Exception e) when (!_cancellation.IsCancellationRequested) {
                    if (e is ErrorCodeException) {
                        DialogError.ShowError(DialogCanvas, e.Message);    
                    } else {
                        DialogOK.ShowError(DialogCanvas, e.Message);
                    }
                }
            }, _cancellation.Token);
        }

        protected override void OnDestroy() {            
            base.OnDestroy();
            _cancellation.Cancel();
            _cancellation.Dispose();
            _handle.Dispose();
        }
        
        #region Buttons

        public void OnActiveEventsBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            OpenActiveEvents();
        }

        public void OnClosedEventsBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            OpenClosedEvents();
        }

        public void OnBackToViewListEventsBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            ViewListEvents(true);
        }

        public void OpenHomePage() {
            WebGLUtils.OpenUrl(currentEventHomePage.text);
        }

        #endregion

        private void OpenActiveEvents() {
            if (_airDropResponse == null) {
                return;
            }
            activeEventBtn.SetActive(true);
            closedEventBtn.SetActive(false);
            SpawnPanels(_airDropResponse.ActiveEvents);
        }

        private void OpenClosedEvents() {
            if (_airDropResponse == null) {
                return;
            }
            activeEventBtn.SetActive(false);
            closedEventBtn.SetActive(true);
            SpawnPanels(_airDropResponse.ClosedEvents);
        }
        
        private void ViewListEvents(bool useEffect) {
            void Cb() {
                groupListEvents.SetActive(true);
                groupAboutEvent.SetActive(false);
            }
            if (useEffect) {
                groupAboutEvent.transform.DOLocalMoveX(Screen.width, 0.3f).SetEase(Ease.OutSine).OnComplete(Cb);
            } else {
                Cb();
            }
        }

        private void ViewEvent(IAirDropEvent ev) {
            var data = _airDropManager.GetData(ev.CodeName);
            if (data == null) {
                return;
            }

            currentEventPanel.Init(ev, ClaimReward, ViewEvent, _soundManager, _languageManager, _chestRewardManager,
                data, DialogCanvas);
            currentEventAbout.text = data.content;
            currentEventHomePage.text = data.homePage;
            
            groupListEvents.SetActive(false);
            groupAboutEvent.SetActive(true);

            var startPos = new Vector3(Screen.width, 0, 0);
            groupAboutEvent.transform.DOLocalMoveX(0, 0.3f).SetEase(Ease.OutSine).ChangeStartValue(startPos);
        }

        private void ClaimReward(IAirDropEvent ev) {
            _soundManager.PlaySound(Audio.Tap);
            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    var resp = await _serverManager.General.ClaimAirDrop(ev.CodeName);
                    if (_airDropManager.GetData(ev.CodeName).canClaimNft) {
                        _airDropResponse = resp.Events;
                        var success = await _blockchainManager.GetNFT(resp.Amount, resp.EventId, resp.Nonce, resp.Signature);
                        if (success) {
                            await _serverManager.General.ConfirmClaimAirDrop(ev.CodeName);
                            DialogOK.ShowInfo(DialogCanvas, "Successfully");
                        } else {
                            DialogOK.ShowError(DialogCanvas, "Claim failed, please try again later");    
                        }
                    } else {
                        DialogOK.ShowInfo(DialogCanvas, "Successfully");
                    }
                  
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

        private void SpawnPanels(List<IAirDropEvent> events) {
            ClearList();
            foreach (var ev in events) {
                var data = _airDropManager.GetData(ev.CodeName); 
                if (data == null) {
                    continue;
                }
                var panel = Instantiate(panelPrefab, scrollView.content);
                panel.Init(ev, ClaimReward, ViewEvent, _soundManager, _languageManager, _chestRewardManager, data,
                    DialogCanvas);
            }
        }

        private void ClearList() {
            foreach (Transform t in scrollView.content.transform) {
                Destroy(t.gameObject);
            }
        }

        private void UpdateChestReward(IChestReward _) {
            bcoinTxt.text = App.Utils.FormatBcoinValue(_chestRewardManager.GetChestReward(BlockRewardType.BCoinDeposited));
            senTxt.text = App.Utils.FormatBcoinValue(_chestRewardManager.GetChestReward(BlockRewardType.Senspark));
        }
    }
}