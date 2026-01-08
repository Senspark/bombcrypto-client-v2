using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Actions;

using App;

using BLPvpMode.Data;
using BLPvpMode.Engine.Entity;
using BLPvpMode.GameView;
using BLPvpMode.Test;
using BLPvpMode.UI;

using DG.Tweening;

using Engine.Entities;
using Engine.Utils;

using Game.UI;

using JetBrains.Annotations;

using PvpMode.Entities;
using PvpMode.Manager;

using Scenes.PvpModeScene.Scripts;

using Senspark;

using UnityEngine;
using UnityEngine.UI;

using Random = UnityEngine.Random;

namespace Scenes.TutorialScene.Scripts {
    public class BLPvpHelper : MonoBehaviour {
        [SerializeField]
        protected Canvas canvasDialog;

        [SerializeField]
        protected TextAsset textPvpMapDetail;

        [SerializeField]
        protected BLLevelScenePvpSimulator levelScene;

        [SerializeField]
        protected GameObject bot;

        protected BLGuiPvp guiPvp => levelScene.GuiPvp;

        private ILevelViewPvp _levelView = null;
        public ILevelViewPvp LevelView => _levelView;

        public BLLevelScenePvpSimulator LevelScene => levelScene;

        private int _participantSlot;

        private bool _updateSendSpawnBomb;

        private BoosterStatus _boosterStatus;

        private ISoundManager _soundManager;
        private IInputManager _inputManager;

        private Action _checkCondition = null;
        private Action _onBombButtonClicked = null;
        private Action _onKeyButtonClicked = null;
        private Action _onShieldButtonClicked = null;
        private bool _enableMove = false;
        private bool _enablePlantBomb = true;

        private ObserverHandle _handle;
        
        private HotkeyCombo _hotkeyCombo; 

        private void Awake() {
            Physics2D.simulationMode = SimulationMode2D.Script;
            Physics2D.gravity = Vector2.zero;
            _handle = new ObserverHandle();
            _inputManager = ServiceLocator.Instance.Resolve<IInputManager>();

            _hotkeyCombo = ServiceLocator.Instance.Resolve<IHotkeyControlManager>().GetHotkeyCombo();
        }

        private void StartGame(Action onStartGame) {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _participantSlot = 0;
            levelScene.SetPvpMapDetails("Tutorial", textPvpMapDetail.text, _participantSlot);
            levelScene.LoadLevel();
            _levelView = levelScene.LevelView;
            var pvpHeroes = _levelView.GetPvpHeroes();
            for (var slot = 0; slot < pvpHeroes.Count; slot++) {
                pvpHeroes[slot]
                    .SetSlotAndFaction(slot, slot == _participantSlot ? FactionType.Ally : FactionType.Enemy);
            }
            levelScene.InitForSimulator();
            levelScene.SimulatorSyncSyncHero();
            // levelScene.UpdateBoostersInfinity();
            levelScene.StartGame(onStartGame);
        }

        private void OnDestroy() {
            DOTween.KillAll(true);
            ServiceLocator.Instance.Resolve<ISoundManager>().StopMusic();
            _handle.Dispose();
        }

        private void Update() {
            if (_levelView == null) {
                return;
            }
            if (levelScene.enabled) {
                _checkCondition?.Invoke();
                return;
            }
            var delta = Time.deltaTime;
            if (delta > BLLevelScenePvp.FixedDeltaTime) {
                delta = BLLevelScenePvp.FixedDeltaTime;
            }
            _levelView.Step(delta);

            if (_enableMove) {
                guiPvp.CheckInputKeyDown();
                // process movement from direction input
                _levelView.ProcessMovement(_participantSlot, guiPvp.GetDirectionFromInput());
            } else {
                _levelView.ProcessMovement(_participantSlot, Vector2.zero);
                if (_enablePlantBomb && (Input.GetKeyDown(_hotkeyCombo.GetControl(ControlKey.PlaceBom))
                                         || _inputManager.ReadButton(_inputManager.InputConfig.PlantBomb))) {
                    _onBombButtonClicked?.Invoke();
                }
            }

            // update BombLoseControl
            _levelView.ProcessUpdatePlayer(_participantSlot);
            _checkCondition?.Invoke();

            UiUpdateAvailableBomb();
        }

