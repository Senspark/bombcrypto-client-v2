using System;
using Analytics;
using App;
using Cysharp.Threading.Tasks;
using Senspark;
using Game.Manager;
using Scenes.FarmingScene.Scripts;
using Share.Scripts.Dialog;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class AutoMineButton : MonoBehaviour {
        [SerializeField]
        private Canvas canvasDialog;

        [SerializeField]
        private LevelScene levelScene;

        [SerializeField]
        private GameObject endTimeInfo;

        [SerializeField]
        private Text endTimeTxt;

        [SerializeField]
        private Text autoModeTxt;

        [SerializeField]
        private Button openPackageBtn;

        private IFeatureManager _featureManager;
        private ILanguageManager _languageManager;
        private IStorageManager _storageManager;
        private ISoundManager _soundManager;
        private IServerManager _serverManager;
        private IAnalytics _analytics;
        private ObserverHandle _handle;
        private DialogAutoMinePackage _dialog;

        private void Awake() {
            _featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
            _languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
            _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            
            _handle = new ObserverHandle();
            _handle.AddObserver(_storageManager, new StoreManagerObserver() {
                OnAutoMineChanged = OnAutoMineChanged,
                OnRefreshAutoMine = OnRefreshAutoMine
            });

            endTimeInfo.SetActive(false);
            openPackageBtn.interactable = CanBuyAutoMine();
        }

        private void OnDestroy() {
            _handle.Dispose();
        }

        private void Start() {
            CheckAutoMineValid();
        }

        public void OnAutoMineTriggerEnter() {
            if (canvasDialog.transform.childCount > 0) {
                return;
            }
            if (_storageManager.AutoMineInfo == null) {
                return;
            }
            if (_storageManager.AutoMineInfo.ActiveAutoMine) {
                endTimeInfo.SetActive(true);
            }
        }

        public void OnAutoMineTriggerExit() {
            if (_storageManager.AutoMineInfo == null) {
                return;
            }
            if (_storageManager.AutoMineInfo.ActiveAutoMine) {
                endTimeInfo.SetActive(false);
            }
        }

        public void OnAutoMineClicked() {
            _soundManager.PlaySound(Audio.Tap);
            if (_storageManager.AutoMineInfo == null) {
                return;
            }
            var autoMineInfo = _storageManager.AutoMineInfo;
            if (!autoMineInfo.ActiveAutoMine) {
                return;
            }
            // Nếu đang còn hạn sử dụng automine thì cho bật tắt tự do
            // Mỗi lần bật sẽ gửi lên server
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    if (!_storageManager.EnableAutoMine) {
                        var enable = false;
                        if (AppConfig.IsSolana()) {
                            enable = await _serverManager.UserSolanaManager.StartAutoMineSol();
                        } else {
                            enable = await _serverManager.General.StartAutoMine();
                        }
                        _storageManager.EnableAutoMine = enable;
                        if (enable) {
                            _analytics.TrackConversion(ConversionType.UseAutoMine);
                        }
                    } else {
                        _storageManager.EnableAutoMine = false;
                    }
                } catch (Exception e) {
                    DialogOK.ShowError(canvasDialog, e.Message);
                } finally {
                    waiting.End();
                }
            });
        }

        public void OnOpenDialogAutoMineBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            if (_dialog != null) {
                return;
            }
            // Nếu đã hết hạn sử dụng automine thì cho hiện dialog
            // Nếu hết hạn trong 2 ngày thì sẽ cho mua gói
            if (CanBuyAutoMine()) {
                UniTask.Void(async () => {
                    CloseOtherDialog();
                    levelScene.PauseStatus.SetValue(this, true);
                    _dialog = await DialogAutoMinePackage.Create();
                    _dialog.Show(canvasDialog);
                    _dialog.OnDidHide(() => {
                        levelScene.ResetButtonEvents();
                    });
                    await _dialog.WaitForHide();
                    _dialog = null;
                    levelScene.PauseStatus.SetValue(this, false);
                });
            }
        }

        private void CheckAutoMineValid() {
            var autoMineInfo = _storageManager.AutoMineInfo;
            if (autoMineInfo == null || !autoMineInfo.ActiveAutoMine) {
                _storageManager.EnableAutoMine = false;
            }
            DisplayOnOffTextOrSprite();
            SetEndDateString();
        }

        private void DisplayOnOffTextOrSprite() {
            var enableAutoMine = _storageManager.EnableAutoMine;
            var status = enableAutoMine ? "On" : "Off";
            var color = enableAutoMine ? Color.green : Color.red;
            autoModeTxt.text = $"{status}";
            autoModeTxt.color = color;
        }
        
        private void SetEndDateString() {
            var info = _languageManager.GetValue(LocalizeKey.ui_end_time_automine);
            var str = string.Format(info, _storageManager?.AutoMineInfo?.EndTime);
            endTimeTxt.text = $"{str}";
        }

        private void OnAutoMineChanged(bool autoMine) {
            CheckAutoMineValid();
        }

        private bool CanBuyAutoMine() {
            var autoMineInfo = _storageManager.AutoMineInfo;
            if (autoMineInfo == null) {
                return false;
            }
            return (autoMineInfo.CanBuyAutoMine || !autoMineInfo.ActiveAutoMine) && _featureManager.EnableBuyAutoMine;
        }

        private void OnRefreshAutoMine() {
            SetEndDateString();
            openPackageBtn.interactable = CanBuyAutoMine();
        }
        
        private void CloseOtherDialog() {
            var trans = canvasDialog.transform;
            var count = trans.childCount;
            for (var i = 0; i < count; i++) {
                var other = trans.GetChild(i).GetComponent<Dialog.Dialog>();
                if (other != null) {
                    other.Hide();
                }
            }
        }
    }
}