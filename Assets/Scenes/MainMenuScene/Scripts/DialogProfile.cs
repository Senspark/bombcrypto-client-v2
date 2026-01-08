using System.Collections.Generic;
using App;
using BomberLand.Inventory;
using Cysharp.Threading.Tasks;
using Data;
using Game.Dialog;
using PvpMode.Services;
using PvpMode.UI;
using Scenes.MainMenuScene.Scripts.Controller;
using Senspark;
using Share.Scripts.PrefabsManager;
using UnityEngine;

namespace Scenes.MainMenuScene.Scripts {
    public class DialogProfile : Dialog {
        [SerializeField]
        private PvpRanking pvpRanking;

        [SerializeField]
        private BLEquipment equipment;

        [SerializeField]
        private BLHeroInfomation hero;

        [SerializeField]
        private BLProfileCard profileCard;
        
        private HeroSelectorController _controller;

        private Dictionary<int, StatData[]> _equipSkinStats;

        private System.Action _onEditAvatar;
        
        public static UniTask<DialogProfile> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogProfile>();
        }

        protected override void Awake() {
            base.Awake();
            _controller = new HeroSelectorController();
        }

        public void InitCurrentHero(System.Action callback) {
            if (profileCard != null) {
                profileCard.UpdateUI();
            }
            UniTask.Void(async () => {
                _onEditAvatar = callback;
                await _controller.Initialized();
                await equipment.InitializeAsync(DialogCanvas, OnReloadEquip, null, OnWingEquipped);
                UpdateHeroChoose(_controller.GetCurrentPvpHero());
                UpdateRank(_controller.CurrentRank);
            });
        }

        public void InitOtherHero(int userId, string userName, IPvpRankingItemResult rankItem) {
            if (profileCard != null) {
                profileCard.UpdateUIFromRankItem(rankItem);
            }
            UniTask.Void(async () => {
                await _controller.Initialized();
                var result = await _controller.GetOtherUserInfo(userId, userName);
                await equipment.InitializeWithData(result.EquipData, OnReloadEquip, OnWingEquipped);
                UpdateHeroChoose(result.Hero);
                UpdateRank(result.Rank);
            });
        }

        private void UpdateRank(IPvpRankingItemResult rank) {
            pvpRanking.SetCurrentInfo(rank);
        }
        
        private void UpdateHeroChoose(PlayerData pvpHero) {
            hero.UpdateHero(pvpHero);
            hero.UpdateStatsFromSkinStats(_equipSkinStats);
        }

        private void OnReloadEquip(Dictionary<int, StatData[]> skinStats) {
            _equipSkinStats = skinStats;
            hero.UpdateStatsFromSkinStats(_equipSkinStats);
        }

        private void OnWingEquipped(int wingId) {
            hero.ShowWing(wingId);
        }
        
        public void OnButtonAvatarClicked() {
           _onEditAvatar?.Invoke();
        }

        protected override void OnYesClick() {
            // Do nothing
        }
    }
}
