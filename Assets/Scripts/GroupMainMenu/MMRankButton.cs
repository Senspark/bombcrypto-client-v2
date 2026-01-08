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
using UnityEngine.UI;

namespace GroupMainMenu {
    public class MMRankButton : MonoBehaviour {
        [SerializeField]
        private Button button;

        [SerializeField]
        private AnimationZoom notification;

        private CancellationTokenSource _cancellationTokenSource;
        private bool _isClaim;
        private IPvpCurrentRewardResult _pvpRankingResult;

        private Canvas _canvasDialog;

        public void SetCanvasDialog(Canvas canvas) {
            _canvasDialog = canvas;
        }

        public void LoadData() {
            _cancellationTokenSource = new CancellationTokenSource();
            var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            notification.gameObject.SetActive(false);
            button.interactable = false;
            UniTask.Void(async (token) => {
                try {
                    var result = await serverManager.Pvp.GetPvpRanking();
                    _isClaim = result.CurrentSeason > 1 && !result.CurrentReward.IsClaim &&
                               result.CurrentReward.Rank > 0;
                    _pvpRankingResult = result.CurrentReward;
                    button.interactable = true;
                    if (_isClaim) {
                        notification.gameObject.SetActive(true);
                        notification.Play();
                    }
                } catch (Exception e) {
                    DialogError.ShowError(_canvasDialog, e.Message);
                }
            }, _cancellationTokenSource.Token);
        }

        public void ShowPvpRanking() {
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
                    };;
                    reward.Show(_canvasDialog);
                });
            } else {
                if (ScreenUtils.IsIPadScreen()) {
                    DialogLeaderboardPad.Create().ContinueWith((dialog) => { dialog.Show(_canvasDialog); });
                } else {
                    DialogLeaderboard.Create().ContinueWith((dialog) => { dialog.Show(_canvasDialog); });
                }
            }
        }
    }
}