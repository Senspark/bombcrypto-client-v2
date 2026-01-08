using System;
using System.Threading.Tasks;

using App;

using BomberLand.Tutorial;

using Constant;

using Data;

using DG.Tweening;

using Engine.Entities;

using Services;

using UnityEngine;

namespace BomberLand.Component {
    [CreateAssetMenu(fileName = "TutorialGui", menuName = "BomberLand/TutorialGui")]
    public class BLTutorialGui : ScriptableObject {
        public enum ETutorialGui {
            Hover = 10,
            InstructionsBox = 20,
            InstructionsTap = 30,
            HandTouchOnMap = 40,
            HandTouchOnUi = 50,
            DimInGui = 60
        }

        public enum TutorialPopup {
            NewEquipments,
            SelectEquipments,
            FullRankConquest,
            FullRankGuardian,
            SelectJoystick,
            ActivatedConquest,
            ActivatedGuardian,
            PopupWin,
            PopupLose,
            CompletedReward,
            PvpMenu,
            Leaderboard,
            PopupQuit,
            PvpMenuPad,
            LeaderboardPad,
        }

        [Serializable]
        public class ResourcePicker {
            public GameObject obj;
        }

        public SerializableDictionaryEnumKey<ETutorialGui, ResourcePicker> resource;
        public SerializableDictionaryEnumKey<TutorialPopup, ResourcePicker> popupResource;

        public BLDimHighlight CreateHover(Transform tfParent) {
            var hoverPrefab = resource[ETutorialGui.Hover].obj;
            return Instantiate(hoverPrefab, tfParent, false).GetComponent<BLDimHighlight>();
        }

        public BLInstructionsBox CreateInstructionsBox(Transform tfParent) {
            var instructionsBoxPrefab = resource[ETutorialGui.InstructionsBox].obj;
            return Instantiate(instructionsBoxPrefab, tfParent, false).GetComponent<BLInstructionsBox>();
        }

        public BLInstructionsTap CreateInstructionsTap(Transform tfParent) {
            var prefab = resource[ETutorialGui.InstructionsTap].obj;
            return Instantiate(prefab, tfParent, false).GetComponent<BLInstructionsTap>();
        }

        public GameObject CreateHandTouchOnMap(Transform tfParent) {
            var prefab = resource[ETutorialGui.HandTouchOnMap].obj;
            return Instantiate(prefab, tfParent, false);
        }

        public GameObject CreateHandTouchOnUi(Transform tfParent) {
            var prefab = resource[ETutorialGui.HandTouchOnUi].obj;
            return Instantiate(prefab, tfParent, false);
        }

        public BLDimInGui CreateDimInGui(Transform tfParent, Vector3 pos) {
            var prefab = resource[ETutorialGui.DimInGui].obj;
            var obj = Instantiate(prefab, tfParent, false);
            obj.transform.position = pos;
            return obj.GetComponent<BLDimInGui>();
        }

