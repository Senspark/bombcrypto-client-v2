using System.Collections.Generic;
using Actions;
using BLPvpMode.Engine.Entity;
using Engine.Components;
using Engine.Entities;
using Engine.Strategy.Spawner;
using Engine.Strategy.Weapon;
using JetBrains.Annotations;
using PvpMode.Services;
using UnityEngine;
using UnityEngine.Assertions;

namespace PvpMode.Entities {
    public class PlayerPvp : Player {
        [SerializeField]
        private GameObject meArrow;

        [SerializeField]
        private GameObject AuthorizedPosition;

        private bool _isShowMe = false;
        public FactionType Faction { get; private set; }

        private Vector2Int _spawnLocation;

        protected override void InitMoveComponents() {
            var callback = GenerateMovableCallback();
            callback.SetActiveReverseIcon = (value) => {
                if (reverseIcon != null) {
                    reverseIcon.SetActive(value);
                }
            };
            callback.SetAuthorizedPosition = SetAuthorizedPosition;
            
            Movable = new PvpMovable(this);
            WalkThrough = new WalkThrough(this.Type, Movable);
            Movable.Init(WalkThrough, callback);
            AddEntityComponent<WalkThrough>(WalkThrough);
            AddEntityComponent<Movable>(Movable);
        }
        
        private void SetAuthorizedPosition(Vector2 position) {
            AuthorizedPosition.transform.localPosition = position;
        }
        
        protected override void SetupWeapon() {
            var weapon = new BombWeapon(WeaponType.Bomb,
                new BombSpawner(),
                Bombable);
            Bombable.SetWeapon(weapon);
            Bombable.SetSpawnLocation(() => _spawnLocation);
        }

        public void SetSlotAndFaction(int slot, FactionType factionType) {
            Slot = slot;
            Faction = factionType;
        }

        public void ShowMe(bool value) {
            _isShowMe = value;
            meArrow.SetActive(value);
        }

        public bool KillHero(int slot) {
            if (slot != Slot) {
                return false;
            }
            JailBreakBeforeDie();
            return true;
        }

        public void ForceToPosition(Vector3 position) {
            Movable.ForceToPosition(position);
        }

        public override Vector3 GetPosition() {
            return Movable.Position;
        }

        public override Vector2Int GetTileLocation() {
            return EntityManager.MapManager.GetTileLocation(Movable.Position);
        }

        public BombPvp SpawnBomb(int id, int range, Vector2Int position) {
            _spawnLocation = position;
            return Bombable.PvpForceSpawn(id, range);
        }

        public void ExplodeBomb(float posX, float posY, (int, int)[] brokenList, bool isShaking = false) {
            Bombable.ForceExplode(posX, posY, brokenList, isShaking);
        }

        public void ExplodeBomb(int id, [NotNull] Dictionary<Direction, int> ranges, bool isShaking) {
            Bombable.ForceExplode(id, ranges, isShaking);
        }

        public void RemoveBomb(int id) {
            Bombable.RemoveBomb(id);
        }

        public void SetPlayerInJail() {
            SetInJail(-1, null);
        }

        public void AddItem(bool playSound, HeroItem item, int value) {
            switch (item) {
                case HeroItem.BombUp: {
                    // When streaming mid-game, value may > 1.
                    SetItem(ItemType.BombUp, 0, playSound);
                    break;
                }
                case HeroItem.FireUp: {
                    // When streaming mid-game, value may > 1.
                    SetItem(ItemType.FireUp, 0, playSound);
                    break;
                }
                case HeroItem.Boots: {
                    // When streaming mid-game, value may > 1.
                    SetItem(ItemType.Boots, 0, playSound);
                    break;
                }
                case HeroItem.Gold:
                case HeroItem.BronzeChest:
                case HeroItem.SilverChest:
                case HeroItem.GoldChest:
                case HeroItem.PlatinumChest: {
                    Assert.IsTrue(value > 0);
                    SetItem(ItemType.GoldX1, value, playSound);
                    break;
                }
            }
        }

        public void StartSkullHeadEffect(bool playSound, HeroEffect effect, int duration) {
            base.StartSkullHeadEffect(effect, duration, playSound);
        }
    }
}