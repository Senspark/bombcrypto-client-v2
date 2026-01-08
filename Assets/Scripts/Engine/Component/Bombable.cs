using System;
using System.Collections.Generic;
using System.Linq;

using App;

using Engine.Entities;
using Engine.Strategy.Weapon;

using JetBrains.Annotations;

using PvpMode.Entities;
using PvpMode.Services;

using UnityEngine;
using UnityEngine.Assertions;

namespace Engine.Components {
    using SpawnLocationCallback = Func<Vector2Int>;

    public class Bombable : Spawnable {
        public int MaxBombNumber { set; get; } = 1;
        public float Damage { set; get; } = 1;
        public int ExplosionLength { set; get; } = 10;
        private float TimeToExplode => 3.0f;

        public bool ThroughBrick { set; get; } = false;
        public bool TreasureHunter { set; get; } = false;
        public bool JailBreaker { set; get; } = false;

        private HeroId _heroId;
        private int _bombSkin;
        private int _explosionSkin;

        /// <summary>
        /// Active bombs.
        /// </summary>
        private List<Bomb> _bombs;

        private SpawnLocationCallback _spawnLocationCallback;
        private IWeapon _weapon;

        public Vector2Int SpawnLocation => _spawnLocationCallback?.Invoke() ?? Vector2Int.zero;
        public Entity Entity { get; }

        public Bombable(Entity entity) {
            Entity = entity;
            Entity.GetEntityComponent<Updater>()
                .OnBegin(Init)
                .OnUpdate(delta => { _weapon?.Update(delta); });
        }

        private void Init() {
            _bombs = new List<Bomb>();
        }

        public void SetHeroIdAndBombSkin(HeroId heroId, int bombSkin, int explosionSkin) {
            _heroId = heroId;
            _bombSkin = bombSkin;
            _explosionSkin = explosionSkin;
        }

        public bool Spawn() {
            if (!CanPlantBomb()) {
                return false;
            }
            var id = GetNextBombId();
            Spawn(id);
            return true;
        }

        private void Spawn(int id) {
            Assert.IsTrue(CanPlantBomb());
            float damage = 0;
            float damageJail = 0;
            if (Entity.EntityManager.LevelManager.IsStoryMode) {
                damage = Damage;
                damageJail = Damage;
            } else if (Entity.EntityManager.LevelManager.IsPvpMode) {
                damage = Damage;
                damageJail = Damage;
            } else {
                damage = Damage + (TreasureHunter ? 5 : 0);
                damageJail = Damage + (JailBreaker ? 5 : 0);
            }
            var bomb = (Bomb) _weapon.Spawn();
            bomb.Init(
                id,
                _heroId,
                _bombSkin,
                _explosionSkin,
                damage,
                damageJail,
                ExplosionLength,
                TimeToExplode,
                ThroughBrick,
                OnExplodedCallback);
            _bombs.Add(bomb);
        }

        public int GetNextBombId() {
            var occupiedIds = _bombs.Select(item => item.BombId).ToHashSet();
            for (var i = 0; i < MaxBombNumber; ++i) {
                if (!occupiedIds.Contains(i)) {
                    return i;
                }
            }
            return -1;
        }

        public bool CanPlantBomb() {
            return _bombs.Count < MaxBombNumber;
        }

        public int CountPlantedBomb() {
            return _bombs.Count;
        }

        public int CountAvailableBomb() {
            return MaxBombNumber - _bombs.Count;
        }
        
        public BombPvp PvpForceSpawn(int id, int range) {
            var bomb = (BombPvp) _weapon.Spawn();
            bomb.Init(
                id,
                _heroId,
                _bombSkin,
                _explosionSkin,
                0,
                0,
                range,
                -1,
                ThroughBrick,
                OnExplodedCallback);
            _bombs.Add(bomb);
            return bomb;
        }

        [CanBeNull]
        public Bomb FindBomb(float posX, float posY) {
            foreach (var bomb in _bombs) {
                if (bomb.IsAlive) {
                    // OK.
                } else {
                    // Dead.
                    continue;
                }
                var position = bomb.transform.localPosition;
                if (Mathf.Approximately(position.x, posX) &&
                    Mathf.Approximately(position.y, posY)) {
                    // OK.
                } else {
                    // Mismatched bomb.
                    continue;
                }
                return bomb;
            }
            return null;
        }

        /// <summary>
        /// Legacy.
        /// </summary>
        public void ForceExplode(float posX, float posY, (int, int)[] brokenList, bool isShaking = false) {
            var bomb = FindBomb(posX, posY);
            if (bomb != null) {
                bomb.ForceExplode(new Vector2(posX, posY), null, brokenList, isShaking);
            }
        }

        public void ForceExplode(int id, [NotNull] Dictionary<Direction, int> ranges, bool isShaking) {
            var items = _bombs.Where(item => item.BombId == id).ToList();
            if (items.Count == 0) {
                Debug.LogError($"ForceExplode: invalid bomb ID: {id}");
                return;
            }
            if (items.Count > 1) {
                Debug.LogError($"ForceExplode: multiple bomb found: {id}");
            }
            var item = items[0];
            item.ForceExplode(
                item.transform.localPosition,
                ranges,
                Array.Empty<(int, int)>(),
                isShaking);
        }

        public void RemoveBomb(int id) {
            var items = _bombs.Where(item => item.BombId == id).ToList();
            if (items.Count == 0) {
                Debug.LogError($"RemoveBomb: invalid bomb ID: {id}");
                return;
            }
            if (items.Count > 1) {
                Debug.LogError($"RemoveBomb: multiple bomb found: {id}");
            }
            var item = items[0];
            item.DestroyMe();
            _bombs.Remove(item);
        }

        public void SetBombsThroughAble(bool value) {
            foreach (var bomb in _bombs) {
                if (bomb.IsAlive) {
                    var collider = bomb.GetComponent<Collider2D>();
                    if (collider != null) {
                        collider.isTrigger = value;
                    }
                }
            }
        }

        private void OnExplodedCallback(Bomb bomb) {
            _bombs.Remove(bomb);
        }

        public Bombable SetWeapon(IWeapon weapon) {
            _weapon = weapon;
            return this;
        }

        public Bombable SetSpawnLocation(SpawnLocationCallback callback) {
            _spawnLocationCallback = callback;
            return this;
        }

        public int IncreaseExplosionLength() {
            ExplosionLength += 1;
            return ExplosionLength;
        }

        public int DecreaseExplosionLength() {
            ExplosionLength -= 1;
            if (ExplosionLength < 1) {
                ExplosionLength = 1;
            }
            return ExplosionLength;
        }

        public void ResetExplosionLength() {
            ExplosionLength = 1;
        }

        public int IncreaseMaxBombNumber() {
            MaxBombNumber += 1;
            return MaxBombNumber;
        }

        public int DecreaseMaxBombNumber() {
            MaxBombNumber -= 1;
            if (MaxBombNumber < 1) {
                MaxBombNumber = 1;
            }
            return MaxBombNumber;
        }

        public void ResetMaxBombNumber() {
            MaxBombNumber = 1;
        }
    }
}