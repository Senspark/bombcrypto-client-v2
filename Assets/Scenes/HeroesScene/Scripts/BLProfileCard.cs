using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Engine.Utils;
using Senspark;
using Game.Dialog.BomberLand.BLGacha;
using Game.UI;
using PvpMode.Services;
using PvpMode.Utils;
using Services;
using Services.Server.Exceptions;
using Share.Scripts.Dialog;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Game.Dialog {
    public class BLProfileCard : MonoBehaviour {
        [SerializeField]
        private Text nickname;

        [SerializeField]
        private Text rank;

        [SerializeField]
        private Image rankIcon;

        [SerializeField]
        private Text rankPoint;

        [SerializeField]
        private Text latencyText;

        [SerializeField]
        private Color[] latencyColors;
        
        [SerializeField]
        private BLGachaRes resource;
        
        [SerializeField]
        private ImageAnimation avatarTR;

        [SerializeField]
        private Button avatarButton;
        
        private ILogManager _logManager;
        private IPvPBombRankManager _rankManager;
        private Action _onAvatarBtnClicked;
        private bool _enable = true;

        private bool _initServices;

        public void TryLoadData() {
            var profileCardInitializer = GetComponent<ProfileCardInitializer>();
            UniTask.Void(async () => {
                await profileCardInitializer.Initialized();
                profileCardInitializer.TryLoadData();    
            });
            
        }
        
        public static string GetRankName(PvpRankType rankType) {
            var configManager = ServiceLocator.Instance.Resolve<IEarlyConfigManager>();
            var ranks = configManager.Ranks;
            var rank = Array.Find(ranks, it => it.BombRank == (int) rankType);
            return rank != null ? rank.Name : ranks[0].Name;
        }

        public async UniTask<Sprite> GetSprite(PvpRankType pvpRankType) {
            return await resource.GetSpriteByPvpRank(pvpRankType);
        }

        public async Task InitializeAsync(Canvas canvas) {
            try {
                InitServices();
                _logManager.Log();
                await _rankManager.InitializeAsync();
                UpdateUI();
            } catch (Exception e) {
                if (e is ErrorCodeException) {
                    DialogError.ShowError(canvas, e.Message);    
                } else {
                    DialogOK.ShowError(canvas, e.Message);
                }
            }
        }

        private void InitServices() {
            if (_initServices) {
                return;
            }
            _initServices = true;
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            _rankManager = ServiceLocator.Instance.Resolve<IPvPBombRankManager>();
        }
        
        public void UpdateLatency(int lag) {
            latencyText.text = $"{lag}ms";
            latencyText.color = lag switch {
                <= 20 => Color.Lerp(latencyColors[0], latencyColors[1], lag / 20f),
                <= 100 => Color.Lerp(latencyColors[1], latencyColors[2], (lag - 20) / 100f),
                <= 150 => Color.Lerp(latencyColors[2], latencyColors[3], (lag - 100) / 150f),
                _ => latencyColors[3]
            };
        }
        
        public async void UpdateUI() {
            InitServices();
            _logManager.Log();
            var bombRank = _rankManager.GetBombRank();
            nickname.text = $"{Ellipsis.EllipsisAddress(UserNameDisplay.GetStringToDisplay())}";
            rank.text = GetRankName(bombRank);
            rankIcon.sprite = await resource.GetSpriteByPvpRank(bombRank);
            if (rankPoint) {
                rankPoint.text = $"{_rankManager.GetCurrentPoint()}";
            }

            if (avatarTR) {
                var avatarTRManager = ServiceLocator.Instance.Resolve<IAvatarTRManager>();
                var sprites = await resource.GetAvatar(avatarTRManager.GetCurrentAvatarId());
                avatarTR.StartAni(sprites);
            }
            if (avatarButton != null) {
                avatarButton.gameObject.SetActive(true);
            }
        }

        public async void UpdateUIFromRankItem(IPvpRankingItemResult rankItem) {
            InitServices();
            var userName = rankItem.Name ?? rankItem.UserName;
            nickname.text = $"{Ellipsis.EllipsisAddress(userName)}";
            var rankType = _rankManager.GetBombRank(rankItem.BombRank);
            rank.text = GetRankName(rankType);
            rankIcon.sprite = await resource.GetSpriteByPvpRank(rankType);
            if (rankPoint) {
                rankPoint.text = $"{rankItem.Point}";
            }
            if (avatarTR) {
                var sprites = await resource.GetAvatar(rankItem.Avatar);
                avatarTR.StartAni(sprites);
            }
            if (avatarButton != null) {
                avatarButton.gameObject.SetActive(false);
            }
        }
        
        public void SetEnable(bool value) {
            _enable = value;
        }
        
        public void SetBtnClicked(Action onAvatarBtnClicked) {
            _onAvatarBtnClicked = onAvatarBtnClicked;
        }

        public void OnBtnClicked() {
            if (!_enable) {
                return;
            }
            _onAvatarBtnClicked?.Invoke();
        }
    }
}