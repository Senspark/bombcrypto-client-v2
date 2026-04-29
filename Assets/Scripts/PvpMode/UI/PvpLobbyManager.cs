using UnityEngine;
using UnityEngine.UI;
using App;
using Senspark;
using Sfs2X.Entities.Data;
using PvpMode.Entities;
using System.Collections.Generic;
using BLPvpMode.Manager;
using Cysharp.Threading.Tasks;

namespace PvpMode.UI {
    public class PvpLobbyManager : MonoBehaviour {
        [SerializeField] private PvpModeSelector modeSelector;
        [SerializeField] private PvpChatPanel chatPanel;
        [SerializeField] private Button findMatchButton;
        [SerializeField] private Text playersOnlineText;
        [SerializeField] private Text activeMatchesText;
        [SerializeField] private GameObject matchingOverlay;
        [SerializeField] private Text matchingTimerText;
        [SerializeField] private Button cancelMatchButton;

        private IServerManager _serverManager;
        private IPvpJoinManager _joinManager;
        private ILogManager _logManager;
        private ObserverHandle _handle;
        private float _matchStartTime;

        private void Awake() {
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _joinManager = ServiceLocator.Instance.Resolve<IPvpJoinManager>();
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();

            _handle = new ObserverHandle();
            _handle.AddObserver(_serverManager, new ServerObserver {
                OnExtensionResponse = OnExtensionResponse
            });

            findMatchButton.onClick.AddListener(StartMatchmaking);
            cancelMatchButton.onClick.AddListener(CancelMatchmaking);
        }

        private void Start() {
            // Initial stats request
            RequestLobbyStats();
            
            // Repeat every 10s
            InvokeRepeating(nameof(RequestLobbyStats), 10f, 10f);
        }

        private void Update() {
            if (matchingOverlay.activeSelf) {
                float elapsed = Time.time - _matchStartTime;
                matchingTimerText.text = $"Searching... {elapsed:F1}s";
            }
        }

        private void OnDestroy() {
            _handle.Dispose();
            CancelInvoke();
        }

        private void RequestLobbyStats() {
            _serverManager.SendExtensionRequestAsync(new CustomExtCmd(SFSDefine.SFSCommand.PVP_LOBBY_STATS, new SFSObject()));
        }

        private void StartMatchmaking() {
            var mode = modeSelector.GetCurrentMode();
            bool isWagered = modeSelector.IsWagered();
            int wagerTier = modeSelector.GetCurrentWagerTierIndex();

            _matchStartTime = Time.time;
            matchingOverlay.SetActive(true);
            
            // Map the PvpGameMode to the old int mode if needed, or just use it as is
            // For now, let's assume we pass the new mode in the 'gameMode' slot
            
            UniTask.Void(async () => {
                try {
                    var results = await _joinManager.FindMatch(
                        global::BLPvpMode.Engine.Info.PvpMode.FFA_4, // Dummy old mode
                        null,
                        (int)mode,
                        isWagered ? 1 : 0,
                        wagerTier,
                        0 // token default to 0 for BSC BCOIN
                    );
                    
                    if (results != null) {
                        _logManager.Log("Match Found!");
                        // Transition to PvpReadyScene handled by other systems usually
                    }
                } catch (PvpJoinException ex) when (ex.Result == PvpJoinExceptionType.CancelFinding) {
                    _logManager.Log("Matchmaking cancelled");
                } catch (System.Exception ex) {
                    _logManager.LogError($"Matchmaking error: {ex.Message}");
                    matchingOverlay.SetActive(false);
                }
            });
        }

        private void CancelMatchmaking() {
            UniTask.Void(async () => {
                await _joinManager.CancelFinding();
                matchingOverlay.SetActive(false);
            });
        }

        private void OnExtensionResponse(string cmd, ISFSObject data) {
            if (cmd == SFSDefine.SFSCommand.PVP_LOBBY_STATS) {
                int online = data.GetInt("online");
                int active = data.GetInt("active_matches");
                playersOnlineText.text = $"Players Online: {online}";
                activeMatchesText.text = $"Active Matches: {active}";
            }
        }

        private class CustomExtCmd : IExtCmd<ISFSObject> {
            public string Cmd { get; }
            public ISFSObject Data { get; }
            public bool EnableLog => false;

            public CustomExtCmd(string cmd, ISFSObject data) {
                Cmd = cmd;
                Data = data;
            }

            public ISFSObject ExportData() => Data;
        }
    }
}