        public Task<bool> CreateAndWaitBtFakeClick(Transform btPvp, Transform parent,
            Action<UnityEngine.UI.Button> onCreateBt) {
            var task = new TaskCompletionSource<bool>();
            var obj = Instantiate(btPvp, parent, true);
            var btFake = obj.GetComponent<UnityEngine.UI.Button>();
            if (!btFake) {
                btFake = obj.GetComponentInChildren<UnityEngine.UI.Button>();
            }

            var canvasGroup = obj.GetComponent<CanvasGroup>();
            if (!canvasGroup) {
                canvasGroup = obj.gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0;
            canvasGroup.DOFade(1.0f, 0.3f);
            btFake.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            btFake.onClick.AddListener(() => {
                btFake.interactable = false;
                task.SetResult(true);
            });
            onCreateBt?.Invoke(btFake);
            return task.Task;
        }

        #region Tutorial Popup

        public TutorialSelectJoystick CreateSelectJoystick(Transform tfParent, Action<int> chooseCallback) {
            var prefab = popupResource[TutorialPopup.SelectJoystick].obj;
            var obj = Instantiate(prefab, tfParent);
            var selectJoystick = obj.GetComponent<TutorialSelectJoystick>();
            selectJoystick.Initialized(chooseCallback);
            return selectJoystick;
        }

        public TutorialNewEquipments CreateNewEquipmentPopup(Transform tfParent,
            string title,
            int itemId,
            string footer,
            System.Action claimCallback) {
            var equipments = new TutorialEquipment[] { new TutorialEquipment(title, itemId, footer), };

            var prefab = popupResource[TutorialPopup.NewEquipments].obj;
            var obj = Instantiate(prefab, tfParent);
            var newEquipments = obj.GetComponent<TutorialNewEquipments>();
            newEquipments.Initialized(equipments, claimCallback);
            return newEquipments;
        }

        public TutorialNewEquipments CreateNew4EquipmentPopup(Transform tfParent,
            System.Action claimCallback) {
            var equipments = new TutorialEquipment[] {
                new TutorialEquipment("NEW BOMB", 49, "1D"), new TutorialEquipment("NEW WING", 64, "1D"),
                new TutorialEquipment("NEW FIRE", 76, "1D"), new TutorialEquipment("NEW TRAIL", 125, "1D"),
            };

            var prefab = popupResource[TutorialPopup.NewEquipments].obj;
            var obj = Instantiate(prefab, tfParent);
            var newEquipments = obj.GetComponent<TutorialNewEquipments>();
            newEquipments.Initialized(equipments, claimCallback);
            return newEquipments;
        }

        public TutorialSelectEquipments CreateSelectEquipmentPopup(Canvas canvas, Transform tfParent,
            Action findMatchCallback,
            bool equipped = false) {
            var playerData = new PlayerData() {
                itemId = (int) GachaChestProductId.Ninja,
                playerType = PlayerType.Ninja,
                playercolor = PlayerColor.HeroTr,
            };

            var skinList = new[] {
                CreateSkin((int) InventoryItemType.BombSkin, 49, "Monitor", equipped,
                    new[] { new StatData((int) StatId.Damage, 0, 0, 1) }),
                CreateSkin((int) InventoryItemType.Avatar, 64, "Asura", equipped,
                    new[] { new StatData((int) StatId.Range, 0, 0, 1) }),
                CreateSkin((int) InventoryItemType.Fire, 76, "Wukong", equipped,
                    new[] { new StatData((int) StatId.Damage, 0, 0, 1) }),
                CreateSkin((int) InventoryItemType.Trail, 125, "Happy Color", equipped,
                    new[] { new StatData((int) StatId.Speed, 0, 0, 1) }),
            };

            var prefab = popupResource[TutorialPopup.SelectEquipments].obj;
            var obj = Instantiate(prefab, tfParent);
            var selectEquipment = obj.GetComponent<TutorialSelectEquipments>();
            selectEquipment.Initialized(canvas, playerData, skinList, findMatchCallback);
            return selectEquipment;
        }

        public TutorialSelectEquipments CreateFullSelectedEquipmentPopup(Canvas canvas, Transform tfParent,
            Action findMatchCallback
        ) {
            return CreateSelectEquipmentPopup(canvas, tfParent, findMatchCallback, true);
        }

        public TutorialFullRankBooster CreateFullRankConquest(Transform tfParent, Action claimCallback) {
            var prefab = popupResource[TutorialPopup.FullRankConquest].obj;
            var obj = Instantiate(prefab, tfParent);
            var selectEquipment = obj.GetComponent<TutorialFullRankBooster>();
            selectEquipment.Initialized(claimCallback);
            return selectEquipment;
        }

        public TutorialFullRankBooster CreateFullRankGuardian(Transform tfParent, Action claimCallback) {
            var prefab = popupResource[TutorialPopup.FullRankGuardian].obj;
            var obj = Instantiate(prefab, tfParent);
            var selectEquipment = obj.GetComponent<TutorialFullRankBooster>();
            selectEquipment.Initialized(claimCallback);
            return selectEquipment;
        }

        public TutorialActivatedBooster CreateActivatedConquest(Transform tfParent, Action okCallback) {
            var prefab = popupResource[TutorialPopup.ActivatedConquest].obj;
            var obj = Instantiate(prefab, tfParent);
            var activatedBooster = obj.GetComponent<TutorialActivatedBooster>();
            activatedBooster.Initialized(okCallback);
            return activatedBooster;
        }

        public TutorialActivatedBooster CreateActivatedGuardian(Transform tfParent, Action okCallback) {
            var prefab = popupResource[TutorialPopup.ActivatedGuardian].obj;
            var obj = Instantiate(prefab, tfParent);
            var activatedBooster = obj.GetComponent<TutorialActivatedBooster>();
            activatedBooster.Initialized(okCallback);
            return activatedBooster;
        }

        public TutorialPopupResult CreatePopupWin(Transform tfParent, Action nextCallback) {
            var prefab = popupResource[TutorialPopup.PopupWin].obj;
            var obj = Instantiate(prefab, tfParent);
            var popupResult = obj.GetComponent<TutorialPopupResult>();
            popupResult.Initialized(nextCallback);
            return popupResult;
        }

        public TutorialPopupResult CreatePopupLose(Transform tfParent, Action nextCallback) {
            var prefab = popupResource[TutorialPopup.PopupLose].obj;
            var obj = Instantiate(prefab, tfParent);
            var popupResult = obj.GetComponent<TutorialPopupResult>();
            popupResult.Initialized(nextCallback);
            return popupResult;
        }

        public TutorialCompleteReward CreateCompleteReward(Transform tfParent,
            System.Action claimCallback) {
            var rewards = new TutorialReward[] {
                new TutorialReward(9, "Knight"), new TutorialReward(12, "Witch"), new TutorialReward(0, "100"),
            };

            var prefab = popupResource[TutorialPopup.CompletedReward].obj;
            var obj = Instantiate(prefab, tfParent);
            var completeReward = obj.GetComponent<TutorialCompleteReward>();
            completeReward.Initialized(rewards, claimCallback);
            return completeReward;
        }

        public TutorialPvpMenu CreatePvpMenu(Canvas canvas, Transform tfParent) {
            var prefab = ScreenUtils.IsIPadScreen() ? popupResource[TutorialPopup.PvpMenuPad].obj : popupResource[TutorialPopup.PvpMenu].obj;
            var obj = Instantiate(prefab, tfParent);
            var pvpMenu = obj.GetComponent<TutorialPvpMenu>();
            pvpMenu.Initialized(canvas);
            return pvpMenu;
        }

        public TutorialLeaderboard CreateLeaderboard(Transform tfParent) {
            var prefab = ScreenUtils.IsIPadScreen() ? popupResource[TutorialPopup.LeaderboardPad].obj : popupResource[TutorialPopup.Leaderboard].obj;
            var obj = Instantiate(prefab, tfParent);
            var leaderboard = obj.GetComponent<TutorialLeaderboard>();
            leaderboard.Initialized();
            return leaderboard;
        }

        public TutorialPopupQuit CreatePopupQuit(Transform tfParent) {
            var prefab = popupResource[TutorialPopup.PopupQuit].obj;
            var obj = Instantiate(prefab, tfParent);
            var leaderboard = obj.GetComponent<TutorialPopupQuit>();
            return leaderboard;
        }

        public static ISkinManager.Skin CreateSkin(int itemType, int itemId, string name, bool equipped = false,
            StatData[] stats = null) {
            return new ISkinManager.Skin( //
                Array.Empty<int>(),
                false,
                24 * 60 * 60 * 1000, // 1 day
                equipped,
                DateTime.Now,
                1,
                itemId,
                name,
                false,
                itemType,
                stats ?? Array.Empty<StatData>());
        }

        #endregion
    }
}