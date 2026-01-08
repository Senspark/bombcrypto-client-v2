// #define USE_UDP
using System;
using System.Threading.Tasks;
using App;
using BLPvpMode.Engine;
using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.Info;
using BLPvpMode.Engine.User;
using BLPvpMode.Manager.Api.Handlers;
using BLPvpMode.Manager.Api.Listeners;
using CustomSmartFox;
using Data;
using JetBrains.Annotations;
using Senspark;
using UnityEngine;

namespace BLPvpMode.Manager.Api {
    public class RemoteUser : ObserverManager<UserObserver>, IUser {
        private const string Zone = "PVPZone";

        [NotNull]
        private readonly IServerBridge _bridge;

        [NotNull]
        private readonly ISmartFoxApi _api;

        [NotNull]
        private readonly ILogManager _logManager;

        [NotNull]
        private readonly ITimeManager _timeManager;

        [NotNull]
        private readonly PvPServerData _serverConfig;

        [NotNull]
        private readonly IServerListener[] _listeners;

        [NotNull]
        private readonly string _username;

        [NotNull]
        private readonly object _statusLocker;

        private readonly ITaskDelay _taskDelay;
        private readonly LatencyMonitor _latencyMonitor;
        private readonly bool _useWebSocket;
        private UserStatus _status;
        private bool _udpEnabled;
        private bool _disposed;
        private readonly IExtResponseEncoder _encoder;

        public bool IsParticipant => MatchInfo.Slot < MatchInfo.Info.Length;
        public bool IsBot => MatchInfo.Info[MatchInfo.Slot].IsBot;
        public IMatchInfo MatchInfo { get; }

        public UserStatus Status {
            get {
                lock (_statusLocker) {
                    return _status;
                }
            }
            private set {
                lock (_statusLocker) {
                    if (_status == value) {
                        return;
                    }
                    _status = value;
                    DispatchEvent(observer => observer.OnChangeStatus?.Invoke(Status));
                }
            }
        }

