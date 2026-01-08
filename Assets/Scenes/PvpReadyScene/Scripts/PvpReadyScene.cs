using System;
using System.Collections.Generic;
using System.Linq;

using App;

using BLPvpMode;
using BLPvpMode.Data;
using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.User;
using BLPvpMode.Manager.Api.Modules;
using BLPvpMode.UI;

using Cysharp.Threading.Tasks;

using Engine.Entities;
using Engine.Manager;

using Game.Dialog;
using Game.Dialog.BomberLand.BLGacha;
using Game.UI.Animation;

using JetBrains.Annotations;

using PvpMode.Manager;
using PvpMode.Utils;

using Reconnect;
using Reconnect.Backend;
using Reconnect.View;

using Scenes.PvpModeScene.Scripts;

using Senspark;

using Services;

using Share.Scripts.Dialog;
using Share.Scripts.Utils;

using TMPro;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Scenes.PvpReadyScene.Scripts {
    public class PvpReadyScene : MonoBehaviour {
        [SerializeField]
        private Canvas canvasDialog;

        [SerializeField]
        private BLHeroReadyCom[] heroesReadyCom;

        [SerializeField]
        private ButtonZoomAndFlash buttonReady;

        [SerializeField]
        private BLProfileCard profileCard;

        [SerializeField]
        private TextMeshProUGUI timerText;

        [SerializeField]
        private Text waitOpponentText;

        [SerializeField]
        private GameObject vs;

        [SerializeField]
        private GameObject score;

        [SerializeField]
        private TextMeshProUGUI scoreText;

        [SerializeField]
        private TextMeshProUGUI titleText;

        [SerializeField]
        private TextMeshProUGUI roundText;
        
        [SerializeField]
        private BLGachaRes resource;

        private const float StartTime = 30;
        private float _timeToStart;
        private float _timeProcess;
        private bool _stopProcess;

        private ILogManager _logManager;

        private ISceneManager _sceneLoader;

        private IServerManager _serverManager;
        private ObserverHandle _handle;
        private ISoundManager _audioManager;
        private ILanguageManager _languageManager;
        private IStorageManager _storageManager;
        private BoosterStatus _boosterStatus;

        [CanBeNull]
        private IUser _interactiveUser; // Use to click buttons.

        private IUser[] _users;
        private List<IUserModule> _modules;
        private bool[] _readies;
        private bool _readying;
        private IReconnectStrategy _gameReconnectStrategy;
        private IReconnectStrategy _pvpReconnectStrategy;
        private IInputManager _inputManager;

        private void Awake() {
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _sceneLoader = ServiceLocator.Instance.Resolve<ISceneManager>();
            _audioManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
            _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _boosterStatus = _storageManager.PvPBoosters;
            _handle = new ObserverHandle();
            _handle.AddObserver(_serverManager, new ServerObserver {
                    OnServerStateChanged = OnServerStateChanged, //
                });
            _gameReconnectStrategy = new DefaultReconnectStrategy(
                _logManager,
                new MainReconnectBackend(),
                new LogReconnectView()
            );
            _gameReconnectStrategy.Start();
            
            _inputManager = ServiceLocator.Instance.Resolve<IInputManager>();
        }

        private void OnDestroy() {
            _handle.Dispose();
            _gameReconnectStrategy.Dispose();
            _pvpReconnectStrategy.Dispose();
        }

        private void Start() {
            _timeToStart = StartTime;
            _timeProcess = 0;
            _stopProcess = false;
        }

        private void Update() {
            if (_stopProcess) {
                return;
            }
            if (Input.GetKeyDown(KeyBoardInputDefine.Ready) ||
                _inputManager.ReadButton(_inputManager.InputConfig.Enter)) {
                OnReadyButtonClick();
            }
            _timeProcess += Time.deltaTime;
            if (_timeProcess < 1) {
                return;
            }
            _timeProcess = 0;
            _timeToStart -= 1;
            if (_timeToStart <= 0) {
                timerText.text = "0";
                _stopProcess = true;
                // Force Ready in server side.
                //OnReadyButtonClick();
            } else {
                timerText.text = $"{_timeToStart}";
            }
        }

        public async void SetInfo(
            [NotNull] IUser[] users,
            List<IUserModule> modules,
            string strRound = null,
            string strScores = null
        ) {
            Assert.IsNull(_users, "Already initialized");
            _interactiveUser = users.FirstOrDefault(user => user.IsParticipant && !user.IsBot);
            _users = users;
            _modules = modules;

            Assert.IsTrue(users.Length > 0, "Empty users");
            // Only need to listen to 1 user events.
            var user = users[0];
            var userInfo = user.MatchInfo.Info; // User info list is the same for all users.

            _handle.AddObserver(user, new UserObserver {
                OnReady = OnReady,
                OnStartRound = OnStartRound,
            });
            
            _pvpReconnectStrategy = new MultiReconnectStrategy(users.Select(it =>
                (IReconnectStrategy) new DefaultReconnectStrategy(
                    _logManager,
                    new PvpReconnectBackend(it),
                    it.IsBot
                        ? new LogReconnectView()
                        : new WaitingReconnectView(canvasDialog)
                )).ToArray());
            _pvpReconnectStrategy.Start();

            // Ready status.
            _readying = false;
            _readies = new bool[userInfo.Length];
            waitOpponentText.gameObject.SetActive(false);

            for (var slot = 0; slot < userInfo.Length; ++slot) {
                if (slot >= heroesReadyCom.Length) {
                    // FIXME: UI not supported.
                    continue;
                }
                var userData = userInfo[slot];
                var heroCom = heroesReadyCom[slot];

                // Avatar.
                heroCom.avatar.ChangeImage(DefaultPlayerManager.TrPlayerType(userData.Hero.Skin), PlayerColor.HeroTr);

                // Name.
                string userName;
                if (string.IsNullOrEmpty(userData.DisplayName)) {
                    userName = Ellipsis.EllipsisAddress(App.Utils.GetShortenName(userData.Username));
                } else {
                    userName = Ellipsis.EllipsisAddress(App.Utils.GetShortenName(userData.DisplayName));
                }
                heroCom.addressText.text = userName;

                // Ready.
                SetReady(slot, false);

                // Boosters.
                var boosters = userData.Boosters;
                foreach (var iter in boosters) {
                    var boosterType = DefaultBoosterManager.ConvertFromId(iter);
                    heroCom.boosterDisplay.ShowBooster(boosterType);
                }

                // Rank.
                heroCom.ranks.text = BLProfileCard.GetRankName((PvpRankType)userData.Rank);
                heroCom.rankIcons.sprite = await profileCard.GetSprite((PvpRankType) userData.Rank);
                
                // AvatarTR
                var sprites = await resource.GetAvatar(userData.Avatar);
                heroCom.avatarTR.StartAni(sprites);
            }

            // Round Info
            if (strRound == null) {
                return;
            }
            roundText.text = strRound;
            titleText.text = "NEXT ROUND START IN";
            vs.SetActive(false);
            score.SetActive(true);
            scoreText.text = strScores;
            UpdateReadyStatus();
        }
        
        private void OnReadyClick(bool value) {
            if (value && !_stopProcess) {
                OnReadyButtonClick();
            }
        }
        public void OnReadyButtonClick() {
            _logManager.Log();
            _audioManager.PlaySound(Audio.Tap);
            Ready();
        }

        private void Ready() {
            // Fix không cho request nhiều lần lên server
            if (_readying) {
                return;
            }
            _logManager.Log();
            _readying = true;
            UpdateReadyStatus();
            UniTask.Void(async () => {
                try {
                    Assert.IsNotNull(_interactiveUser);
                    await _interactiveUser.Ready();
                } catch (Exception ex) {
                    DialogOK.ShowError(canvasDialog, ex.Message);
                } finally {
                    _readying = false;
                    UpdateReadyStatus();
                }
            });
        }

        private void SetReady(int slot, bool ready) {
            _logManager.Log($"slot={slot} ready={ready}");
            _readies[slot] = ready;
            UpdateReadyStatus();
            if (slot >= heroesReadyCom.Length) {
                // FIXME: UI not supported.
                return;
            }
            var heroCom = heroesReadyCom[slot];
            var color = heroCom.readyText.color;
            color.a = ready ? 1f : 100f / 255;
            heroCom.readyText.color = color;
        }

        private void UpdateReadyStatus() {
            if (_interactiveUser == null) {
                waitOpponentText.gameObject.SetActive(false);
                buttonReady.gameObject.SetActive(false);
                return;
            }
            var slot = _interactiveUser.MatchInfo.Slot;
            if (_readies[slot]) {
                waitOpponentText.gameObject.SetActive(true);
                buttonReady.gameObject.SetActive(false);
            } else {
                waitOpponentText.gameObject.SetActive(false);
                // Sending ready event.
                buttonReady.gameObject.SetActive(!_readying);
            }
        }

        private void OnServerStateChanged(ServerConnectionState state) {
            if (_stopProcess) {
                return;
            }
            if (state == ServerConnectionState.LostConnection) {
                // FIXME: flow disconnect chưa hoàn thiện
                _stopProcess = true;
                DialogOK.ShowError(canvasDialog, "Disconnected",
                    () => {
                        const string sceneName = "MainMenuScene";
                        SceneLoader.LoadSceneAsync(sceneName).Forget();
                    });
            }
        }

        private void OnReady(IMatchReadyData data) {
            _logManager.Log();
            SetReady(data.Slot, true);
        }

        private void OnStartRound(IMatchStartData data) {
            _logManager.Log();
            _storageManager.PvpMatchId = data.Match.Id;
            void OnLoaded(GameObject obj) {
                var pvpScene = obj.GetComponent<BLLevelScenePvp>();
                pvpScene.IsCountDownEnabled = true;
                pvpScene.SetPvpMapDetails(
                    _users,
                    _modules,
                    data.Match,
                    data.Map,
                    _boosterStatus
                );
            }
            const string sceneName = "PvpModeScene";
            SceneLoader.LoadSceneAsync(sceneName, OnLoaded).Forget();
        }
    }
}