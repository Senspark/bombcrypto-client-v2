using System;
using System.Collections.Generic;

using Animation;

using App;

using Engine.Entities;
using Engine.Manager;
using Engine.Strategy.Provider;
using Engine.Strategy.Spawner;
using Engine.Strategy.Weapon;
using Engine.Utils;

using Senspark;

using UnityEngine;

using Random = UnityEngine.Random;

namespace Engine.Components {
    public class BotManager : MonoBehaviour {
        public GameObject zZ;
        public bool IsSleeping { get; private set; }
        private bool shouldFindTarget = true;

        private IPveHeroStateManager _pveHeroStateManager;
        public IMapManager mapManager;
        private ILevelManager levelManager;
        private Player player;
        private Bombable bombable;
        private BotMove botMove;
        private BotDestroyBrick botDestroyBrick;
        private Vector2Int checkingLocation;
        private WalkThrough walkThrough;
        private HeroAnimator _heroAnimation;
        private Movable _movable;
        private IPlayerStorageManager _playerStoreManager;
        public TileType[,] map;
        public Vector2Int currentLocation, targetLocation;
        public List<Vector2Int> reachableLocationList, safeLocationList, nearestLocationList;
        public NotPathLocations notPathToLocationList;

        private const float MIN_DISTANCE = 10f;

        #region UNITY EVENTS

        private void Awake() {
            _pveHeroStateManager = ServiceLocator.Instance.Resolve<IPveHeroStateManager>();
            _playerStoreManager = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
            player = GetComponent<Player>();
            player.GetEntityComponent<Updater>()
                .OnBegin(Init)
                .OnUpdate(OnUpdate);
        }

        private void Init() {
            bombable = player.GetEntityComponent<Bombable>();
            botMove = GetComponent<BotMove>();
            botDestroyBrick = GetComponent<BotDestroyBrick>();
            walkThrough = player.GetEntityComponent<WalkThrough>();
            _heroAnimation = GetComponent<HeroAnimator>();
            _movable = player.GetEntityComponent<Movable>();
            levelManager = player.EntityManager.LevelManager;
            mapManager = player.EntityManager.MapManager;
            currentLocation = mapManager.GetTileLocation(transform.localPosition);
            map = mapManager.GetTileTypeMap();

            reachableLocationList = new List<Vector2Int>();
            safeLocationList = new List<Vector2Int>();
            nearestLocationList = new List<Vector2Int>();
            notPathToLocationList = new NotPathLocations(10);

            player.Health.SetOnTakeDamage(OnTakeDamage);

            SetupWeapon();

            GetReachableLocationList();
            GetSafeLocationList();

#if UNITY_EDITOR
            name = player.HeroId.ToString();
#endif
        }

        private void SetupWeapon() {
            var weapon = new BombWeapon(WeaponType.Bomb,
                new BombSpawner(),
                bombable);
            bombable.SetWeapon(weapon);
            bombable.SetSpawnLocation(GetSpawnLocation);
        }

        private void OnUpdate(float delta) {
            if (levelManager.IsCompleted) {
                return;
            }

            if (IsSleeping) {
                return;
            }

            var localPosition = transform.localPosition;
            currentLocation = mapManager.GetTileLocation(localPosition);

            // fix 
            if (mapManager.IsWallOrOutSide(currentLocation.x, currentLocation.y)) {
                var safeLocation = safeLocationList[0];
                var tilePosition = mapManager.GetTilePosition(safeLocation);
                transform.localPosition = tilePosition;
            }

            if (IsCenterTheTile()) {
                if (currentLocation == targetLocation) {
                    if (!CheckToSpawnBomb()) {
                        notPathToLocationList.Add(targetLocation);
                    }

                    if (shouldFindTarget) {
                        ChooseNextTarget();
                    }
                } else {
                    if (isNoneTarget || botDestroyBrick.HasBrickAround(targetLocation)) {
                        botMove.MoveToTargetLocation();
                    } else {
                        ChooseNextTarget();
                    }
                }
            } else {
                if (isNoneTarget || botDestroyBrick.HasBrickAround(targetLocation)) {
                    botMove.MoveToTargetLocation();
                } else {
                    ChooseNextTarget();
                }
            }
        }

        private void OnTakeDamage(float hp, DamageFrom damageFrom) {
            var playerData = _playerStoreManager.GetPlayerDataFromId(player.HeroId);
            if (playerData != null) {
                playerData.hp = hp;
            }

            if (hp < 1 && damageFrom == DamageFrom.BombExplode) {
                GoToSleep_SendRequest();
            }
        }

        #endregion

        #region PUBLIC METHODS

        public void ForceSleep() {
            IsSleeping = true;
            zZ.gameObject.SetActive(true);
            _movable.ForceStop();
            _heroAnimation.PlaySleep();
        }

        public void GoToSleep_SendRequest() {
            ForceSleep();
            var data = _playerStoreManager.GetPlayerDataFromId(player.HeroId);
            if (data != null && data.stage == HeroStage.Working) {
                _pveHeroStateManager.ChangeHeroState(player.HeroId, HeroStage.Sleep);
            }
        }

        public void ForceWork() {
            IsSleeping = false;
            zZ.gameObject.SetActive(false);
            _heroAnimation.PlayWork();
        }

        public bool IsCenterTheTile() {
            var localPosition = transform.localPosition;
            currentLocation = mapManager.GetTileLocation(localPosition);
            var tilePosition = mapManager.GetTilePosition(currentLocation);
            return DistanceXY.Equal(localPosition, tilePosition);
        }

