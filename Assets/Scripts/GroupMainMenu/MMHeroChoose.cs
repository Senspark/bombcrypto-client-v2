using System;

using Constant;

using Cysharp.Threading.Tasks;

using Game.Dialog;

using Scenes.MainMenuScene.Scripts.Controller;
using Scenes.PvpModeScene.Scripts;

using Services;

using UnityEngine;

namespace GroupMainMenu {
    public struct HeroChooseCallback {
        public Action<Dialog> OnShowDialog;
        public Action OnEquipClicked;
        public Action OpenAllChests;
    }

    public class MMHeroChoose : MonoBehaviour {
        public static string BomberRankKey => "bomber_rank";
        
        [SerializeField]
        private MMHero hero;

        [SerializeField]
        private MMRank rank;

        [SerializeField]
        private MMButton buttonEquip;

        public MMButton Button => buttonEquip;

        private HeroChooseController _controller;
        private HeroChooseCallback _heroChooseCallback;
        private PvpRankType _prevRankType;
        private Canvas _canvas;

        private void Awake() {
            buttonEquip.SetOnClickedCallback(OnEquipClicked);
            _prevRankType = (PvpRankType) PlayerPrefs.GetInt(BomberRankKey, 0);
        }

        public void SetCanvas(Canvas canvas) {
            _canvas = canvas;
        }

        public void InitHeroChoose() {
            _controller = new HeroChooseController();
            UniTask.Void(async () => {
                await _controller.Initialized();
                UpdateHero();
                UpdateRank();
                hero.gameObject.SetActive(true);
                if (_controller.GetDecayPointUser() != 0) {
                    ShowDialogRankDownDecay(_controller.GetRankType());
                } else {
                    ShowDialogRankChange(_controller.GetRankType());
                }
            });
        }

        private void UpdateHero() {
            UniTask.Void(async () => {
                var heroId = await _controller.GetHeroChoose();
                var skins = await _controller.GetSkins();
                
               var wingId = -1;
                var bombId = 0;
                foreach (var skin in skins) {
                    var itemType = (InventoryItemType) skin.ItemType;
                    if (!skin.Equipped) {
                        continue;
                    }
                    switch (itemType) {
                        case InventoryItemType.Avatar:
                            wingId = skin.SkinId;
                            break;
                        case InventoryItemType.BombSkin:
                            bombId = skin.SkinId;
                            break;
                        default:
                            continue;
                    }
                }
                hero.SetInfo(heroId, wingId, bombId);
            });
        }

        public void UpdateHero(int heroId) {
            hero.UpdateHero(heroId);
        }

        public void UpdateWing(int wingId) {
            hero.UpdateWing(wingId);
        }

        public void UpdateBomb(int bombId) {
            hero.UpdateBomb(bombId);
        }
        
        private void UpdateRank() {
            var from = _controller.GetRankType();
            var rankPoint = _controller.GetCurrentPoint();
            var points = _controller.GetStartEndPoint();
            rank.SetInfo(from, rankPoint, points.startPoint, points.endPoint);
        }

        private void ShowDialogRankChange(PvpRankType rankType) {
            // Nếu chưa có lưu vết thì ghi nhận rankType hiện thời.
            if (_prevRankType == 0) {
                _prevRankType = rankType;
                SaveBomberRank(rankType);
            }
            
            if (_prevRankType == rankType) {
                _heroChooseCallback.OpenAllChests?.Invoke();
                return;
            }
            if (_prevRankType < rankType) {
                ShowDialogRankUp(_prevRankType, rankType);
            } else {
                ShowDialogRankDown(_prevRankType, rankType);
            }
            _prevRankType = rankType;
            SaveBomberRank(rankType);
        }

        private void SaveBomberRank(PvpRankType rankType) {
            PlayerPrefs.SetInt(BomberRankKey, (int) rankType);
            PlayerPrefs.Save();
        }

        private void ShowDialogRankUp(PvpRankType from, PvpRankType to) {
            UniTask.Void(async () => {
                var dialog = await DialogPvpRankUp.Create();
                await dialog.SetInfo(from, to);
                dialog.ShowImmediately(_canvas);
                _heroChooseCallback.OnShowDialog?.Invoke(dialog);
            });
        }

        private void ShowDialogRankDown(PvpRankType from, PvpRankType to) {
            UniTask.Void(async () => {
                var dialog = await DialogPvpRankDown.Create();
                await dialog.SetInfo(from, to);
                dialog.ShowImmediately(_canvas);
                _heroChooseCallback.OnShowDialog?.Invoke(dialog);
            });
        }

        private void ShowDialogRankDownDecay(PvpRankType rankType) {
            var dialog = DialogPvpRankDown.Create().ContinueWith(dialog => {
                dialog.SetInfo(rankType);
                dialog.Show(_canvas);
            });
        }

        public void SetHeroChooseCallback(HeroChooseCallback callback) {
            _heroChooseCallback = callback;
        }
        
        private void OnEquipClicked() {
            _heroChooseCallback.OnEquipClicked?.Invoke();
        }
    }
}