        private void UiUpdateAvailableBomb() {
            var bombAble = _levelView.EntityManager.PlayerManager.GetPlayerBySlot(_participantSlot).Bombable;
            guiPvp.SetQuantityBomb(bombAble.CountAvailableBomb());
        }

        public void AddItemOnMap(Vector2Int location, ItemType type) {
            _levelView.CreateItem(location, type);
        }

        public void RemoveItemOnMap(Vector2Int location) {
            _levelView.RemoveItem(new Vector2Int(location.x, location.y));
        }

        public Vector3 GetTilePosition(int tileX, int tileY) {
            var pos = LevelView.GetTilePosition(tileX, tileY);
            return pos;
        }

        public GameObject StartLocation => LevelView.StartLocation;

        public Task<bool> WaitStartGame(Action onLoadMap) {
            var task = new TaskCompletionSource<bool>();
            StartGame(() => { task.SetResult(true); });
            StopMusicWhenDie();
            onLoadMap?.Invoke();
            return task.Task;
        }

        public Task<bool> WaitHeroMoveTo(int slot, int tileX, int tileY) {
            var task = new TaskCompletionSource<bool>();
            var hero = LevelView.GetPvpHeroBySlot(slot);
            var posMoveTo = LevelView.GetTilePosition(tileX, tileY);
            _checkCondition = () => {
                var tile = hero.GetTileLocation();
                if (tile.x != tileX || tile.y != tileY) {
                    return;
                }
                var posHero = hero.GetPosition();
                if (posHero.x <= posMoveTo.x - 0.1) {
                    return;
                }
                if (posHero.x >= posMoveTo.x + 0.1) {
                    return;
                }
                if (posHero.y <= posMoveTo.y - 0.1) {
                    return;
                }
                if (posHero.y >= posMoveTo.y + 0.1) {
                    return;
                }
                _checkCondition = null;
                _soundManager.PlaySound(Audio.TutorialMoveToHand);
                task.SetResult(true);
            };
            return task.Task;
        }

