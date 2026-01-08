using System;
using System.Threading;
using App;
using Cysharp.Threading.Tasks;
using Game.Dialog;
using Game.Manager;
using PvpMode.UI;
using Scenes.MainMenuScene.Scripts;
using Senspark;
using Share.Scripts.Dialog;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Scenes.ConnectScene.Scripts.Connectors {
    public class AfUserProfile : MonoBehaviour {
        [SerializeField]
        private BLProfileCard profileCard;

        [SerializeField]
        private PvpRanking pvpRanking;
        
        [SerializeField]
        private Button btnRename;
        
        [SerializeField]
        private Button btnLogOut;
        
        [SerializeField]
        private Button btnDelete;

        private ISoundManager _soundManager;
        private IServerManager _serverManager;
        private IUserAccountManager _userAccountManager;
        private Canvas _canvasDialog;
        private CancellationTokenSource _cancellation;

        private void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _userAccountManager = ServiceLocator.Instance.Resolve<IUserAccountManager>();
            
            var isIOs = false;
#if UNITY_IOS
            isIOs = true;
#endif
            btnDelete.gameObject.SetActive(isIOs);
            
            btnRename.onClick.AddListener(OnBtnRenameClicked);
            btnLogOut.onClick.AddListener(OnBtnLogOutClicked);
            btnDelete.onClick.AddListener(OnBtnDeleteClicked);

            if (AppConfig.IsTournament()) {
                btnRename.gameObject.SetActive(false);
            }

        }

        private void OnDestroy() {
            _cancellation?.Cancel();
            _cancellation?.Dispose();
            _cancellation = null;
        }

        private void Start() {
            TryLoadData();
        }

        public void Init(Canvas canvasDialog) {
            _canvasDialog = canvasDialog;
        }

        private void TryLoadData() {
            if (_cancellation != null) {
                return;
            }
            _cancellation = new CancellationTokenSource();
            UniTask.Void(async (c) => {
                ReloadProfile();
                var rankingResult = await _serverManager.Pvp.GetPvpRanking();
                if (!c.IsCancellationRequested) {
                    pvpRanking.SetCurrentInfo(rankingResult.CurrentRank);
                }
            }, _cancellation.Token);
        }

        private void ReloadProfile() {
            profileCard.InitializeAsync(_canvasDialog).Forget();
        }

        private void OnBtnRenameClicked() {
            _soundManager.PlaySound(Audio.Tap);
            AfDialogRename.Create().ContinueWith(dialog => {
                dialog.Init(ReloadProfile);
                dialog.Show(_canvasDialog);
            });
        }

        private async void OnBtnLogOutClicked() {
            _soundManager.PlaySound(Audio.Tap);
            var confirm = await DialogConfirm.Create();
            confirm.SetInfo(
                "Are you sure you want to log out?",
                "YES",
                "NO",
                () => {
                    _soundManager.StopMusic();
                    var waiting = new WaitingUiManager(_canvasDialog);
                    waiting.Begin();
                    UniTask.Void(async () => {
                        try {
                            var isWallet = _userAccountManager.GetRememberedAccount()?.loginType is LoginType.Wallet;

                            _userAccountManager.EraseData();
                            //Fix tạm delay để tránh destroy trước khi waiting gọi show
                            await UniTask.Delay(500);  
                            App.Utils.KickToConnectScene();
                        } catch (Exception e) {
                            DialogOK.ShowError(_canvasDialog, e.Message);
                        }
                        waiting.End();
                    });
                },
                () => { }
            );
            confirm.Show(_canvasDialog);
        }

        private void OnBtnDeleteClicked() {
            _soundManager.PlaySound(Audio.Tap);
            DialogDeleteAccount.Create().ContinueWith(dialog => {
                dialog.Show(_canvasDialog);
            });
        }
    }
}