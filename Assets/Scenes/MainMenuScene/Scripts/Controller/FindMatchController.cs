using System;
using System.Linq;

using Analytics;

using App;

using BLPvpMode.Manager;

using Cysharp.Threading.Tasks;

using Senspark;

using Services;

namespace Scenes.MainMenuScene.Scripts.Controller {
    public class FindMatchController {
        private IServerManager _serverManager;
        private IAnalytics _analytics;
        private IStorageManager _storageManager;
        private IPvPServerConfigManager _serverConfigManager;
        private IPvpJoinManager _pvpJoinManager;
        private ILogManager _logManager;
        private IPingManager _pingManager;
        private IInventoryManager _inventoryManager;

        private ObserverHandle _handleMain;

        // count down 30s
        private const float TimeCountDown = 30f;
        private float _timeToStop;
        private float _timeProcess;
        private bool _stopProcess;
        private bool _isDestroyed;

        private Action _showCancelButton;
        private Action _stopFinding;
        private Action<string> _updateCountDownText;
        private Action<string> _showError;

        public void Initialized(
            Action showCancelButton,
            Action stopFinding,
            Action<string> updateCountDownText,
            Action<string> showError
        ) {
            _showCancelButton = showCancelButton;
            _updateCountDownText = updateCountDownText;
            _stopFinding = stopFinding;
            _showError = showError;

            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _serverConfigManager = ServiceLocator.Instance.Resolve<IPvPServerConfigManager>();
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            _pingManager = ServiceLocator.Instance.Resolve<IPingManager>();
            _inventoryManager = ServiceLocator.Instance.Resolve<IInventoryManager>();

            _handleMain = new ObserverHandle();
            _pvpJoinManager = new PvpJoinManager(
                _serverManager,
                _storageManager,
                _pingManager,
                _logManager,
                _inventoryManager);

            _handleMain.AddObserver(_pvpJoinManager, new PvpJoinObserver {
                    ChangeJoinStatus = ChangeJoinStatus, //
                });
        }

        public void OnProcess(float delta) {
            if (_stopProcess) {
                return;
            }
            _timeProcess += delta;
            if (_timeProcess < 1) {
                return;
            }
            _timeProcess = 0;
            _timeToStop -= 1;
            if (_timeToStop <= 0) {
                _updateCountDownText.Invoke("");
                _stopProcess = true;
            } else {
                _updateCountDownText.Invoke($"{_timeToStop}");
            }
        }

        public void StartFindMatch() {
            var heroId = _storageManager.SelectedHeroKey;
            var pvpHeroId = new HeroId(heroId, HeroAccountType.Tr);
            JoinPvPQueue(global::BLPvpMode.Engine.Info.PvpMode.FFA_2, null, pvpHeroId);
            _analytics.TrackClickPlayPvp(pvpHeroId.Id);
        }

        public void CancelFinding() {
            SendCancelFindingRequest();
        }

        public void OnDestroy() {
            _isDestroyed = true;
            _pvpJoinManager.Destroy();
            _handleMain.Dispose();
        }

        private void JoinPvPQueue(global::BLPvpMode.Engine.Info.PvpMode mode, string matchId, HeroId heroId) {
            UniTask.Void(async () => {
                try {
                    var results = await _pvpJoinManager.FindMatch(mode, matchId);
                    var infoList = results.Select(item => item.MatchInfo).ToArray();
                    var helper = new MatchHelper(
                        new PredefinedMatchFinder(infoList),
                        new RemoteUserFactory()
                    );
                    await helper.Start();
                } catch (Exception ex) {
                    if (ex is PvpJoinException pvpJoinException) {
                        switch (pvpJoinException.Result) {
                            case PvpJoinExceptionType.CancelFinding:
                                // Do nothing
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    } else {
                        _showError.Invoke(ex.Message);
                        _stopFinding.Invoke();
                        StopCountDown();
                    }
                }
            });
        }

        private void SendCancelFindingRequest() {
            UniTask.Void(async () => {
                try {
                    await _pvpJoinManager.CancelFinding();
                    // Fix: thêm delay 0.5s sau khi cancel do server chưa xử lý bất đồng bộ.
                    await WebGLTaskDelay.Instance.Delay(500);
                } catch (Exception e) {
                    // Fix: Server Không cho CANCEL vì đã vào trận, Client phải chờ 2 giây để nhận match info.
                    await WebGLTaskDelay.Instance.Delay(2000);
                    if (!_isDestroyed && _pvpJoinManager.JoinStatus == JoinStatus.Joining) {
                        // CancelFinding không thành công, chờ quá 2s server vẫn chưa trả trả về thông tin
                        // tạm xử lý hiện dialog báo time-out
                        _showError.Invoke("Request find match time-out");
                    }
                } finally {
                    if (!_isDestroyed) {
                        _stopFinding.Invoke();
                        StopCountDown();
                    }
                }
            });
        }

        private void ChangeJoinStatus(JoinStatus status) {
            switch (status) {
                case JoinStatus.InQueue:
                    _showCancelButton.Invoke();
                    StartCountDown();
                    break;
                case JoinStatus.None:
                case JoinStatus.Joining:
                    break;
            }
        }

        private void StartCountDown() {
            _timeToStop = TimeCountDown;
            _timeProcess = 0;
            _stopProcess = false;
            _updateCountDownText.Invoke($"{TimeCountDown}");
        }

        private void StopCountDown() {
            _updateCountDownText.Invoke("");
            _stopProcess = true;
        }
    }
}