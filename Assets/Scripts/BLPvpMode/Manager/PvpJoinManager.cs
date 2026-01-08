using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using App;

using BLPvpMode.Queue;

using Cysharp.Threading.Tasks;

using Senspark;

using Services;

using Sfs2X.Entities.Data;

using Task = System.Threading.Tasks.Task;

namespace BLPvpMode.Manager {
    public class PvpJoinManager : ObserverManager<PvpJoinObserver>, IPvpJoinManager {
        private readonly IServerManager _serverManager;
        private readonly IStorageManager _storageManager;
        private readonly IPingManager _pingManager;
        private readonly ILogManager _logManager;
        private readonly IInventoryManager _inventoryManager;
        private ObserverHandle _handle;
        private TaskCompletionSource<IFindMatchResult[]> _taskFindMatch;

        public JoinStatus JoinStatus { get; private set; }
        private List<IFindMatchResult> _matchList;
        private bool _waitMatch;

        public PvpJoinManager(
            IServerManager serverManager,
            IStorageManager storageManager,
            IPingManager pingManager,
            ILogManager logManager,
            IInventoryManager inventoryManager
        ) {
            _serverManager = serverManager;
            _storageManager = storageManager;
            _pingManager = pingManager;
            _logManager = logManager;
            _inventoryManager = inventoryManager;
            _handle = new ObserverHandle();
            _handle.AddObserver(_serverManager, new ServerObserver { OnExtensionResponse = OnExtensionResponse, });
            JoinStatus = JoinStatus.None;
            UniTask.Void(async () => {
                while (_handle != null) {
                    if (JoinStatus == JoinStatus.InQueue) {
                        _serverManager.General.KeepJoiningPvPQueue();
                    }
                    await WebGLTaskDelay.Instance.Delay(10000);
                }
            });
        }

        public void Destroy() {
            _handle?.Dispose();
            _handle = null;
        }

        private void UpdateStatus(JoinStatus status) {
            _logManager.Log($"PvpJoin: UpdateStatus {status}");
            JoinStatus = status;
            DispatchEvent(e => e.ChangeJoinStatus?.Invoke(JoinStatus));
        }

        private void OnExtensionResponse(string command, ISFSObject data) {
            switch (command) {
                case SFSDefine.SFSCommand.PVP_FOUND_MATCH:
                    OnFoundMatch(data.ToJson());
                    break;
            }
        }

        private void OnFoundMatch(string data) {
            // Nếu chưa gọi FindMatch ở instance trong FindMatchController
            // Mà DialogPvpStream với instance khác gọi FindMatch
            // => _matchList == null
            if (_matchList == null) {
                return;
            }
            
            _logManager.Log($"PvpJoin: OnFoundMatch {data}");
            _matchList.Add(new FindMatchResult(data));
            WaitForNextMatch();
        }

        private void WaitForNextMatch() {
            if (_waitMatch) {
                return;
            }
            _waitMatch = true;
            UniTask.Void(async () => {
                await WebGLTaskDelay.Instance.Delay(1000);
                FinishFindMatch();
            });
        }

        private void FinishFindMatch() {
            _taskFindMatch?.SetResult(_matchList.ToArray());
        }

        public async Task<IFindMatchResult[]> FindMatch(global::BLPvpMode.Engine.Info.PvpMode mode, string matchId) {
            try {
                _matchList ??= new List<IFindMatchResult>();
                _matchList.Clear();

                long heroId = _storageManager.SelectedHeroKey;
                var boosters = _storageManager.PvPBoosters.GetSelectedBoosterIds();
                var pingInfo = await _pingManager.GetLatencies();
                _logManager.Log($"latencies={string.Join("", pingInfo.Select(it => $"[{it.ZoneId}={it.Latency}]"))}");

                UpdateStatus(JoinStatus.Joining);
                _logManager.Log($"PvpJoin: Request JoinQueue");
                var avatarId = await _inventoryManager.GetCurrentAvatarTR();
                await _serverManager.Pvp.JoinQueue(
                    (int) mode,
                    null,
                    (int) heroId,
                    boosters,
                    pingInfo,
                    avatarId,
                    false);
                _logManager.Log($"PvpJoin: Response JoinQueue");
                UpdateStatus(JoinStatus.InQueue);
                _taskFindMatch = new TaskCompletionSource<IFindMatchResult[]>();
                var results = await _taskFindMatch.Task;
                if (JoinStatus != JoinStatus.InQueue) {
                    _logManager.Log($"PvpJoin: throw CancelFinding");
                    throw new PvpJoinException(PvpJoinExceptionType.CancelFinding, "User cancelled");
                }
                _logManager.Log($"PvpJoin: Response FindMatch");
                return results;
            } finally {
                _waitMatch = false;
                _taskFindMatch = null;
            }
        }

        public async Task CancelFinding() {
            try {
                await _serverManager.Pvp.LeaveQueue();
                _logManager.Log($"PvpJoin: CancelFinding success");
                UpdateStatus(JoinStatus.None);
                _taskFindMatch?.SetResult(null);
            } catch (Exception e) {
                _logManager.Log($"PvpJoin: CancelFinding false");
                throw;
            }
        }
    }
}