using App;

using BLPvpMode.UI;

using BomberLand.Component;
using BomberLand.Tutorial;

using Scenes.PvpModeScene.Scripts;

using Senspark;

using UnityEngine;

namespace BLPvpMode.Test {
    public class TestTutorial : MonoBehaviour {
        [SerializeField]
        private Canvas canvas;

        [SerializeField]
        private BLGuiPvp guiPvp;

        [SerializeField]
        private BLTutorialGui resource;

        private TutorialNewEquipments _newEquipments;
        private TutorialSelectEquipments _selectEquipments;
        private TutorialFullRankBooster _fullRankBooster;
        private TutorialSelectJoystick _selectJoystick;
        private TutorialActivatedBooster _activatedBooster;
        private TutorialPopupResult _popupResult;
        private TutorialCompleteReward _completeReward;
        private TutorialPvpMenu _pvpMenu;
        private TutorialLeaderboard _leaderboard;

        [Button]
        public void ShowHideElementGui(ElementGui element, bool value) {
            guiPvp.SetVisible(element, value);
        }

        [Button]
        public void ShowNewEquipments(int itemId, string footer) {
            _newEquipments = resource.CreateNewEquipmentPopup(canvas.transform, "", itemId, footer, OnClaimClicked);
        }

        [Button]
        public void ShowNew4Equipments() {
            _newEquipments = resource.CreateNew4EquipmentPopup(canvas.transform, OnClaimClicked);
        }

        [Button]
        public void ShowSelectEquipments() {
            _selectEquipments = resource.CreateSelectEquipmentPopup(canvas, canvas.transform, OnFindMatchClicked);
        }

        [Button]
        public void ShowFullSelectedEquipments() {
            _selectEquipments = resource.CreateFullSelectedEquipmentPopup(canvas, canvas.transform, OnFindMatchClicked);
        }

        [Button]
        public void ShowFullRankConquest() {
            _fullRankBooster = resource.CreateFullRankConquest(canvas.transform, OnBoosterClaimClicked);
        }

        [Button]
        public void ShowFullRankGuardian() {
            _fullRankBooster = resource.CreateFullRankGuardian(canvas.transform, OnBoosterClaimClicked);
        }

        [Button]
        public void ShowSelectJoystick() {
            _selectJoystick = resource.CreateSelectJoystick(canvas.transform, OnChooseJoystickClicked);
        }

        [Button]
        public void ShowActivatedConquest() {
            _activatedBooster = resource.CreateActivatedConquest(canvas.transform, OnOkButtonClicked);
        }

        [Button]
        public void ShowActivatedGuardian() {
            _activatedBooster = resource.CreateActivatedGuardian(canvas.transform, OnOkButtonClicked);
        }

        [Button]
        public void ShowPopupWin() {
            _popupResult = resource.CreatePopupWin(canvas.transform, OnNextButtonClicked);
        }

        [Button]
        public void ShowPopupLose() {
            _popupResult = resource.CreatePopupLose(canvas.transform, OnNextButtonClicked);
        }

        [Button]
        public void ShowPopupCompletedRewards() {
            _completeReward = resource.CreateCompleteReward(canvas.transform, OnRewardClaimClicked);
        }

        [Button]
        public void ShowPvpMenu() {
            _pvpMenu = resource.CreatePvpMenu(canvas, canvas.transform);
        }

        [Button]
        public void HideElementInPvpMenu(int index, bool value) {
            if (_pvpMenu) {
                _pvpMenu.SetVisible(index, value);
            }
        }

        [Button]
        public void ShowLeaderboard() {
            _leaderboard = resource.CreateLeaderboard(canvas.transform);
        }

        private void OnClaimClicked() {
            Destroy(_newEquipments.gameObject);
        }

        private void OnFindMatchClicked() {
            Destroy(_selectEquipments.gameObject);
        }

        private void OnBoosterClaimClicked() {
            Destroy(_fullRankBooster.gameObject);
        }

        private void OnChooseJoystickClicked(int type) {
            var storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            storageManager.IsJoyStickChoice = (type == 1);
            Destroy(_selectJoystick.gameObject);
        }

        private void OnOkButtonClicked() {
            Destroy(_activatedBooster.gameObject);
        }

        private void OnNextButtonClicked() {
            Destroy(_popupResult.gameObject);
        }

        private void OnRewardClaimClicked() {
            Destroy(_completeReward.gameObject);
        }
    }
}