        public Task<bool> WaitHeroAutoMoveTo(int slot, int tileX, int tileY, bool isFaceDownAfterMove = false) {
            var task = new TaskCompletionSource<bool>();
            var hero = LevelView.GetPvpHeroBySlot(slot);
            var path = LevelView.MapManager.ShortestPath(hero.GetTileLocation(), new Vector2Int(tileX, tileY), false,
                false);
            bool IsHeroMoveTo(int i, int j) {
                var tile = hero.GetTileLocation();
                var posMoveTo = LevelView.GetTilePosition(i, j);
                if (tile.x != i || tile.y != j) {
                    return false;
                }
                var posHero = hero.GetPosition();
                if (posHero.x <= posMoveTo.x - 0.1) {
                    return false;
                }
                if (posHero.x >= posMoveTo.x + 0.1) {
                    return false;
                }
                if (posHero.y <= posMoveTo.y - 0.1) {
                    return false;
                }
                if (posHero.y >= posMoveTo.y + 0.1) {
                    return false;
                }
                return true;
            }
            var frameDelayFaceTo = 4;
            _checkCondition = () => {
                if (path.Count > 0) {
                    // Move to center
                    var last = path.Last();
                    var posHero = hero.GetPosition();
                    var posMoveTo = LevelView.GetTilePosition(last.x, last.y);
                    var direction = posMoveTo - posHero;
                    if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y)) {
                        direction.y = 0;
                    } else {
                        direction.x = 0;
                    }
                    _levelView.ProcessMovement(slot, direction);
                    if (IsHeroMoveTo(last.x, last.y)) {
                        path.RemoveAt(path.Count - 1);
                    }
                }
                if (path.Count > 0) {
                    return;
                }
                if (isFaceDownAfterMove) {
                    if (frameDelayFaceTo > 0) {
                        frameDelayFaceTo--;
                        return;
                    }
                    hero.ForceUpdateFace(Vector2.down);
                }
                _checkCondition = null;
                _soundManager.PlaySound(Audio.TutorialMoveToHand);
                task.SetResult(true);
            };
            return task.Task;
        }

        public Task<List<BombPvp>> WaitHeroPlanMultiBomb(List<Vector2Int> posPlantBomb) {
            var task = new TaskCompletionSource<List<BombPvp>>();
            var result = new List<BombPvp>();
            _checkCondition = () => {
                var hero = LevelView.GetPvpHeroBySlot(_participantSlot);
                var tileLocation = hero.GetTileLocation();
                SetEnableInputPlantBomb(posPlantBomb.Any(pos => pos == tileLocation));
            };
            _onBombButtonClicked = () => {
                if (!_levelView.CheckSpawnPvpBomb(_participantSlot)) {
                    return;
                }
                var hero = LevelView.GetPvpHeroBySlot(_participantSlot);
                var tileLocation = hero.GetTileLocation();
                var idx = posPlantBomb.FindIndex(pos => pos == tileLocation);
                if (idx < 0) {
                    return;
                }
                posPlantBomb.RemoveAt(idx);
                var id = hero.Bombable.GetNextBombId();
                var bomb = hero.SpawnBomb(id, 2, hero.GetTileLocation());
                bomb.SetCountDownEnable(false);
                result.Add(bomb);
                if (posPlantBomb.Count > 0) {
                    return;
                }
                _handle.Dispose();
                _onBombButtonClicked = null;
                _checkCondition = null;
                task.SetResult(result);
            };
            _handle.AddObserver(guiPvp, new BLGuiObserver() { SpawnBomb = () => { _onBombButtonClicked(); } });
            return task.Task;
        }

        public Task<BombPvp> WaitHeroPlanTheBomb() {
            var task = new TaskCompletionSource<BombPvp>();
            _onBombButtonClicked = () => {
                _handle.Dispose();
                _onBombButtonClicked = null;
                if (!_levelView.CheckSpawnPvpBomb(_participantSlot)) {
                    return;
                }
                var hero = LevelView.GetPvpHeroBySlot(_participantSlot);
                var id = hero.Bombable.GetNextBombId();
                var bomb = hero.SpawnBomb(id, 2, hero.GetTileLocation());
                bomb.SetCountDownEnable(false);
                task.SetResult(bomb);
            };
            _handle.AddObserver(guiPvp, new BLGuiObserver() { SpawnBomb = () => { _onBombButtonClicked(); } });
            return task.Task;
        }

        public BombPvp PlanTheBomb(int tileX, int tileY, int slot = 0) {
            if (!_levelView.CheckSpawnPvpBomb(slot)) {
                return null;
            }
            var hero = LevelView.GetPvpHeroBySlot(slot);
            var id = hero.Bombable.GetNextBombId();
            var bomb = hero.SpawnBomb(id, 2, new Vector2Int(tileX, tileY));
            bomb.SetCountDownEnable(false);
            return bomb;
        }

        public void ExplodeBombTutorial(BombPvp bomb) {
            bomb.StartExplode(bomb.transform.localPosition);
        }

        public Transform ButtonBombTransform => guiPvp.ButtonBombTransform;

        public Transform ButtonShieldTransform => guiPvp.ButtonShieldTransform;

        public Transform ButtonKeyTransform => guiPvp.ButtonKeyTransform;

        public IBLInputKey PlayerInput => guiPvp.Input;

        public void SetEnableInputMove(bool isEnable) {
            _enableMove = isEnable;
        }

        public void SetEnableInputPlantBomb(bool isEnable) {
            _enablePlantBomb = isEnable;
            guiPvp.SetEnableInputPlantBomb(isEnable);
        }

        public void SetEnableInputShield(bool isEnable) {
            guiPvp.SetEnableInputShield(isEnable);
        }

        public void SetEnableInputKey(bool isEnable) {
            guiPvp.SetEnableInputKey(isEnable);
        }

        public void AddItemToPlayer(int slot, HeroItem item, int value) {
            _levelView.AddItemToPlayer(_participantSlot == slot, slot, item, value);
        }

        public Text GetUiTextItem(ItemType item) {
            return guiPvp.GetUiTextItem(item);
        }

        public void UpdateHealthUi(int slot, int value) {
            guiPvp.UpdateHealthUi(slot, value);
        }

        public void SetPlayerInJail(int slot = 0) {
            var hero = LevelView.GetPvpHeroBySlot(slot);
            hero.SetPlayerInJail();
        }

        public void SetEnemyInJail() {
            var pvpHeroes = _levelView.GetPvpHeroes();
            for (var slot = 0; slot < pvpHeroes.Count; slot++) {
                if (slot == _participantSlot) {
                    continue;
                }
                pvpHeroes[slot].SetPlayerInJail();
            }
        }

        public void KillEnemy() {
            var pvpHeroes = _levelView.GetPvpHeroes();
            for (var slot = 0; slot < pvpHeroes.Count; slot++) {
                if (slot == _participantSlot) {
                    continue;
                }
                pvpHeroes[slot].KillHero(slot);
            }
        }

        public void KillPlayer() {
            _levelView.GetPvpHeroBySlot(_participantSlot).KillHero(_participantSlot);
        }

        public void SetPlayerPos(int slot, Vector2Int locations) {
            _levelView.GetPvpHeroBySlot(slot)
                .ForceToPosition(_levelView.EntityManager.MapManager.GetTilePosition(locations));
        }

        public void RevivePlayer(int slot, Vector2Int locations) {
            if (_levelView.GetPvpHeroBySlot(slot).IsAlive) {
                _levelView.GetPvpHeroBySlot(slot)
                    .ForceToPosition(_levelView.EntityManager.MapManager.GetTilePosition(locations));
                return;
            }
            var playerData = _levelView.EntityManager.PlayerManager.GetPlayerDataRaw(slot);
            _levelView.EntityManager.PlayerManager.AddPlayer(locations, playerData, slot, false);
            if (slot == _participantSlot) {
                levelScene.ProCamera.SetTarget(_levelView.GetPvpHeroBySlot(_participantSlot));
            }
            StopMusicWhenDie();
        }

        private void StopMusicWhenDie() {
            var pvpHeroes = _levelView.GetPvpHeroes();
            foreach (var player in pvpHeroes) {
                player.StopMusicWhenDie = false;
            }
        }

        public void ResetPlayerMainUpgrade(Vector2Int locations) {
            var slot = _participantSlot;
            if (_levelView.GetPvpHeroBySlot(slot).IsAlive) {
                _levelView.GetPvpHeroBySlot(slot).Kill(false);
            }
            var playerData = _levelView.EntityManager.PlayerManager.GetPlayerDataRaw(slot);
            playerData.skinChests[(int) SkinChestType.Bomb] = new List<int>() { 49 };
            playerData.skinChests[(int) SkinChestType.Avatar] = new List<int>() { 64 };
            playerData.skinChests[(int) SkinChestType.Explosion] = new List<int>() { 76 };
            playerData.skinChests[(int) SkinChestType.Trail] = new List<int>() { 125 };
            playerData.bombSkin = 49;
            playerData.explosionSkin = 76;
            playerData.bombDamage = 5;
            playerData.bombRange = 3;
            playerData.speed = 5;
            playerData.bombNum = 2;
            playerData.hp = 2;
            // Sync Ui
            guiPvp.SetQuantityBomb(2);
            guiPvp.UpdateBombNumUi(2);
            guiPvp.UpdateDamageUi(5);
            guiPvp.UpdateFireUi(3);
            guiPvp.UpdateSpeedUi(5);
            _levelView.EntityManager.PlayerManager.AddPlayer(locations, playerData, slot, false);
            levelScene.ProCamera.SetTarget(_levelView.GetPvpHeroBySlot(slot));
            StopMusicWhenDie();
        }

        public void SetEnemyMaxSpeedBot(int maxSpeed) {
            var pvpHeroes = _levelView.GetPvpHeroes();
            for (var slot = 0; slot < pvpHeroes.Count; slot++) {
                if (slot == _participantSlot) {
                    continue;
                }
                pvpHeroes[slot].ForceSetMaxSpeed(maxSpeed);
            }
        }

        public void AddBrick(int i, int j) {
            LevelView.MapManager.PaintBrick(i, j);
        }

        public void AddItemUnderBrick(int i, int j, ItemType itemType) {
            LevelView.MapManager.PaintBrick(i, j);
            LevelView.MapManager.SetItemUnderBrick(itemType, new Vector2Int(i, j));
        }

        public Task<bool> WaitHeroUseKey() {
            var task = new TaskCompletionSource<bool>();
            _checkCondition = () => {
                if (Input.GetKeyDown(_hotkeyCombo.GetControl(ControlKey.UseItem)) ||
                    _inputManager.ReadButton(_inputManager.InputConfig.UseBooster)) {
                    _onKeyButtonClicked?.Invoke();
                }
            };
            _onKeyButtonClicked = () => {
                _handle.Dispose();
                _onKeyButtonClicked = null;
                _checkCondition = null;
                var hero = LevelView.GetPvpHeroBySlot(_participantSlot);
                hero.JailBreak();
                task.SetResult(true);
            };

            _handle.AddObserver(guiPvp, new BLGuiObserver() {
                    UseBooster = (type) => {
                        if (type == BoosterType.Key) {
                            _onKeyButtonClicked();
                        }
                    }
                });

            return task.Task;
        }

        public Task<bool> WaitHeroUseShield() {
            var task = new TaskCompletionSource<bool>();
            _checkCondition = () => {
                if (Input.GetKeyDown(_hotkeyCombo.GetControl(ControlKey.UseItem)) ||
                    _inputManager.ReadButton(_inputManager.InputConfig.UseBooster)) {
                    _onShieldButtonClicked?.Invoke();
                }
            };
            _onShieldButtonClicked = () => {
                _handle.Dispose();
                _onShieldButtonClicked = null;
                _checkCondition = null;
                var hero = LevelView.GetPvpHeroBySlot(_participantSlot);
                hero.SetShield(2.5f, null);
                task.SetResult(true);
            };

            _handle.AddObserver(guiPvp, new BLGuiObserver() {
                    UseBooster = (type) => {
                        if (type == BoosterType.Shield) {
                            _onShieldButtonClicked();
                        }
                    }
                });

            return task.Task;
        }

        public void UpdateBoostersNum(int key, int shield) {
            levelScene.UpdateBoostersNum(key, shield);
        }

        public Task<bool> WaitClashBot([CanBeNull] GameObject handTouch = null) {
            var task = new TaskCompletionSource<bool>();
            levelScene.InitForSimulator();
            levelScene.CreateSyncHeroes();
            levelScene.UpdateBoostersInfinity();
            levelScene.SimulatorSyncSyncHero();
            var guiPvpBot = bot.GetComponent<BLGuiPvpBot>();
            guiPvpBot.Initialized();
            // levelScene.StartCountDown.StartCount(() => {
            //     levelScene.enabled = true;
            //     bot.SetActive(true);
            // });
            levelScene.enabled = true;
            bot.SetActive(true);
            if (handTouch) {
                handTouch.SetActive(false);
            }
            var timeLimit = Epoch.GetUnixTimestamp(TimeSpan.TicksPerMillisecond) + 20000;
            _checkCondition = () => {
                if (Epoch.GetUnixTimestamp(TimeSpan.TicksPerMillisecond) > timeLimit) {
                    guiPvpBot.Suicide();
                }
                if (LevelView.GetPvpHeroBySlot(_participantSlot).IsInJail) {
                    // main char is in-jail, wait use key
                    if (handTouch) {
                        handTouch.SetActive(true);
                    }
                    return;
                }
                if (handTouch) {
                    handTouch.SetActive(false);
                }
                if (LevelView.GetPvpHeroes().Any(hero => !hero.IsAlive)) {
                    bot.SetActive(false);
                    _checkCondition = null;
                    levelScene.enabled = false;
                    RemoveAllBrickAndItem();
                    task.SetResult(true);
                }
            };
            return task.Task;
        }

        public void GenRanBrick() {
            var mapManager = LevelView.MapManager;
            var col = mapManager.Col;
            var row = mapManager.Row;
            for (var x = 0; x < col; x++) {
                for (var y = 0; y < row; y++) {
                    if (mapManager.IsItem(x, y)) {
                        LevelView.MapManager.RemoveItem(x, y);
                    }
                    if (!mapManager.IsEmpty(x, y)) {
                        continue;
                    }
                    if (x <= 4 && y >= row - 3) {
                        continue;
                    }
                    if (x >= col - 2 && y <= 1) {
                        continue;
                    }
                    if (Random.Range(0, 100) <= 40) {
                        LevelView.MapManager.PaintBrick(x, y);
                    }
                }
            }
        }

        public void RemoveAllBrickAndItem() {
            var mapManager = LevelView.MapManager;
            var bombs = LevelView.EntityManager.FindEntities<BombPvp>();
            for (var idx = bombs.Count - 1; idx >= 0; idx--) {
                bombs[idx].DestroyMe();
            }
            var col = mapManager.Col;
            var row = mapManager.Row;
            for (var x = 0; x < col; x++) {
                for (var y = 0; y < row; y++) {
                    if (mapManager.IsBrick(x, y)) {
                        LevelView.MapManager.RemoveBrick(x, y);
                    } else if (mapManager.IsItem(x, y)) {
                        LevelView.MapManager.RemoveItem(x, y);
                    }
                }
            }
        }

        public void GenRanItemUnderBrick() {
            var mapManager = LevelView.MapManager;
            var col = mapManager.Col;
            var row = mapManager.Row;
            for (var x = 0; x < col; x++) {
                for (var y = 0; y < row; y++) {
                    if (!mapManager.IsBrick(x, y)) {
                        continue;
                    }
                    var ran = Random.Range(0, 100);
                    switch (ran) {
                        case >= 40:
                            continue;
                        case < 10:
                            mapManager.SetItemUnderBrick(ItemType.FireUp, new Vector2Int(x, y));
                            break;
                        case < 25:
                            mapManager.SetItemUnderBrick(ItemType.BombUp, new Vector2Int(x, y));
                            break;
                        default:
                            mapManager.SetItemUnderBrick(ItemType.Boots, new Vector2Int(x, y));
                            break;
                    }
                }
            }
        }

        public void HideTimer() {
            guiPvp.HideTimer();
        }

        public void HideAllElementGui() {
            var hiddenGuis = new[] {
                ElementGui.Joystick, //
                ElementGui.ButtonBomb, //
                ElementGui.ButtonKey, //
                ElementGui.ButtonShield, //
                ElementGui.StatDamage, //
                ElementGui.StatBombUp, //
                ElementGui.StatFireUp, //
                ElementGui.StatBoots, //
                ElementGui.Timer, //
                ElementGui.HeroGroup, //
                ElementGui.MatchId, //
                ElementGui.BtBack, //
                ElementGui.ButtonEmoji, //
            };
            foreach (var e in hiddenGuis) {
                guiPvp.SetVisible(e, false);
            }
        }

        public void ShowElementGui(ElementGui e) {
            guiPvp.SetVisible(e, true);
        }

        public void HideElementGui(ElementGui e) {
            guiPvp.SetVisible(e, false);
        }

        public void StopGame() {
            levelScene.enabled = false;
            bot.SetActive(false);
            // ServiceLocator.Resolve<ISoundManager>().PauseMusic();
        }

        public virtual void FinishTutorialInGame() { }
    }
}