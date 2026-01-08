using System;
using System.Threading.Tasks;

using App;

using BomberLand.Component;
using BomberLand.Inventory;

using Data;

using Engine.Entities;

using Game.Dialog;
using Game.UI.Animation;

using Services;

using UnityEngine;

namespace BomberLand.Tutorial {
    public class TutorialSelectEquipments : MonoBehaviour {
        [SerializeField]
        private BLEquipment equipment;

        [SerializeField]
        private BLHeroInfomation hero;

        [SerializeField]
        private ButtonZoomAndFlash btFindMatch;

        [SerializeField]
        private BLTutorialGui tutorialGui;

        public ButtonZoomAndFlash BtFindMatch => btFindMatch;

        public BLEquipment Equipment => equipment;

        private PlayerData _playerData;
        private ISkinManager.Skin[] _skinList;

        private Action _onFindMatchCallback;

        public void Initialized(Canvas canvas, PlayerData playerData, ISkinManager.Skin[] skinList,
            Action findMatchCallback) {
            _onFindMatchCallback = findMatchCallback;
            _playerData = playerData;
            _skinList = skinList;
            var productItem = new ProductItemData {
                Name = _playerData.playerType.ToString(), ItemKind = ProductItemKind.Normal,
            };
            hero.UpdateHero(_playerData, productItem);
            equipment.InitializeForTutorial(canvas, _skinList, null, null, OnWingEquipped);
        }

        private void OnWingEquipped(int itemId) {
            hero.ShowWing(itemId);
        }

        public void OnFindMatchClicked() {
            _onFindMatchCallback?.Invoke();
        }

        public Task<bool> FindMatchAsync(ISoundManager soundManager) {
            var task = new TaskCompletionSource<bool>();
            _onFindMatchCallback = () => {
                soundManager.PlaySound(Audio.Tap);
                btFindMatch.Interactable = false;
                _onFindMatchCallback = null;
                task.SetResult(true);
            };
            return task.Task;
        }

        public Task<bool> WaitUiEquip(SkinChestType type, GameObject pointerHand, GameObject boxSystem, float rotation,
            Vector3 offset, ISoundManager soundManager) {
            return equipment.WaitUiEquip(type, pointerHand, boxSystem, rotation, offset, tutorialGui, _skinList,
                soundManager);
        }
    }
}