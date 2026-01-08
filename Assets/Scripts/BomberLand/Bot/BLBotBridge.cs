#define MOVE_TO_CENTER
#if UNITY_EDITOR
#define LOG_BOT
#endif

using System;
using System.Collections.Generic;
using System.Linq;

using BLPvpMode.GameView;
using BLPvpMode.Manager;

using Cysharp.Threading.Tasks;

using Engine.Components;
using Engine.Utils;

using Game.UI;

using PvpMode.Entities;

using Senspark;

using UnityEngine;

namespace BomberLand.Bot {
    public class BLBotBridge : ObserverManager<BLGuiObserver>, IBLParticipantGui {
        private ICommandManager _commandManager;
        private PlayerPvp _hero;
        private PvpMovable _pvpMovable;
        private readonly Queue<Vector2Int> _path;
        private TimeSpan _thinking;
        private readonly int _slot;
        private ILevelViewPvp _view;
        private int _plantCounter;
        private int _lastStageCheck = 0;
        private int _lastCountBombEnemy = 0;
        private long _lastTimePlantBomb = 0;
        private Vector2Int _lastPosPlantBomb = new Vector2Int(-1, -1);
        private IBLBotUtil _botUtil;
        public long LastTimePlantBomb => _lastTimePlantBomb;

        public Queue<Vector2Int> Path => _path;

        public Vector2Int LastPosPlantBomb => _lastPosPlantBomb;

        public ILevelViewPvp View => _view;

        public PlayerPvp Player => _hero;

        public BLBotBridge(int slot) {
            _path = new Queue<Vector2Int>();
            _slot = slot;
            _plantCounter = 0;
            _lastStageCheck = 0;
        }

        public void Init(ICommandManager commandManager, int slot, ILevelViewPvp view) {
            _commandManager = commandManager;
            _hero = view.GetPvpHeroBySlot(slot);
            _pvpMovable = ((PvpMovable) _hero.Movable);
            _view = view;
            _botUtil = new BLBotUtil(_view.MapManager);
        }

        public Vector2 Direction { get; private set; }

        public Vector2 GetDirectionFromInput() {
            if (_hero.Movable.ReverseEffect) {
                return Direction * -1;
            }
            return Direction;
        }

        public bool IsAlive() {
            return _hero.IsAlive;
        }

        public bool HasTarget() {
            return _path.Count > 0;
        }

        public bool IsArrived() {
            return _path.Count == 0;
        }

        private static bool IsArrived(Vector3 src, Vector3 des) {
#if MOVE_TO_CENTER
            const float delta = .2f;
            var direction = des - src;
            return direction.sqrMagnitude < delta * delta;
#else
                return (int) src.x == (int) des.x && (int) src.y == (int) des.y;
#endif
        }

        public bool IsPlanted() {
            return _hero.Bombable.CountPlantedBomb() > 0 || _plantCounter > 0;
        }

        public bool IsThinking() {
            return _thinking > TimeSpan.Zero;
        }

        public bool Move() {
            var isMoveToNextTile = false;
            if (_path.Count > 0) {
                var des = _path.Peek();
                var pos = _hero.GetPosition();
                if (IsArrived(pos, _view.GetTilePosition(des.x, des.y))) {
                    Direction = Vector2.zero;
                    _path.Dequeue();
                    isMoveToNextTile = true;
                } else {
                    Direction = Normalize(_view.GetTilePosition(des.x, des.y) - pos);
                }
            } else {
                Direction = Vector2.zero;
            }
            return isMoveToNextTile;
        }

        private Vector2 Normalize(Vector3 value) {
            var x = Mathf.Abs(value.x) > Mathf.Abs(value.y);
            return new Vector2(x ? value.x : 0, x ? 0 : value.y);
        }

        public bool IsRecommendSpamBomb() {
            if (_hero.Bombable.CountPlantedBomb() >= _hero.Bombable.MaxBombNumber - 1) {
                return false;
            }
            if (_path.Count < 3) {
                return false;
            }
            var posCheck = _path.ElementAt(2);
            var posCurrent = GetTileLocation();
            if (posCheck.x == posCurrent.x || posCheck.y == posCurrent.y) {
                return false;
            }
            return true;
        }

        public void PlantBomb() {
            if (!_view.CheckSpawnPvpBomb(_slot)) {
                return;
            }
            _lastPosPlantBomb = _hero.GetTileLocation();
            _lastTimePlantBomb = Epoch.GetUnixTimestamp(TimeSpan.TicksPerMillisecond);
            _view.MapManager.MarkPlayerAddBomb(_lastPosPlantBomb, _hero.Bombable.ExplosionLength);
            UniTask.Void(async () => {
                try {
                    _plantCounter++;
                    await _commandManager.PlantBomb();
                } finally {
                    _plantCounter--;
                }
            });
        }

