// #define Test_Step

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Analytics;

using App;

using BLPvpMode.Engine.Entity;
using BLPvpMode.UI;

using BomberLand.Component;
using BomberLand.Tutorial;

using Constant;

using Cysharp.Threading.Tasks;

using DG.Tweening;

using Engine.Entities;

using Exceptions;

using Game.Dialog;
using Game.Dialog.Connects;

using PvpMode.Entities;
using PvpMode.Manager;

using Scenes.ConnectScene.Scripts;
using Scenes.ConnectScene.Scripts.Connectors;
using Scenes.StoryModeScene.Scripts;

using Senspark;

using Services;

using Share.Scripts.Dialog;
using Share.Scripts.Utils;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Scenes.TutorialScene.Scripts {
    public class BLGuiPvpTutorialV2 : MonoBehaviour {
        [SerializeField]
        private Canvas canvasDialog;

        [SerializeField]
        private Canvas canvasDialogTop;

        [SerializeField]
        private BLTutorialGui tutorialGui;

        [SerializeField]
        private BLLevelScenePvpTutorial levelScenePvpTutorial;

        [SerializeField]
        private BLBlinkDpad blinkDpad;

        [SerializeField]
        private Camera camera;

        [SerializeField]
        private GameObject dialogWaiting;

        [SerializeField]
        private Text waitingText;

        [SerializeField]
        private Image splashFade;

        private BLInstructionsTap _instructionsTap;

        private IAnalytics _analytics;

        private ISoundManager _soundManager;
        
        private IStorageManager _storageManager;

        private void Awake() {
            dialogWaiting.SetActive(false);
            splashFade.gameObject.SetActive(true);
        }

        protected void Start() {
            ShowTutorialPvp();
        }

        private void ShowTutorialPvp() {
            if (!_instructionsTap) {
                _instructionsTap = tutorialGui.CreateInstructionsTap(canvasDialog.transform);
                _instructionsTap.transform.SetSiblingIndex(_instructionsTap.transform.parent.childCount - 2);
            }
            _instructionsTap.HideWelcome();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _analytics.TrackScene(SceneType.ControlGuideTutorial);
            _soundManager.PlayMusic(Audio.PvpMusic);

#if Test_Step
            TestStep();
            return;
#endif

            UniTask.Void(async () => {
                try {
                    splashFade.DOFade(0.0f, 1.5f).OnComplete(() => { splashFade.gameObject.SetActive(false); });
                    await Main();
                } catch (Exception ex) {
                    if (ex is not CancelProcessException le) {
                        throw;
                    }
                    // Do nothing
#if UNITY_EDITOR
                    Debug.Log("Cancel Tutorial success");
#endif
                }
            });
        }

        private async Task Main() {
            // GUIDE_1
            _instructionsTap.HideDimBackground();
            var pItemBombUp = new Vector2Int(2, 3);
            var pItemFireUp = new Vector2Int(0, 4);
            var pItemSpeedUp = new Vector2Int(0, 1);
            await Process(levelScenePvpTutorial.WaitStartGame(() => {
                // setup brick
                levelScenePvpTutorial.HideTimer();
                // levelScenePvpTutorial.GenRanBrick();
                levelScenePvpTutorial.AddItemUnderBrick(pItemBombUp.x, pItemBombUp.y, ItemType.BombUp);
                levelScenePvpTutorial.AddItemUnderBrick(pItemFireUp.x, pItemFireUp.y, ItemType.FireUp);
                levelScenePvpTutorial.AddItemUnderBrick(pItemSpeedUp.x, pItemSpeedUp.y, ItemType.Boots);
                levelScenePvpTutorial.SetEnemyMaxSpeedBot(1);
                levelScenePvpTutorial.HideAllElementGui();
                var dialogQuit = tutorialGui.CreatePopupQuit(canvasDialogTop.transform);
                dialogQuit.transform.SetSiblingIndex(99);
                dialogQuit.Hide();
                dialogQuit.SetOnConfirmCallback(() => {
                    CancelProcess();
                    _analytics.TrackScene(SceneType.SkipTutorialBot);
                    Finish(true);
                    dialogQuit.Hide();
                });
                levelScenePvpTutorial.LevelScene.SetOnRequestQuitPvp(() => { dialogQuit.Show(); });
                dialogWaiting.SetActive(false);
            }));
            blinkDpad.Initialized(levelScenePvpTutorial.PlayerInput);
            levelScenePvpTutorial.SetEnableInputMove(false);
            levelScenePvpTutorial.SetEnableInputPlantBomb(false);
            await Step1(pItemBombUp, pItemFireUp, pItemSpeedUp);
             await Step2();
             await Step3();
             await Step4();
             await Step5();
            Finish(false);
        }

        #region Util

        private async Task WaitHeroMoveTo(Vector2Int pos) {
            await levelScenePvpTutorial.WaitHeroMoveTo(0, pos.x, pos.y);
        }

        private GameObject CreatPointerInGame(int tileX, int tileY, bool isShowIndicator = true) {
            var pointerHandInMap = tutorialGui.CreateHandTouchOnMap(levelScenePvpTutorial.StartLocation.transform);
            var pos = levelScenePvpTutorial.GetTilePosition(tileX, tileY);
            pointerHandInMap.transform.localPosition = new Vector3(pos.x, pos.y, 0);
            if (!isShowIndicator) {
                pointerHandInMap.transform.Find("Indicator").gameObject.SetActive(false);
            }
            return pointerHandInMap;
        }

        private Vector2 ConvertPosCamToCanvas(Vector3 pWorld) {
            var canvasSize = canvasDialog.GetComponent<RectTransform>().sizeDelta;
            Vector2 p = camera.WorldToViewportPoint(pWorld);
            var p2 = new Vector2(
                ((p.x * canvasSize.x) - (canvasSize.x * 0.5f)),
                ((p.y * canvasSize.y) - (canvasSize.y * 0.5f)));
            return p2;
        }

        private void GenRanBrick() {
            var pItemBombUp = new Vector2Int(2, 3);
            var pItemFireUp = new Vector2Int(0, 4);
            var pItemSpeedUp = new Vector2Int(0, 1);
            levelScenePvpTutorial.AddBrick(pItemBombUp.x, pItemBombUp.y);
            levelScenePvpTutorial.AddBrick(pItemFireUp.x, pItemFireUp.y);
            levelScenePvpTutorial.AddBrick(pItemSpeedUp.x, pItemSpeedUp.y);
            levelScenePvpTutorial.GenRanBrick();
        }

        #endregion

        #region Popup

        private const int OffsetDialog = 60;

        private TutorialNewEquipments ShowPopupCollectKey() {
            var popup = tutorialGui.CreateNewEquipmentPopup(canvasDialog.transform, "",
                (int) GachaChestProductId.Key, "x3", null);
            popup.transform.localPosition = new Vector3(0, OffsetDialog, 0);
            // popup.transform.SetAsFirstSibling();
            return popup;
        }

        private TutorialNewEquipments ShowPopupCollectShield() {
            var popup = tutorialGui.CreateNewEquipmentPopup(canvasDialog.transform, "",
                (int) GachaChestProductId.Shield, "x3", null);
            popup.transform.localPosition = new Vector3(0, OffsetDialog, 0);
            return popup;
        }

        private TutorialSelectJoystick ShowPopupSelectJoystick() {
            var popup = tutorialGui.CreateSelectJoystick(canvasDialog.transform, null);
            popup.transform.localPosition = new Vector3(0, OffsetDialog, 0);
            return popup;
        }

        private TutorialNewEquipments ShowPopupNew4Equipment() {
            var popup = tutorialGui.CreateNew4EquipmentPopup(canvasDialog.transform, null);
            var tf = popup.transform;
            tf.localPosition = new Vector3(0, OffsetDialog, 0);
            tf.SetSiblingIndex(tf.childCount - 1);
            return popup;
        }

        private TutorialSelectEquipments ShowEquipmentPopup() {
            var popup = tutorialGui.CreateSelectEquipmentPopup(canvasDialog, _instructionsTap.BgPvp.transform, null);
            var tf = popup.transform;
            tf.localPosition = new Vector3(0, OffsetDialog, 0);
            return popup;
        }

        private TutorialSelectEquipments ShowEquipmentFullPopup() {
            var popup = tutorialGui.CreateFullSelectedEquipmentPopup(canvasDialog, _instructionsTap.BgPvp.transform,
                null);
            var tf = popup.transform;
            tf.localPosition = new Vector3(0, OffsetDialog, 0);
            return popup;
        }

        private TutorialPopupResult ShowWinPopup(bool isShowNext = false) {
            var popup = tutorialGui.CreatePopupWin(_instructionsTap.transform, null);
            popup.transform.SetSiblingIndex(1);
            popup.BtNext.SetActive(isShowNext);
            _soundManager.PlaySound(Audio.PopupWin);
            return popup;
        }

        private TutorialPopupResult ShowLosePopup() {
            var popup = tutorialGui.CreatePopupLose(_instructionsTap.transform, null);
            popup.transform.SetSiblingIndex(1);
            _soundManager.PlaySound(Audio.PopupDefeated);
            return popup;
        }

        private TutorialLeaderboard ShowLeaderboardPopup() {
            var popup = tutorialGui.CreateLeaderboard(_instructionsTap.ShortDialog);
            popup.transform.SetSiblingIndex(2);
            return popup;
        }

        private TutorialPvpMenu ShowPvpMenuPopup() {
            var popup = tutorialGui.CreatePvpMenu(canvasDialog, _instructionsTap.transform);
            popup.transform.SetSiblingIndex(2);
            return popup;
        }

        private TutorialFullRankBooster ShowClaimFullRankConquestPopup() {
            var popup = tutorialGui.CreateFullRankConquest(_instructionsTap.ShortDialog, null);
            popup.transform.localPosition = new Vector3(0, OffsetDialog, 0);
            return popup;
        }

        private TutorialFullRankBooster ShowClaimFullRankGuardianPopup() {
            var popup = tutorialGui.CreateFullRankGuardian(_instructionsTap.ShortDialog, null);
            popup.transform.localPosition = new Vector3(0, OffsetDialog, 0);
            return popup;
        }

        private TutorialActivatedBooster ShowActivatedConquestPopup() {
            var popup = tutorialGui.CreateActivatedConquest(canvasDialog.transform, null);
            return popup;
        }

        private TutorialActivatedBooster ShowActivatedGuardianPopup() {
            var popup = tutorialGui.CreateActivatedGuardian(canvasDialog.transform, null);
            return popup;
        }

        private TutorialCompleteReward ShowCompleteRewardPopup() {
            var popup = tutorialGui.CreateCompleteReward(canvasDialogTop.transform, null);
            popup.transform.localPosition = new Vector3(0, OffsetDialog, 0);
            popup.transform.SetAsLastSibling();
            return popup;
        }

        #endregion

        #region Steps

        private void TestStep() {
            _instructionsTap.HideDimBackground();
            UniTask.Void(async () => {
                await Process(levelScenePvpTutorial.WaitStartGame(() => {
                    // setup brick
                    levelScenePvpTutorial.HideTimer();
                }));
                await Process(_instructionsTap.Show());
                _instructionsTap.InstructionsBox.SetPos(BLInstructionsBox.PosBox.Bot);
                levelScenePvpTutorial.KillEnemy();
                levelScenePvpTutorial.SetPlayerPos(0, new Vector2Int(6, 0));
                levelScenePvpTutorial.HideElementGui(ElementGui.HeroGroup);
                await Step2();
                await Step3();
                await Step4();
                await Step5();
                Finish(false);
            });
        }

        private async Task Step1(Vector2Int pItemBombUp, Vector2Int pItemFireUp, Vector2Int pItemSpeedUp) {
            await Process(_instructionsTap.Show());
            _instructionsTap.InstructionsBox.SetPos(BLInstructionsBox.PosBox.Bot);

            await Process(
                _instructionsTap.NpcGuide("Welcome to Bomb Crypto. I'm Witch. I will guide you through the game.",
                    "dialog_1"));

            await Process(_instructionsTap.NpcGuide("Now let's go for a walk together.", "dialog_2"));

            // Show popup select Joystick
            {
                var popup = ShowPopupSelectJoystick();
                await Process(_instructionsTap.NpcGuide("Please choose your controller.", "dialog_2_1", true));
                await Process(popup.CloseAsync(_soundManager));
                await Process(_instructionsTap.InstructionsBox.HideAsync());
            }

            {
                var dim = tutorialGui.CreateDimInGui(canvasDialog.transform,
                    levelScenePvpTutorial.PlayerInput.JoyPad.Background.transform.position);
                dim.transform.SetAsFirstSibling();
                dim.SetSize(400);
                _instructionsTap.InstructionsBox.SetPos(BLInstructionsBox.PosBox.BotRight);
                levelScenePvpTutorial.ShowElementGui(ElementGui.Joystick);
                await Process(_instructionsTap.NpcGuide(
                    "This is the Analog key, which helps you move the hero.", "dialog_3"));
                await Process(_instructionsTap.InstructionsBox.HideAsync());
                Destroy(dim.gameObject);
            }

            // Move to plant bomb
            {
                var pMoveToPlantBomb = new Vector2Int(2, 4);
                var pointerHand = CreatPointerInGame(pMoveToPlantBomb.x, pMoveToPlantBomb.y);
                levelScenePvpTutorial.SetEnableInputMove(true);
                // blinkDpad.StartBlink();
                _instructionsTap.SetSystemText("HOLD ARROW & MOVE HERO TO THIS POINT",
                    ConvertPosCamToCanvas(pointerHand.transform.position) + new Vector2(0, -80));
                await Process(WaitHeroMoveTo(pMoveToPlantBomb));
                _instructionsTap.HideSystemBox();
                levelScenePvpTutorial.SetEnableInputMove(false);
                Destroy(pointerHand);
            }

            await Process(_instructionsTap.InstructionsBox.HideAsync());

            // Plant Bomb
            {
                BombPvp bomb = null;
                if (Application.isMobilePlatform || Application.isEditor) {
                    _instructionsTap.SetPosPointerHand(levelScenePvpTutorial.ButtonBombTransform
                        .GetComponent<RectTransform>());
                    levelScenePvpTutorial.ShowElementGui(ElementGui.ButtonBomb);
                    _instructionsTap.SetSystemText("PLACE BOMB TO BREAK THE BLOCK");
                    var posBtBomb = levelScenePvpTutorial.ButtonBombTransform.position;
                    _instructionsTap.SetSystemBoxPos(new Vector2(posBtBomb.x - 150, posBtBomb.y + 300));

                    levelScenePvpTutorial.SetEnableInputPlantBomb(true);
                    bomb = await ProcessResult(levelScenePvpTutorial.WaitHeroPlanTheBomb());
                    _instructionsTap.HidePointerHand();
                    // _instructionsTap.HideInstruction();
                } else {
                    await Process(_instructionsTap.Show());
                    levelScenePvpTutorial.SetEnableInputPlantBomb(true);
                    _instructionsTap.SetSystemText("PLACE BOMB TO BREAK THE BLOCK", new Vector2(0, 0));
                    bomb = await ProcessResult(levelScenePvpTutorial.WaitHeroPlanTheBomb());
                }

                _instructionsTap.HideSystemBox();

                levelScenePvpTutorial.SetEnableInputMove(true);
                levelScenePvpTutorial.SetEnableInputPlantBomb(false);
                var pMoveOut = new Vector2Int(4, 5);
                var pointerHand = CreatPointerInGame(pMoveOut.x, pMoveOut.y);
                _instructionsTap.SetSystemText("RUN OUT OF THE EXPLOSION AREA",
                    ConvertPosCamToCanvas(pointerHand.transform.position) + new Vector2(0, -150));
                await Process(WaitHeroMoveTo(pMoveOut));
                // _instructionsTap.HideInstruction();
                _instructionsTap.HideSystemBox();
                levelScenePvpTutorial.SetEnableInputMove(false);
                Destroy(pointerHand);
                levelScenePvpTutorial.ExplodeBombTutorial(bomb);
                await Process(WebGLTaskDelay.Instance.Delay(1500));
            }
            _instructionsTap.InstructionsBox.SetPos(BLInstructionsBox.PosBox.Bot);
            await Process(_instructionsTap.NpcGuide("Destroyed blocks may drop items.", "dialog_4"));
            await Process(_instructionsTap.InstructionsBox.HideAsync());

            // Pickup bombUp
            {
                var pointerHand = CreatPointerInGame(pItemBombUp.x, pItemBombUp.y, false);
                pointerHand.transform.rotation = Quaternion.Euler(0, 180, 0);
                _instructionsTap.SetSystemText("NOW COLLECT THIS ITEM",
                    ConvertPosCamToCanvas(pointerHand.transform.position) + new Vector2(0, -80));
                levelScenePvpTutorial.SetEnableInputMove(true);
                levelScenePvpTutorial.ShowElementGui(ElementGui.StatBombUp);
                await Process(WaitHeroMoveTo(pItemBombUp));
                _instructionsTap.HideSystemBox();
                levelScenePvpTutorial.SetEnableInputMove(false);
                Destroy(pointerHand);
                // _instructionsTap.HideInstruction();
                levelScenePvpTutorial.RemoveItemOnMap(pItemBombUp);
                levelScenePvpTutorial.AddItemToPlayer(0, HeroItem.BombUp, 1);
                Destroy(pointerHand);
                await Process(WebGLTaskDelay.Instance.Delay(1000));
            }

            await Process(_instructionsTap.NpcGuide(
                "BOMB helps you to increase the maximum bomb limit that can be placed at the same time.", "dialog_5"));
            await Process(_instructionsTap.InstructionsBox.HideAsync());

            // Plant 2 Bomb
            {
                levelScenePvpTutorial.SetEnableInputMove(true);
                _instructionsTap.SetSystemText("PLACE 2 BOMBS TO DESTROY BLOCKS", new Vector2(0, -100));
                var posPlantBomb = new List<Vector2Int>() {
                    new(0, 5), // 
                    new(4, 6) //
                };
                var pointerHands = new List<GameObject>();
                foreach (var pos in posPlantBomb) {
                    pointerHands.Add(CreatPointerInGame(pos.x, pos.y));
                }

                var bombs = await ProcessResult(levelScenePvpTutorial.WaitHeroPlanMultiBomb(posPlantBomb));
                // _instructionsTap.HideInstruction();
                _instructionsTap.HideSystemBox();
                levelScenePvpTutorial.SetEnableInputMove(false);
                foreach (var pointerHand in pointerHands) {
                    Destroy(pointerHand);
                }

                // Move to safe point
                {
                    var posSafe = new Vector2Int(2, 5);
                    var pointerHand = CreatPointerInGame(posSafe.x, posSafe.y);
                    levelScenePvpTutorial.SetEnableInputMove(true);
                    _instructionsTap.SetSystemText("RUN OUT OF THE EXPLOSION AREA",
                        ConvertPosCamToCanvas(pointerHand.transform.position) + new Vector2(0, -80));
                    await Process(WaitHeroMoveTo(posSafe));
                    _instructionsTap.HideSystemBox();
                    levelScenePvpTutorial.SetEnableInputMove(false);
                    Destroy(pointerHand);
                }
                await Process(WebGLTaskDelay.Instance.Delay(500));
                foreach (var bomb in bombs) {
                    levelScenePvpTutorial.ExplodeBombTutorial(bomb);
                }
            }
            // PickUp item fire-up
            {
                await Process(_instructionsTap.NpcGuide(
                    "We found a new item that increases the bomb's explosive length.", "dialog_6"));
                await Process(_instructionsTap.InstructionsBox.HideAsync());
                levelScenePvpTutorial.SetEnableInputMove(true);
                var pointerHand = CreatPointerInGame(pItemFireUp.x, pItemFireUp.y, false);
                levelScenePvpTutorial.SetEnableInputMove(true);
                _instructionsTap.SetSystemText("COLLECT RANGE ITEM",
                    ConvertPosCamToCanvas(pointerHand.transform.position) + new Vector2(100, -80));
                levelScenePvpTutorial.ShowElementGui(ElementGui.StatFireUp);
                await Process(WaitHeroMoveTo(pItemFireUp));
                _instructionsTap.HideSystemBox();
                levelScenePvpTutorial.SetEnableInputMove(false);
                Destroy(pointerHand);
                levelScenePvpTutorial.RemoveItemOnMap(pItemFireUp);
                levelScenePvpTutorial.AddItemToPlayer(0, HeroItem.FireUp, 1);
                await Process(WebGLTaskDelay.Instance.Delay(1000));
            }

            // Plant bomb to test new power
            {
                List<BombPvp> bombs;
                // Move to plant bomb
                {
                    levelScenePvpTutorial.SetEnableInputMove(true);
                    _instructionsTap.SetSystemText("PLACE BOMB HERE TO TEST NEW POWER", new Vector2(0, -100));
                    var posPlantBomb = new Vector2Int(0, 3);
                    var pointerHand = CreatPointerInGame(posPlantBomb.x, posPlantBomb.y);
                    pointerHand.transform.rotation = Quaternion.Euler(0, 180, -90);
                    bombs = await ProcessResult(
                        levelScenePvpTutorial.WaitHeroPlanMultiBomb(new List<Vector2Int>() { posPlantBomb }));
                    // _instructionsTap.HideInstruction();
                    _instructionsTap.HideSystemBox();
                    levelScenePvpTutorial.SetEnableInputMove(false);
                    Destroy(pointerHand);
                }

                // Move to safe point
                {
                    var posSafe = new Vector2Int(1, 4);
                    var pointerHand = CreatPointerInGame(posSafe.x, posSafe.y);
                    levelScenePvpTutorial.SetEnableInputMove(true);
                    _instructionsTap.SetSystemText("RUN OUT OF THE EXPLOSION AREA",
                        ConvertPosCamToCanvas(pointerHand.transform.position) + new Vector2(0, -80));
                    await Process(WaitHeroMoveTo(posSafe));
                    _instructionsTap.HideSystemBox();
                    levelScenePvpTutorial.SetEnableInputMove(false);
                    Destroy(pointerHand);
                }
                await Process(WebGLTaskDelay.Instance.Delay(500));
                foreach (var bomb in bombs) {
                    levelScenePvpTutorial.ExplodeBombTutorial(bomb);
                }
            }

            // PickUp item boot
            {
                await Process(_instructionsTap.NpcGuide(
                    "We find a Speed. This item will help you move faster.", "dialog_7"));
                await Process(_instructionsTap.InstructionsBox.HideAsync());
                levelScenePvpTutorial.SetEnableInputMove(true);
                var pointerHand = CreatPointerInGame(pItemSpeedUp.x, pItemSpeedUp.y, false);
                levelScenePvpTutorial.SetEnableInputMove(true);
                _instructionsTap.SetSystemText("COLLECT SPEED ITEM",
                    ConvertPosCamToCanvas(pointerHand.transform.position) + new Vector2(100, -80));
                levelScenePvpTutorial.ShowElementGui(ElementGui.StatBoots);
                await Process(WaitHeroMoveTo(pItemSpeedUp));
                _instructionsTap.HideSystemBox();
                levelScenePvpTutorial.SetEnableInputMove(false);
                Destroy(pointerHand);
                levelScenePvpTutorial.RemoveItemOnMap(pItemSpeedUp);
                levelScenePvpTutorial.AddItemToPlayer(0, HeroItem.Boots, 1);
                await Process(WebGLTaskDelay.Instance.Delay(1000));
            }

            // Kill enemy
            {
                levelScenePvpTutorial.ShowElementGui(ElementGui.StatDamage);
                List<BombPvp> bombs;
                // Move to plant bomb
                {
                    levelScenePvpTutorial.SetEnableInputMove(true);
                    var posPlantBomb = new Vector2Int(6, 2);
                    var pointerHand = CreatPointerInGame(posPlantBomb.x, posPlantBomb.y);
                    _instructionsTap.SetSystemText("PLACE BOMB HERE TO KILL ENEMY",
                        ConvertPosCamToCanvas(pointerHand.transform.position) + new Vector2(-100, 200));
                    bombs = await ProcessResult(
                        levelScenePvpTutorial.WaitHeroPlanMultiBomb(new List<Vector2Int>() { posPlantBomb }));
                    _instructionsTap.HideSystemBox();
                    levelScenePvpTutorial.SetEnableInputMove(false);
                    Destroy(pointerHand);
                }
                // Move to safe point
                {
                    var posSafe = new Vector2Int(4, 1);
                    var pointerHand = CreatPointerInGame(posSafe.x, posSafe.y);
                    levelScenePvpTutorial.SetEnableInputMove(true);
                    _instructionsTap.SetSystemText("RUN OUT OF THE EXPLOSION AREA",
                        ConvertPosCamToCanvas(pointerHand.transform.position) + new Vector2(0, -80));
                    await Process(WaitHeroMoveTo(posSafe));
                    _instructionsTap.HideSystemBox();
                    levelScenePvpTutorial.SetEnableInputMove(false);
                    Destroy(pointerHand);
                }
                await Process(WebGLTaskDelay.Instance.Delay(500));
                foreach (var bomb in bombs) {
                    levelScenePvpTutorial.ExplodeBombTutorial(bomb);
                }

                levelScenePvpTutorial.SetEnemyInJail();

                // Move to kick Jail
                {
                    var posTo = new Vector2Int(6, 0);
                    var pointerHand = CreatPointerInGame(posTo.x, posTo.y, false);
                    levelScenePvpTutorial.SetEnableInputMove(true);
                    _instructionsTap.SetSystemText("TOUCH JAIL TO DEFEAT ENEMY",
                        ConvertPosCamToCanvas(pointerHand.transform.position) + new Vector2(0, -80));
                    await Process(WaitHeroMoveTo(posTo));
                    _instructionsTap.HideSystemBox();
                    levelScenePvpTutorial.SetEnableInputMove(false);
                    Destroy(pointerHand);
                }
                levelScenePvpTutorial.KillEnemy();
            }
        }

        private async Task Step2() {
            const int playerSlot = 0;
            const int enemySlot = 1;
            levelScenePvpTutorial.SetEnableInputMove(false);
            levelScenePvpTutorial.SetEnableInputPlantBomb(false);
            levelScenePvpTutorial.HideElementGui(ElementGui.Joystick);
            await Process(_instructionsTap.NpcGuide(
                "Congratulations on completing the basic tutorial.", "dialog_8"));
            // Show popup collect Key
            {
                await Process(_instructionsTap.NpcGuide("There's a gift for you.", "dialog_9"));
                var popup = ShowPopupCollectKey();
                await Process(_instructionsTap.NpcGuide("The key, it helps you get out of jail.", "dialog_10"));
                await Process(popup.CloseAsync(_soundManager));
                await Process(_instructionsTap.InstructionsBox.HideAsync());
            }
            levelScenePvpTutorial.ShowElementGui(ElementGui.ButtonKey);
            // Enemy plant bomb
            {
                await Process(levelScenePvpTutorial.WaitHeroAutoMoveTo(0, 4, 2, true));
                await Process(WebGLTaskDelay.Instance.Delay(500));
                levelScenePvpTutorial.RevivePlayer(enemySlot, new Vector2Int(6, 0));
                await Process(WebGLTaskDelay.Instance.Delay(500));
                var posPlantBomb = new Vector2Int(5, 2);
                await Process(levelScenePvpTutorial.WaitHeroAutoMoveTo(enemySlot, posPlantBomb.x, posPlantBomb.y));
                await Process(WebGLTaskDelay.Instance.Delay(500));
                var bomb = levelScenePvpTutorial.PlanTheBomb(posPlantBomb.x, posPlantBomb.y, enemySlot);
                await Process(levelScenePvpTutorial.WaitHeroAutoMoveTo(enemySlot, 6, 1));
                await Process(WebGLTaskDelay.Instance.Delay(1000));
                levelScenePvpTutorial.ExplodeBombTutorial(bomb);
                levelScenePvpTutorial.SetPlayerInJail();
                await Process(WebGLTaskDelay.Instance.Delay(500));
            }

            _instructionsTap.SetPosPointerHand(levelScenePvpTutorial.ButtonKeyTransform
                .GetComponent<RectTransform>());
            _instructionsTap.SetSystemText("USE KEY BOOSTER TO ESCAPE FROM JAIL");
            var posBtKey = levelScenePvpTutorial.ButtonKeyTransform.position;
            _instructionsTap.SetSystemBoxPos(new Vector2(posBtKey.x - 150, posBtKey.y + 300));

            levelScenePvpTutorial.SetEnableInputKey(true);
            await Process(levelScenePvpTutorial.WaitHeroUseKey());
            _instructionsTap.HidePointerHand();
            _instructionsTap.HideSystemBox();
            levelScenePvpTutorial.UpdateHealthUi(playerSlot, 10);
            levelScenePvpTutorial.SetEnableInputKey(false);
            await Process(WebGLTaskDelay.Instance.Delay(500));
            await Process(_instructionsTap.NpcGuide("You need to wait a while to use the key again.", "dialog_10_1"));
            await Process(_instructionsTap.InstructionsBox.HideAsync());
            await Process(WebGLTaskDelay.Instance.Delay(1000));

            // Show popup collect Shield
            {
                var popup = ShowPopupCollectShield();
                await Process(_instructionsTap.NpcGuide("This is the shield, and it protects you from the explosion",
                    "dialog_11"));
                await Process(popup.CloseAsync(_soundManager));
                await Process(_instructionsTap.InstructionsBox.HideAsync());
            }

            // Enemy plant bomb
            {
                var posPlantBomb = new Vector2Int(5, 2);
                var posEnemyWillDie = new Vector2Int(6, 2);
                await Process(
                    levelScenePvpTutorial.WaitHeroAutoMoveTo(enemySlot, posPlantBomb.x, posPlantBomb.y, true));
                await Process(WebGLTaskDelay.Instance.Delay(500));
                var bomb = levelScenePvpTutorial.PlanTheBomb(posPlantBomb.x, posPlantBomb.y, enemySlot);
                await Process(levelScenePvpTutorial.WaitHeroAutoMoveTo(enemySlot, posEnemyWillDie.x, posEnemyWillDie.y,
                    true));

                _instructionsTap.SetPosPointerHand(levelScenePvpTutorial.ButtonKeyTransform
                    .GetComponent<RectTransform>());
                _instructionsTap.SetSystemText("USE SHIELD BOOSTER TO PROTECT");
                var posBtShield = levelScenePvpTutorial.ButtonShieldTransform.position;
                _instructionsTap.SetSystemBoxPos(new Vector2(posBtShield.x - 150, posBtShield.y + 300));
                levelScenePvpTutorial.SetEnableInputShield(true);
                levelScenePvpTutorial.ShowElementGui(ElementGui.ButtonShield);
                levelScenePvpTutorial.HideElementGui(ElementGui.Joystick);
                _instructionsTap.SetPosPointerHand(levelScenePvpTutorial.ButtonShieldTransform
                    .GetComponent<RectTransform>());
                await Process(levelScenePvpTutorial.WaitHeroUseShield());
                levelScenePvpTutorial.SetEnableInputShield(false);
                _instructionsTap.HideSystemBox();
                await Process(_instructionsTap.InstructionsBox.HideAsync());
                _instructionsTap.HidePointerHand();
                _instructionsTap.HideInstruction();
                await Process(WebGLTaskDelay.Instance.Delay(500));
                levelScenePvpTutorial.ExplodeBombTutorial(bomb);
                levelScenePvpTutorial.SetPlayerInJail(enemySlot);
                await Process(WebGLTaskDelay.Instance.Delay(500));
                levelScenePvpTutorial.SetEnableInputMove(true);
                levelScenePvpTutorial.ShowElementGui(ElementGui.Joystick);
                await Process(levelScenePvpTutorial.WaitHeroMoveTo(playerSlot, posEnemyWillDie.x, posEnemyWillDie.y));
                levelScenePvpTutorial.HideElementGui(ElementGui.Joystick);
                levelScenePvpTutorial.SetEnableInputMove(false);
                levelScenePvpTutorial.KillEnemy();
            }
        }

        private async Task Step3() {
            {
                var popup = ShowPopupNew4Equipment();
                var productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
                await productItemManager.InitializeAsync();
                await Process(_instructionsTap.NpcGuide(
                    "Well done, I have some skins for you. Now we will go to equip skins.", "dialog_12", true));
                await Process(popup.CloseAsync(_soundManager));
                await Process(_instructionsTap.InstructionsBox.HideAsync());
            }
            _instructionsTap.ShowBgPvp();
            levelScenePvpTutorial.RevivePlayer(1, new Vector2Int(6, 0));
            levelScenePvpTutorial.ResetPlayerMainUpgrade(new Vector2Int(0, 6));

            // pick Item
            {
                var popup = ShowEquipmentPopup();
                popup.BtFindMatch.Interactable = false;
                popup.Equipment.TurnOffInteractableAll();
                //
                await Process(WebGLTaskDelay.Instance.Delay(500));
                _instructionsTap.SetSystemText("TAP EQUIP BOMB SKIN");
                var halfRect = 35;
                await Process(popup.WaitUiEquip(SkinChestType.Bomb, _instructionsTap.PointerHand,
                    _instructionsTap.BoxSystem, 90,
                    new Vector3(halfRect - 20, -halfRect, 0), _soundManager));
                _instructionsTap.HideSystemBox();
                await Process(WebGLTaskDelay.Instance.Delay(500));
                _instructionsTap.SetSystemText("TAP EQUIP WING SKIN");
                await Process(popup.WaitUiEquip(SkinChestType.Avatar, _instructionsTap.PointerHand,
                    _instructionsTap.BoxSystem, 90,
                    new Vector3(halfRect - 20, halfRect, 0), _soundManager));
                _instructionsTap.HideSystemBox();
                await Process(WebGLTaskDelay.Instance.Delay(500));
                _instructionsTap.SetSystemText("TAP EQUIP FIRE SKIN");
                await Process(popup.WaitUiEquip(SkinChestType.Explosion, _instructionsTap.PointerHand,
                    _instructionsTap.BoxSystem, 270,
                    new Vector3(-halfRect + 20, -halfRect, 0), _soundManager));
                _instructionsTap.HideSystemBox();
                await Process(WebGLTaskDelay.Instance.Delay(500));
                _instructionsTap.SetSystemText("TAP EQUIP TRAIL SKIN");
                await Process(popup.WaitUiEquip(SkinChestType.Trail, _instructionsTap.PointerHand,
                    _instructionsTap.BoxSystem, 270,
                    new Vector3(-halfRect + 20, halfRect, 0), _soundManager));
                _instructionsTap.HideSystemBox();
                await Process(WebGLTaskDelay.Instance.Delay(500));
                _instructionsTap.SetSystemText("FIND A MATCH WITH NEW SKIN", new Vector2(0, -100));
                popup.BtFindMatch.Interactable = true;
                _instructionsTap.SetRotatePointerHand(270);
                _instructionsTap.SetPosPointerHand(popup.BtFindMatch.transform.position + new Vector3(220, 70, 0));
                await Process(popup.FindMatchAsync(_soundManager));
                _instructionsTap.HideSystemBox();
                _instructionsTap.HideBgPvp();
                _instructionsTap.HidePointerHand();
                Destroy(popup.gameObject);
            }

            // ClashBot
            {
                GenRanBrick();
                levelScenePvpTutorial.GenRanItemUnderBrick();
                levelScenePvpTutorial.SetEnableInputMove(false);
                levelScenePvpTutorial.SetEnableInputPlantBomb(true);
                await Process(WebGLTaskDelay.Instance.Delay(500));
                await Process(_instructionsTap.NpcGuide("Now let's play a game.", "dialog_12_1"));
                await Process(_instructionsTap.InstructionsBox.HideAsync());
                levelScenePvpTutorial.ShowElementGui(ElementGui.Joystick);
                _instructionsTap.SetRotatePointerHand(0);
                _instructionsTap.SetPosPointerHand(levelScenePvpTutorial.ButtonShieldTransform
                    .GetComponent<RectTransform>());
                await Process(levelScenePvpTutorial.WaitClashBot(_instructionsTap.PointerHand));
            }
            {
                var winPopup = ShowWinPopup();
                winPopup.BoosterBonus.SetActive(false);
                await Process(WebGLTaskDelay.Instance.Delay(200));
                _instructionsTap.SetRotatePointerHand(270);
                var posRankIcon = winPopup.RankIcon.transform.position;
                _instructionsTap.SetPosPointerHand(posRankIcon, new Vector3(50, 0, 0));
                _instructionsTap.SetSystemText("RANK POINT");
                _instructionsTap.SetSystemBoxPos(posRankIcon + new Vector3(390, 0, 0));
                await Process(_instructionsTap.NpcGuide(
                    "Rank Point helps you increase your rank in the rankings.", "dialog_13"));
                await Process(_instructionsTap.InstructionsBox.HideAsync());
                _instructionsTap.HideSystemBox();
                _instructionsTap.HidePointerHand();
                await Process(WebGLTaskDelay.Instance.Delay(500));
                Destroy(winPopup.gameObject);
            }
        }

        private async Task Step4() {
            _instructionsTap.ShowBgPvp();
            levelScenePvpTutorial.RevivePlayer(1, new Vector2Int(6, 0));
            levelScenePvpTutorial.ResetPlayerMainUpgrade(new Vector2Int(0, 6));
            {
                var popup = ShowLeaderboardPopup();
                await Process(WebGLTaskDelay.Instance.Delay(200));

                await Process(_instructionsTap.NpcGuide(
                    "PVP arena rankings, you can get rewards every season here.", "dialog_14"));
                // _instructionsTap.HidePointerHand();
                await Process(WebGLTaskDelay.Instance.Delay(500));
                var dim = tutorialGui.CreateDimInGui(_instructionsTap.ShortDialog,
                    popup.FrameRank.transform.position + new Vector3(0, -10, 0));
                dim.transform.SetSiblingIndex(3);
                dim.SetSize(Screen.width + 300.0f, 400.0f);
                await Process(_instructionsTap.NpcGuide(
                    "For your own ranking information, follow this area.", "dialog_15"));
                await Process(_instructionsTap.InstructionsBox.HideAsync());
                Destroy(popup.gameObject);
                Destroy(dim.gameObject);
            }
            {
                var popup = ShowEquipmentFullPopup();
                popup.Equipment.TurnOffInteractableAll();
                _instructionsTap.SetSystemText("WIN THIS MATCH TO +20 RANK POINTS", new Vector2(0, -100));
                popup.BtFindMatch.Interactable = true;
                await Process(popup.FindMatchAsync(_soundManager));
                _instructionsTap.HideSystemBox();
                _instructionsTap.HideBgPvp();
                Destroy(popup.gameObject);
            }
            // ClashBot
            {
                GenRanBrick();
                levelScenePvpTutorial.GenRanItemUnderBrick();
                levelScenePvpTutorial.SetEnableInputMove(false);
                levelScenePvpTutorial.SetEnableInputPlantBomb(true);
                await Process(WebGLTaskDelay.Instance.Delay(500));
                levelScenePvpTutorial.ShowElementGui(ElementGui.Joystick);
                _instructionsTap.SetRotatePointerHand(0);
                _instructionsTap.SetPosPointerHand(levelScenePvpTutorial.ButtonShieldTransform
                    .GetComponent<RectTransform>());
                await Process(levelScenePvpTutorial.WaitClashBot(_instructionsTap.PointerHand));
            }
            // Show Dialog Win
            {
                var winPopup = ShowWinPopup(true);
                winPopup.BoosterBonus.SetActive(false);
                await Process(WebGLTaskDelay.Instance.Delay(200));
                _instructionsTap.SetRotatePointerHand(270);
                _instructionsTap.SetPosPointerHand(winPopup.BtNext.transform.position, new Vector3(120, 40, 0));
                await Process(winPopup.WaitClose(_soundManager));
                Destroy(winPopup.gameObject);
                _instructionsTap.HidePointerHand();
                await Process(WebGLTaskDelay.Instance.Delay(500));
            }
            _instructionsTap.ShowBgPvp();
            var popupMenu = ShowPvpMenuPopup();
            var posBtFindMatch = popupMenu.BtFindMatch.transform.position;
            {
                popupMenu.BtFindMatch.interactable = false;
                // popup.SetVisible(1, false);
                popupMenu.SetDimSiblingIndex(1);
                popupMenu.SetVisible(2, false);
                _instructionsTap.InstructionsBox.SetPos(BLInstructionsBox.PosBox.Bot);
                await Process(_instructionsTap.NpcGuide("This is your rank information and your hero stats.",
                    "dialog_16"));
                popupMenu.SetDimSiblingIndex(2);
                popupMenu.SetVisible(2, true);
                await Process(
                    _instructionsTap.NpcGuide("This is a Booster Inventory you can choose to use in battle.",
                        "dialog_17"));
                popupMenu.SetDimSiblingIndex(3);
                // Claim Rank Conquest booster
                {
                    var popupClaim = ShowClaimFullRankConquestPopup();
                    await Process(_instructionsTap.NpcGuide(
                        "Rank Conquest booster helps you earn double Rank Points if you win.", "dialog_18",
                        true));
                    _instructionsTap.SetRotatePointerHand(270);
                    _instructionsTap.SetPosPointerHand(popupClaim.BtClaim.transform.position, new Vector3(120, 40, 0));
                    await Process(popupClaim.WaitClose(_soundManager));
                    _instructionsTap.HidePointerHand();
                    Destroy(popupClaim.gameObject);
                    popupMenu.SetBoosterQuality(BoosterType.FullCupBonus, 3);
                }
                popupMenu.SetDimSiblingIndex(2);
                await Process(_instructionsTap.NpcGuide(
                    "Choose Rank Conquest booster to play the match.", "dialog_19",
                    true));
                _instructionsTap.SetRotatePointerHand(0);
                _instructionsTap.PointerHand.SetActive(true);
                await Process(popupMenu.WaitChooseBooster(BoosterType.FullCupBonus, _instructionsTap.PointerHand,
                    _soundManager));
                _instructionsTap.HidePointerHand();
                await Process(_instructionsTap.InstructionsBox.HideAsync());
                popupMenu.HideDim();
                _instructionsTap.SetRotatePointerHand(270);
                _instructionsTap.SetPosPointerHand(posBtFindMatch, new Vector3(180, 40, 0));
                await Process(popupMenu.WaitFindMatch(_soundManager));
                popupMenu.gameObject.SetActive(false);
                _instructionsTap.HidePointerHand();
            }
            // Clash Bot
            {
                levelScenePvpTutorial.RevivePlayer(1, new Vector2Int(6, 0));
                levelScenePvpTutorial.ResetPlayerMainUpgrade(new Vector2Int(0, 6));
                GenRanBrick();
                levelScenePvpTutorial.GenRanItemUnderBrick();
                levelScenePvpTutorial.SetEnableInputMove(false);
                levelScenePvpTutorial.SetEnableInputPlantBomb(true);
                await Process(WebGLTaskDelay.Instance.Delay(500));
                levelScenePvpTutorial.ShowElementGui(ElementGui.Joystick);
                _instructionsTap.SetRotatePointerHand(0);
                _instructionsTap.SetPosPointerHand(levelScenePvpTutorial.ButtonShieldTransform
                    .GetComponent<RectTransform>());
                _instructionsTap.HideBgPvp();
                await Process(levelScenePvpTutorial.WaitClashBot(_instructionsTap.PointerHand));
                _instructionsTap.HidePointerHand();
            }
            {
                var winPopup = ShowWinPopup(true);
                winPopup.BoosterBonus.SetActive(true);
                winPopup.SetRank(40);
                await Process(ShowActivatedConquestPopup().WaitClose(_soundManager));
                await Process(winPopup.WaitClose(_soundManager));
                Destroy(winPopup.gameObject);
            }

            _instructionsTap.ShowBgPvp();
            popupMenu.gameObject.SetActive(true);
            // Claim Rank Conquest booster
            {
                var popupClaim = ShowClaimFullRankGuardianPopup();
                popupMenu.SetBoosterHighlight(BoosterType.FullCupBonus, false);
                popupMenu.SetDimSiblingIndex(3);
                popupMenu.ShowDim();
                await Process(_instructionsTap.NpcGuide(
                    "Rank Guardian booster helps you not to lose Rank Point if you lose the battle.", "dialog_20",
                    true));
                _instructionsTap.SetRotatePointerHand(270);
                _instructionsTap.SetPosPointerHand(popupClaim.BtClaim.transform.position, new Vector3(120, 40, 0));
                await Process(popupClaim.WaitClose(_soundManager));
                _instructionsTap.HidePointerHand();
                Destroy(popupClaim.gameObject);
                popupMenu.SetDimSiblingIndex(2);
                popupMenu.SetBoosterQuality(BoosterType.FullRankGuardian, 3);
                await Process(_instructionsTap.NpcGuide(
                    "Choose Rank Guardian booster to play the match.", "dialog_21",
                    true));
                _instructionsTap.SetRotatePointerHand(0);
                _instructionsTap.PointerHand.SetActive(true);
                await Process(popupMenu.WaitChooseBooster(BoosterType.FullRankGuardian, _instructionsTap.PointerHand,
                    _soundManager));
                _instructionsTap.HidePointerHand();
                await Process(_instructionsTap.InstructionsBox.HideAsync());
                popupMenu.HideDim();
                _instructionsTap.SetRotatePointerHand(270);
                _instructionsTap.SetPosPointerHand(posBtFindMatch, new Vector3(180, 40, 0));
                await Process(popupMenu.WaitFindMatch(_soundManager));
                popupMenu.gameObject.SetActive(false);
                _instructionsTap.HidePointerHand();
            }

            // Clash Bot
            {
                levelScenePvpTutorial.ResetPlayerMainUpgrade(new Vector2Int(4, 2));
                levelScenePvpTutorial.RevivePlayer(1, new Vector2Int(6, 0));
                levelScenePvpTutorial.HideElementGui(ElementGui.Joystick);
                levelScenePvpTutorial.HideElementGui(ElementGui.ButtonBomb);
                levelScenePvpTutorial.HideElementGui(ElementGui.ButtonKey);
                levelScenePvpTutorial.HideElementGui(ElementGui.ButtonShield);
                GenRanBrick();
                levelScenePvpTutorial.RemoveAllBrickAndItem();
                levelScenePvpTutorial.SetEnableInputMove(false);
                levelScenePvpTutorial.SetEnableInputPlantBomb(false);
                await Process(WebGLTaskDelay.Instance.Delay(200));
                _instructionsTap.HideBgPvp();
                // Enemy plant bomb
                {
                    var enemySlot = 1;
                    var posPlantBomb = new Vector2Int(5, 2);
                    await Process(levelScenePvpTutorial.WaitHeroAutoMoveTo(enemySlot, posPlantBomb.x, posPlantBomb.y));
                    await Process(WebGLTaskDelay.Instance.Delay(500));
                    var bomb = levelScenePvpTutorial.PlanTheBomb(posPlantBomb.x, posPlantBomb.y, enemySlot);
                    await Process(levelScenePvpTutorial.WaitHeroAutoMoveTo(enemySlot, 6, 1));
                    await Process(WebGLTaskDelay.Instance.Delay(1000));
                    levelScenePvpTutorial.ExplodeBombTutorial(bomb);
                    levelScenePvpTutorial.SetPlayerInJail();
                    await Process(WebGLTaskDelay.Instance.Delay(1000));
                    levelScenePvpTutorial.KillPlayer();
                }
                {
                    var losePopup = ShowLosePopup();
                    losePopup.BoosterBonus.SetActive(true);
                    await Process(ShowActivatedGuardianPopup().WaitClose(_soundManager));
                    await Process(losePopup.WaitClose(_soundManager));
                    Destroy(losePopup.gameObject);
                }
            }
        }

        private async Task Step5() {
            levelScenePvpTutorial.HideAllElementGui();
            _instructionsTap.ShowDimBackground();
            var popup = ShowCompleteRewardPopup();
            await Process(_instructionsTap.NpcGuide("Congratulations on completing the basic tutorial.", "dialog_22",
                true));
            await popup.CloseAsync(_soundManager);
            await Process(_instructionsTap.InstructionsBox.HideAsync());
            await Process(WebGLTaskDelay.Instance.Delay(500));
        }

        private void Finish(bool isShowDialogReward) {
            _storageManager.SelectedHeroKey = -1;
            levelScenePvpTutorial.HideAllElementGui();
            _instructionsTap.ShowDimBackground();
            _instructionsTap.HideSystemBox();
            _instructionsTap.InstructionsBox.HideAsync();
            levelScenePvpTutorial.StopGame();
            dialogWaiting.SetActive(true);
            NoAwaitWithoutTryCatch(async () => { await StepGetReward(isShowDialogReward); });
        }

        private async Task StepGetReward(bool isShowDialogReward) {
            var gameReadyController = new GameReadyController(canvasDialogTop, (progress => {
#if UNITY_EDITOR
                Debug.Log(progress.Details);
#endif
                waitingText.text = progress.Details;
            }));
            try {
                levelScenePvpTutorial.FinishTutorialInGame();
                _soundManager.StopMusic();
                await gameReadyController.Start(100);
                await ConnectScene.Scripts.ConnectScene.LoadForMain();
                await TutorialGetReward(isShowDialogReward);
                const string sceneName = "MainMenuScene";
                await SceneLoader.LoadSceneAsync(sceneName);
            } catch (Exception e) {
                if (e is ServerMaintenanceException se) {
                    DialogOK.ShowErrorAndKickToConnectScene(canvasDialog, "Server is under maintenance");
                    // var dialog = await DialogMaintenance.Create();
                    // dialog.Show(canvasDialogTop);
                    // await dialog.WaitMaintenanceFinish(se.SecondWait);
                    // GameReadyController.IgnoreMaintenance = true;
                    // App.Utils.KickToConnectScene();
                    return;
                }
                if (e is LoginException le) {
                    if (le.Error == LoginException.ErrorType.WrongVersion) {
                        const string msg =
                            "Your current version is outdated and needs to be updated to run the game";
                        DialogOK.ShowError(canvasDialogTop, msg);
                        return;
                    }
                }
                Debug.LogException(e);
                if (e is NoInternetException) {
                    var dialog = await AfDialogCheckConnection.Create();
                    dialog.Show(canvasDialogTop);
                    await dialog.WaitForHide();
                    Destroy(dialog.gameObject);
                    NoAwaitWithoutTryCatch(async () => { await StepGetReward(isShowDialogReward); });
                    return;
                }
                DialogOK.ShowError(canvasDialogTop, e.Message);
                // DialogOK.ShowErrorAndKickToConnectScene(canvasDialogTop, e.Message);
            }
        }

        private async Task TutorialGetReward(bool isShowDialogReward) {
            _analytics.TrackScene(SceneType.RewardTutorial);
            var serverRequester = ServiceLocator.Instance.Resolve<IServerRequester>();
            var soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            var newcomerGift = await serverRequester.GetNewcomerGift();
            _analytics.TrackClaimRewardTutorial();
            // Check ưu tiên equip Hero
            var heroes = await ServiceLocator.Instance.Resolve<ITRHeroManager>().GetHeroesAsync("HERO");
            foreach (var hero in heroes) {
                if (hero.ItemId != (int) GachaChestProductId.Ninja) {
                    continue;
                }
                _storageManager.SelectedHeroKey = hero.InstanceId;
                await serverRequester.ActiveTRHero(hero.InstanceId);
                break;
            }
            soundManager.PlaySound(Audio.TutorialReward);
            // tutorialManager.ShowFeatureGuideDone();
            if (isShowDialogReward) {
                var task = new TaskCompletionSource<bool>();
                var dialogReward = await BLDialogLuckyWheelReward.CreateDialogTutorialClaimLarge();
                dialogReward.UpdateUI(newcomerGift.Select(it => (it.ItemId, it.Quantity)));
                dialogReward.OnDidHide(() => {
                    _soundManager.PlaySound(Audio.Tap);
                    task.SetResult(true);
                });
                dialogReward.Show(canvasDialogTop);
                await task.Task;
                soundManager.PlaySound(Audio.TutorialReward);
            }
        }

        #endregion

        /*
         * Quy ước tutorial chỉ chạy 1 call
         * Sử dụng: Process, ProcessResult, CancelProcess
         */

        #region TaskController

        private readonly CancellationTokenSource _tokenCancel = new();
        private TaskCompletionSource<bool> _currentTask;
        private class CancelProcessException : Exception { };

        private static async void NoAwaitWithoutTryCatch(Func<Task> callable) {
            var task = callable();
            await task;
        }

        private async Task Process(Task task) {
            var taskCompletion = new TaskCompletionSource<bool>();
            NoAwaitWithoutTryCatch(async () => {
                await task;
                if (!_tokenCancel.IsCancellationRequested) {
                    taskCompletion.SetResult(true);
                }
            });
            await ProcessTaskCompletion(taskCompletion);
        }

        private async Task<T> ProcessResult<T>(Task<T> task) {
            var warp = new TaskCompletionSource<bool>();
            var taskResult = new TaskCompletionSource<T>();
            NoAwaitWithoutTryCatch(async () => {
                var r = await task;
                if (!_tokenCancel.IsCancellationRequested) {
                    warp.SetResult(true);
                    taskResult.SetResult(r);
                }
            });
            await ProcessTaskCompletion(warp);
            return await taskResult.Task;
        }

        private async Task ProcessTaskCompletion(TaskCompletionSource<bool> task) {
            Debug.Assert(_currentTask == null, "Not support Multiple Tasks");
            _currentTask = task;
            await task.Task;
            _currentTask = null;
            if (_tokenCancel.IsCancellationRequested) {
                throw new CancelProcessException();
            }
        }

        private void CancelProcess() {
            _tokenCancel.Cancel();
            _currentTask?.SetResult(false);
        }

        #endregion
    }
}