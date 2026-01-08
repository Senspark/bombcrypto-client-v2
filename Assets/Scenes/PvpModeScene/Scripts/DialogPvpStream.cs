using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using App;

using BLPvpMode.Data;
using BLPvpMode.Manager;

using Cysharp.Threading.Tasks;

using Data;

using Game.Dialog;
using Game.Manager;

using PvpMode.Component;

using PvpSchedule.Models;

using Senspark;

using Services;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.PvpModeScene.Scripts {
    public class DialogPvpStream : Dialog {
        [SerializeField]
        private ButtonCancelFind buttonCancel;

        [SerializeField]
        private Text countDownText;

        private ISceneManager _sceneLoader;
        private IServerManager _serverManager;
        private ILogManager _logManager;
        private ISoundManager _audioManager;
        private IStorageManager _storageManager;
        private IPingManager _pingManager;
        private IInventoryManager _inventoryManager;

        private Dictionary<string, TimeSpan> _pings;
        private Dictionary<string, PvPServerData> _servers;
        private TaskCompletionSource<bool> _tcs;

        private IPvpJoinManager _pvpJoinManager;
        private ObserverHandle _handle;

        // count down 30s
        private const float TimeCountDown = 30f;
        private float _timeToStop;
        private float _timeProcess;
        private bool _stopProcess;

        public static UniTask<DialogPvpStream> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogPvpStream>();
        }
        
        protected override void Awake() {
            base.Awake();
            _sceneLoader = ServiceLocator.Instance.Resolve<ISceneManager>();
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            _audioManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _pingManager = ServiceLocator.Instance.Resolve<IPingManager>();
            _inventoryManager = ServiceLocator.Instance.Resolve<IInventoryManager>();

            _handle = new ObserverHandle();
            _handle.AddObserver(_serverManager, new ServerObserver { OnServerStateChanged = OnServerStateChanged });

            _pvpJoinManager = new PvpJoinManager(
                _serverManager,
                _storageManager,
                _pingManager,
                _logManager,
                _inventoryManager);
            _handle.AddObserver(_pvpJoinManager, new PvpJoinObserver { ChangeJoinStatus = ChangeJoinStatus, });
        }

        private void Start() {
            buttonCancel.OnCancelCallback = OnCancelFinding;
        }

        protected override void OnDestroy() {
            _pvpJoinManager.Destroy();
            _handle.Dispose();
        }

        private void Update() {
            if (_stopProcess) {
                return;
            }
            _timeProcess += Time.deltaTime;
            if (_timeProcess < 1) {
                return;
            }
            _timeProcess = 0;
            _timeToStop -= 1;
            if (_timeToStop <= 0) {
                countDownText.text = "";
                _stopProcess = true;
            } else {
                countDownText.text = $"{_timeToStop}";
            }
        }

        private void ShowErrorAndKick(string reason) {
            DialogOK.ShowErrorAndKickToConnectScene(DialogCanvas, reason);
        }

        private void OnCancelFinding() {
            _audioManager.PlaySound(Audio.Tap);
            SendCancelFindingRequest();
        }

        private void SendCancelFindingRequest() {
            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    //await _pvpJoinHelper.CancelFinding();
                    await _pvpJoinManager.CancelFinding();
                    Hide();
                } catch (Exception e) {
                    DialogOK.ShowError(DialogCanvas, e.Message, () => Hide());
                } finally {
                    buttonCancel.Hide();
                    StopCountDown();
                    waiting.End();
                }
            });
        }

        private void OnServerStateChanged(ServerConnectionState obj) {
            DialogOK.ShowErrorAndKickToConnectScene(DialogCanvas, "Disconnected");
        }

        private void StartCountDown() {
            countDownText.text = $"{TimeCountDown}";
            _timeToStop = TimeCountDown;
            _timeProcess = 0;
            _stopProcess = false;
        }

        private void StopCountDown() {
            countDownText.text = "";
            _stopProcess = true;
        }

        private void ChangeJoinStatus(JoinStatus status) {
            switch (status) {
                case JoinStatus.InQueue:
                    buttonCancel.ShowButtonCancel();
                    StartCountDown();
                    break;
                case JoinStatus.None:
                case JoinStatus.Joining:
                    break;
            }
        }

        public void JoinPvpQueue(int heroId, IPvpMatchSchedule model) {
            buttonCancel.Show();
            UniTask.Void(async () => {
                try {
                    _storageManager.PvPBoosters = new BoosterStatus(0);
                    var results = await _pvpJoinManager.FindMatch(model.Mode, model.MatchId);
                    var infoList = results.Select(item => item.MatchInfo).ToArray();
                    var helper = new MatchHelper(
                        new PredefinedMatchFinder(infoList),
                        new RemoteUserFactory()
                    );
                    await helper.Start(model.MatchId);
                } catch (Exception ex) {
                    if (ex is PvpJoinException pvpJoinException) {
                        switch (pvpJoinException.Result) {
                            case PvpJoinExceptionType.CancelFinding:
                                // Do nothing
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    } else if (ex.Message.Contains("User joined room")) {
                        _serverManager.Disconnect();
                    } else {
                        DialogOK.ShowError(DialogCanvas, ex.Message);
                        buttonCancel.Hide();
                        StopCountDown();
                        Hide();
                    }
                }
            });
        }

        public async Task JoinMatch(IPvpRoomInfo info) {
            buttonCancel.Show();
            _storageManager.PvPBoosters = new BoosterStatus(0);
            var helper = new MatchHelper(
                new PredefinedMatchFinder(new[] { info.MatchInfo }),
                new RemoteUserFactory()
            );
            await helper.Start();
        }
    }
}