        public void Thinking(TimeSpan value) {
            _thinking = value;
        }

        public void Step(TimeSpan dt) {
            if (!_hero.IsAlive || _hero.IsInJail) {
                return;
            }
            _thinking -= dt;
        }

        public void Debug() {
#if LOG_BOT
            if (!_hero.IsAlive) {
                return;
            }
            if (_path.Count > 0) {
                _pvpMovable.ShowPositionDebug(new Vector2(0.5f, 0.5f) + _path.Last());
            }
#endif
        }

        public bool UpdateBombsite() {
            var paths = _botUtil.PathToNearestBrick(_hero.GetTileLocation());
            if (paths == null || paths.Count == 0) {
                return false;
            }
            if (!_botUtil.CheckPathIsSafeWithNewBomb(paths)) {
                return false;
            }
            UpdateTarget(paths);
            return true;
        }

        public bool HadUpdateStage() {
            var currentStage = _view.MapManager.GetStageCount();
            if (currentStage == _lastStageCheck) {
                return false;
            }
            // có thay đổi thêm map, có thể bomb mới được đặt
            _lastStageCheck = currentStage;
            var heroes = _view.GetPvpHeroes();
            var countBombEnemy = 0;
            foreach (var enemy in heroes) {
                if (enemy == _hero || !enemy.IsAlive) {
                    continue;
                }
                countBombEnemy += enemy.Bombable.CountPlantedBomb();
            }
            if (_lastCountBombEnemy == countBombEnemy) {
                return false;
            }
            if (_lastCountBombEnemy > countBombEnemy) {
                _lastCountBombEnemy = countBombEnemy;
                return false;
            }
            // bomb đối thủ mới đặt thêm
            _lastCountBombEnemy = countBombEnemy;
            return true;
        }

        public bool CheckCurrentIsSafe() {
            // kiểm tra đường đi cũ còn an toàn với bomb mới không
            var path = _path.Count > 0 ? _path.ToList() : new List<Vector2Int>() { _hero.GetTileLocation() };
            if (!_botUtil.CheckPathIsSafeWithNewBomb(path)) {
                return false;
            }
            // Still safe, no need update paths
            return true;
        }

        public void FindSafeZone(out bool isStandOnDangerous) {
            isStandOnDangerous = false;
            var path = _botUtil.PathToNearestSafeZone(_hero.GetTileLocation(), ref isStandOnDangerous);
            UpdateTarget(path);
        }

        public bool FindItemNearest() {
            var paths = _botUtil.PathToNearestItem(_hero.GetTileLocation());
            if (paths == null || paths.Count <= 0) {
                return false;
            }
            if (!_botUtil.CheckPathIsSafeWithNewBomb(paths)) {
                return false;
            }
            UpdateTarget(paths);
            return true;
        }

        public bool FindBombFarthest() {
            var paths = _botUtil.FindBombFarthest(_hero.GetTileLocation());
            if (paths == null || paths.Count <= 0) {
                return false;
            }
            UpdateTarget(paths);
            return true;
        }

        public bool FindEnemy() {
            var heroes = _view.GetPvpHeroes();
            foreach (var enemy in heroes) {
                if (enemy == _hero || !enemy.IsAlive) {
                    continue;
                }
                var paths = _botUtil.PathToNearestEnemy(_hero.GetTileLocation(),
                    enemy.GetTileLocation());
                if (paths == null || paths.Count <= 0) {
                    continue;
                }
                if (!_botUtil.CheckPathIsSafeWithNewBomb(paths)) {
                    return false;
                }
                UpdateTarget(paths);
                return true;
            }
            return false;
        }

        public bool IsCanPlantBomb() {
            return _hero.Bombable.CanPlantBomb();
        }

        public Vector2Int GetTileLocation() {
            return _hero.GetTileLocation();
        }

        private void UpdateTarget(List<Vector2Int> nodes) {
            if (nodes == null) {
                return;
            }
            nodes.Reverse();
            _path.Clear();
            foreach (var node in nodes) {
                _path.Enqueue(node);
            }
        }

        public bool IsAllEnemyDie() {
            var heroes = _view.GetPvpHeroes();
            foreach (var enemy in heroes) {
                if (enemy == _hero) {
                    continue;
                }
                if (enemy.IsAlive) {
                    return false;
                }
            }
            return true;
        }
    }
}