        public RemoteUser(
            [NotNull] PvPServerData serverConfig,
            [NotNull] IMatchInfo matchInfo,
            [NotNull] string username,
            bool useWebSocket,
            [NotNull] ILogManager logManager,
            [NotNull] ITimeManager timeManager,
            [NotNull] ITaskDelay taskDelay,
            [NotNull] IExtResponseEncoder encoder
        ) {
            _encoder = encoder;
            _bridge = new ServerBridge(useWebSocket, serverConfig.UseSSL, false);
            _api = new SmartFoxApi(_bridge, encoder, null);
            _logManager = logManager;
            _timeManager = timeManager;
            _latencyMonitor = new LatencyMonitor(1f, 3f, () => { //
                _ = Disconnect();
            },
                logManager);
            _taskDelay = taskDelay;
            _serverConfig = serverConfig;
            _username = username;
            _statusLocker = new object();
            _useWebSocket = useWebSocket;
            _status = UserStatus.Disconnected;
            _udpEnabled = false;
            MatchInfo = matchInfo;
            _listeners = new IServerListener[] {
                new ConnectionRetryListener(() => {
                    _logManager.Log($"[RemoteUser:ConnectionRetry] slot={MatchInfo.Slot}");
                    ThreadUltils.Thread.RunOnLibraryThread(() => {
                        Status = UserStatus.Connecting; //
                        _latencyMonitor.StopMonitoring();
                    });
                }),
                new ConnectionResumeListener(() => {
                    _logManager.Log($"[RemoteUser:ConnectionResume] slot={MatchInfo.Slot}");
                    ThreadUltils.Thread.RunOnLibraryThread(() => {
                        Status = UserStatus.Connected; //
                        _latencyMonitor.StartMonitoring();
                    });
                }),
                new ConnectionLostListener(reason => {
                    _logManager.Log($"[RemoteUser:ConnectionLost] slot={MatchInfo.Slot} reason={reason}");
                    ThreadUltils.Thread.RunOnLibraryThread(() => {
                        Status = UserStatus.Disconnected;
                        _latencyMonitor.StopMonitoring();
                    });
                }),
                new PingListener(data => {
                    if (Status == UserStatus.Connected) {
                        _ = _api.Process(new PingHandler(encoder, data.RequestId, timeManager.Timestamp, _udpEnabled));
                    }
                    ThreadUltils.Thread.RunOnLibraryThread(() => {
                        _latencyMonitor.NotifyPingSuccess();
                        DispatchEvent(observer => observer.OnPing?.Invoke(data));
                    });
                }),
                new StartReadyListener(() => {
                    _logManager.Log($"[RemoteUser:StartReady] slot={MatchInfo.Slot}");
                    ThreadUltils.Thread.RunOnLibraryThread(() => DispatchEvent(observer => observer.OnStartReady?.Invoke()));
                }),
                new ReadyListener(data => {
                    _logManager.Log($"[RemoteUser:Ready] slot={MatchInfo.Slot}");
                    ThreadUltils.Thread.RunOnLibraryThread(() => DispatchEvent(observer => observer.OnReady?.Invoke(data)));
                }),
                new StartRoundListener(data => {
                    _logManager.Log($"[RemoteUser:StartRound] slot={MatchInfo.Slot}");
                    ThreadUltils.Thread.RunOnLibraryThread(() => DispatchEvent(observer => observer.OnStartRound?.Invoke(data)));
                }),
                new UseEmojiListener(data => {
                    _logManager.Log($"[RemoteUser:UseEmoji] slot={MatchInfo.Slot}");
                    ThreadUltils.Thread.RunOnLibraryThread(() => DispatchEvent(observer => observer?.OnUseEmoji?.Invoke(data)));
                }),
                new FallingBlockListener(data => {
                    ThreadUltils.Thread.RunOnLibraryThread(() =>
                        DispatchEvent(observer => observer?.OnFallingBlocks?.Invoke(data)));
                }),
                new ChangeStateListener(data => {
                    ThreadUltils.Thread.RunOnLibraryThread(() => {
                        _latencyMonitor.NotifyPingSuccess();
                        DispatchEvent(observer => observer.OnChangeState?.Invoke(data));
                    });
                }),
                new FinishRoundListener(data => {
                    _logManager.Log($"[RemoteUser:FinishRound] slot={MatchInfo.Slot}");
                    ThreadUltils.Thread.RunOnLibraryThread(() => DispatchEvent(observer => observer.OnFinishRound?.Invoke(data)));
                }),
                new FinishMatchListener(data => {
                    _logManager.Log($"[RemoteUser:FinishMatch] slot={MatchInfo.Slot}");
                    ThreadUltils.Thread.RunOnLibraryThread(() => DispatchEvent(observer => observer.OnFinishMatch?.Invoke(data)));
                }),
            };
            foreach (var listener in _listeners) {
                _api.AddListener(listener);
            }
        }

        ~RemoteUser() => Dispose(false);

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing) {
            if (_disposed) {
                return;
            }
            if (disposing) {
                foreach (var listener in _listeners) {
                    _api.RemoveListener(listener);
                }
                _api.Dispose();
                _bridge.Dispose();
                _latencyMonitor.StopMonitoring();
            }
            _disposed = true;
        }

        public async Task Connect() {
            _logManager.Log($"[RemoteUser:Connect] slot={MatchInfo.Slot} status={Status}");
            if (Status is UserStatus.Connected or UserStatus.Connecting) {
                return;
            }
            try {
                Status = UserStatus.Connecting;
                _udpEnabled = false;
                _api.Reinitialize();
                await ConnectTcp();
                await ThreadUltils.Thread.SwitchToLibraryThread();
                Status = UserStatus.Connected;
                _latencyMonitor.StartMonitoring();
#if USE_UDP
                EE.Utils.NoAwait(async () => {
                    // Attempt to use UDP.
                    try {
                        _udpEnabled = await ConnectUdp();
                        await ThreadUltils.Thread.SwitchToLibraryThread();
                    } catch (Exception ex) {
                        Debug.LogException(ex);
                    }
                });
#endif
            } catch (Exception ex) {
                _logManager.Log($"[RemoteUser:Connect] slot={MatchInfo.Slot} ex={ex.Message}");
                await ThreadUltils.Thread.SwitchToLibraryThread();
                Status = UserStatus.Disconnected;
                _latencyMonitor.StopMonitoring();
                throw;
            }
        }