        public bool SpawnBomb() {
            if (mapManager.IsEmpty(currentLocation)) {
                if (bombable.Spawn()) {
                    mapManager.SetHadBomb(currentLocation);
                    player.HadOutOfBomb = false;
                    if (mapManager.NumberOfBlock == 1) {
                        shouldFindTarget = false;
                    }
                    return true;
                }
            }
            return false;
        }

        public float GetBlockValue(int i, int j) {
            if (mapManager.TryGetBlock(i, j, out var block)) {
                return block ? block.GetValue() : 0;
            } else {
                return 0;
            }
        }

        public bool isNoneTarget = false;

        public void ChooseNextTarget() {
            GetReachableLocationList();
            GetSafeLocationList();

            botDestroyBrick.isMovingToTarget = false;
            botMove.Waiting();
            isNoneTarget = false;
            if (!botDestroyBrick.FindRandomTileNearBrick(currentLocation, mapManager)) {
                ChooseNextEmptyLocation();
            }
        }

        #endregion

        #region PRIVATE METHODS

        private Vector2Int GetSpawnLocation() {
            return currentLocation;
        }

        private bool CheckToSpawnBomb() {
            GetReachableLocationList();
            GetSafeLocationList();

            // if (isNoneTarget) {
            //     return false;
            // }

            if (mapManager.HadBomb(currentLocation)) {
                return false;
            }

            if (!player.EntityManager.PlayerManager.CanPlaceBomb(player.HeroId)) {
                return false;
            }

            if (botDestroyBrick.HasBrickAround(currentLocation)) {
                return SpawnBomb();
            }

            return false;
        }

        private void GetReachableLocationList() {
            reachableLocationList.Clear();
            CheckReachableLocation();
        }

        private void CheckReachableLocation() {
            var col = map.GetLength(0);
            var row = map.GetLength(1);

            for (var i = 0; i < col; i++) {
                for (var j = 0; j < row; j++) {
                    if (GetBlockValue(i, j) > 0) {
                        if (i > 0 && IsReachable(i - 1, j)) {
                            reachableLocationList.Add(new Vector2Int(i - 1, j));
                        }
                        if (i < col - 1 && IsReachable(i + 1, j)) {
                            reachableLocationList.Add(new Vector2Int(i + 1, j));
                        }
                        if (j > 0 && IsReachable(i, j - 1)) {
                            reachableLocationList.Add(new Vector2Int(i, j - 1));
                        }
                        if (j < row - 1 && IsReachable(i, j + 1)) {
                            reachableLocationList.Add(new Vector2Int(i, j + 1));
                        }
                    }
                }
            }
        }

        private bool IsReachable(int i, int j) {
            if (map[i, j] == TileType.Background) {
                return true;
            }
            return false;
        }

        private void GetSafeLocationList() {
            safeLocationList.Clear();

            foreach (var reachLocation in reachableLocationList) {
                if (notPathToLocationList.Contains(reachLocation)) {
                    continue;
                }

                //if (map[reachLocation.x, reachLocation.y] == TileType.Background
                //    && mapExplosion[reachLocation.x, reachLocation.y] <= 0)
                //if (map[reachLocation.x, reachLocation.y] == TileType.Background)
                {
                    safeLocationList.Add(reachLocation);
                }
            }

            ShuffleSafeLocations();
        }

        private void ShuffleSafeLocations() {
            for (var i = 0; i < safeLocationList.Count; i++) {
                var random = Random.Range(0, safeLocationList.Count);
                (safeLocationList[i], safeLocationList[random]) = (safeLocationList[random], safeLocationList[i]);
            }

            //sort theo khoang cach dua ve cuoi cac diem qua gan
            safeLocationList.Sort((item1, item2) =>
                CompareDistance(item1, item2)
            );
        }

        private int CompareDistance(Vector2Int v1, Vector2Int v2) {
            var dist1 = DistanceXY.DistancePower2(v1, currentLocation);
            var dist2 = DistanceXY.DistancePower2(v2, currentLocation);

            if (dist1 < MIN_DISTANCE) {
                dist1 += 1000000;
            }
            if (dist2 < MIN_DISTANCE) {
                dist2 += 1000000;
            }

            return (int) (dist1 - dist2);
        }

        private void ChooseNextEmptyLocation() {
            isNoneTarget = true;
            targetLocation = mapManager.GetNearestEmptyLocation(currentLocation);
        }

        public void ChooseFarthestEmptyLocation() {
            isNoneTarget = true;
            var grid = mapManager.GetMapGrid(walkThrough.ThroughBrick, walkThrough.ThroughBomb,
                walkThrough.ThroughWall);
            targetLocation = FloodFill.FindFarthestMovableCoordinate(grid, currentLocation);
        }

        #endregion

        public class NotPathLocations {
            private readonly HashSet<Vector2Int> _locations;
            private readonly int _maxItem;

            // public bool IsReachMaximum => _locations.Count;

            public NotPathLocations(int maxItem) {
                _maxItem = maxItem;
                _locations = new HashSet<Vector2Int>(maxItem);
            }

            public bool Add(Vector2Int location) {
                var isMaximum = false;
                if (_locations.Count >= _maxItem) {
                    _locations.Clear();
                    isMaximum = true;
                }
                _locations.Add(location);
                return isMaximum;
            }

            public bool Contains(Vector2Int location) {
                return _locations.Contains(location);
            }
        }
    }
}