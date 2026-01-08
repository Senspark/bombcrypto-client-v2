using System;
using System.Threading;

using App;

using Cysharp.Threading.Tasks;

using Senspark;

using Engine.Utils;

using Game.Dialog;

using PvpMode.Dialogs;
using PvpMode.Services;

using Scenes.MainMenuScene.Scripts;
using Scenes.PvpModeScene.Scripts;

using Share.Scripts.Dialog;

using UnityEngine;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.UI;

namespace Utils {
    [RequireComponent(typeof(Button))]
    public class LeaderboardButton : MonoBehaviour {
        [SerializeField]
        private Canvas canvas;

        [SerializeField]
        private AnimationZoom notification;

        private ISoundManager _soundManager;
        
        private Button _button;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isClaim;
        private IPvpCurrentRewardResult _pvpRankingResult;

        private void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnButtonClicked);
            LoadData();
        }

        private void LoadData() {
            _cancellationTokenSource = new CancellationTokenSource();
            var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            notification.gameObject.SetActive(false);
            _button.interactable = false;
            UniTask.Void(async (token) => {
                try {
                    var result = await serverManager.Pvp.GetPvpRanking();
                    _isClaim = result.CurrentSeason > 1 && !result.CurrentReward.IsClaim &&
                               result.CurrentReward.Rank > 0;
                    _pvpRankingResult = result.CurrentReward;
                    _button.interactable = true;
                    if (_isClaim) {
                        notification.gameObject.SetActive(true);
                        notification.Play();
                    }
                } catch (Exception e) {
                    DialogError.ShowError(canvas, e.Message);
                }
            }, _cancellationTokenSource.Token);
        }

        private void OnButtonClicked() {
            _soundManager.PlaySound(Audio.Tap);
            const string gem = "gem";
            if (_isClaim) {
                BLDialogPvPReward.Create().ContinueWith(reward => {
                    reward.Initialize(
                        _pvpRankingResult.Reward.TryGetValue(gem, out var value)
                            ? value
                            : 0,
                        _pvpRankingResult.Rank
                    );
                    reward.Claimed += () => {
                        _isClaim = false;
                        notification.gameObject.SetActive(false);
                    };
                    reward.Show(canvas);
                });
            } else {
                if (ScreenUtils.IsIPadScreen()) {
                    DialogLeaderboardPad.Create().ContinueWith((dialog) => { dialog.Show(canvas); });
                } else {
                    DialogLeaderboard.Create().ContinueWith((dialog) => { dialog.Show(canvas); });
                }
            }
        }

        private void OnDestroy() {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}