        public async Task Disconnect() {
            _logManager.Log($"[RemoteUser:Disconnect] slot={MatchInfo.Slot} status={Status}");
            if (Status is UserStatus.Disconnected or UserStatus.Connecting) {
                return;
            }
            await _api.Process(new DisconnectHandler());
            Status = UserStatus.Disconnected;
            _latencyMonitor.StopMonitoring();
        }

        public async Task KillConnection() {
            await _api.Process(new KillConnectHandler());
            await ThreadUltils.Thread.SwitchToLibraryThread();
        }

        private async Task ConnectTcp() {
            if (_useWebSocket) {
                // Use websocket.
                await _api.Process(new ConnectHandler(_logManager, _serverConfig.Host, _serverConfig.Port, true,
                    _taskDelay));
            } else {
                // Use TCP socket.
                await _api.Process(new ConnectHandler(_logManager, _serverConfig.UdpHost, _serverConfig.UdpPort, true,
                    _taskDelay));
            }
            await _api.Process(new LoginHandler(Zone, MatchInfo, _username));
            await ThreadUltils.Thread.SwitchToLibraryThread();
        }

        private async Task<bool> ConnectUdp() {
            if (_api.IsUdpAvailable) {
                var result = await _api.Process(new InitUdpHandler(_serverConfig.UdpHost, _serverConfig.UdpPort));
                await ThreadUltils.Thread.SwitchToLibraryThread();
                return result;
            }
            return false;
        }

        public async Task Ready() {
            if (Status != UserStatus.Connected) {
                throw new InvalidOperationException("Current status is not connected");
            }
            await _api.Process(new ReadyHandler(_encoder));
            await ThreadUltils.Thread.SwitchToLibraryThread();
        }

        public async Task Quit() {
            if (Status != UserStatus.Connected) {
                throw new InvalidOperationException("Current status is not connected");
            }
            await _api.Process(new QuitHandler(_encoder));
            await ThreadUltils.Thread.SwitchToLibraryThread();
        }

        public async Task<IMoveHeroData> MoveHero(Vector2 position) {
            if (Status != UserStatus.Connected) {
                throw new InvalidOperationException("Current status is not connected");
            }
            var result = await _api.Process(new MoveHeroHandler(_encoder, _timeManager, position, _udpEnabled));
            await ThreadUltils.Thread.SwitchToLibraryThread();
            return result;
        }

        public async Task<IPlantBombData> PlantBomb() {
            if (Status != UserStatus.Connected) {
                throw new InvalidOperationException("Current status is not connected");
            }
            var result = await _api.Process(new PlantBombHandler(_encoder, _timeManager));
            await ThreadUltils.Thread.SwitchToLibraryThread();
            return result;
        }

        public async Task ThrowBomb() {
            if (Status != UserStatus.Connected) {
                throw new InvalidOperationException("Current status is not connected");
            }
            await _api.Process(new ThrowBombHandler(_encoder, _timeManager));
            await ThreadUltils.Thread.SwitchToLibraryThread();
        }

        public async Task UseBooster(Booster item) {
            if (Status != UserStatus.Connected) {
                throw new InvalidOperationException("Current status is not connected");
            }
            await _api.Process(new UseBoosterHandler(_encoder, _timeManager, item));
            await ThreadUltils.Thread.SwitchToLibraryThread();
        }

        public async Task UseEmoji(int itemId) {
            if (Status != UserStatus.Connected) {
                throw new InvalidOperationException("Current status is not connected");
            }
            await _api.Process(new UseEmojiHandler(_encoder, _timeManager, itemId));
            await ThreadUltils.Thread.SwitchToLibraryThread();
        }
    }
}