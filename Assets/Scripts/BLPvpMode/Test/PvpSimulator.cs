using System;
using System.Collections.Generic;

using BLPvpMode.Engine.Entity;

using Engine.Entities;
using Engine.Manager;

using PvpMode.Entities;

using Scenes.TutorialScene.Scripts;

using UnityEngine;

namespace BLPvpMode.Test {
    public class PvpSimulator {
        private class SimPlayer {
            public PlayerPvp Player { get; }
            public Vector2Int Location { get; private set; }
            public bool IsKill { get; set; } = false;

            public bool IsNewLocation() {
                var location = Player.GetTileLocation();
                if (Location == location) {
                    return false;
                }
                Location = location;
                return true;
            }

            public SimPlayer(PlayerPvp playerPvp) {
                Player = playerPvp;
                Location = playerPvp.GetTileLocation();
            }
        }

        private class PrisonCounter {
            private const float JailDuration = 5f;

            private int _slot;
            private System.Action<int> _callback;
            private bool _isCounting;
            private float _jailDuration;
            private float _jailElapse = 0.0f;

            public void StartCount() {
                if (_callback != null) {
                    _isCounting = true;
                    _jailElapse = 0;
                }
            }

            public PrisonCounter(int slot, Action<int> callback) {
                _slot = slot;
                _callback = callback;
                _jailDuration = JailDuration;
            }

            public void Step(float delta) {
                if (!_isCounting) {
                    return;
                }
                _jailElapse += delta;
                if (_jailElapse >= _jailDuration) {
                    _callback(_slot);
                    _isCounting = false;
                }
            }
        }

        private readonly BLLevelScenePvpSimulator _levelScene;
        private readonly List<SimPlayer> _heroes;
        private Dictionary<int, PrisonCounter> _counters = new Dictionary<int, PrisonCounter>();

        public PvpSimulator(BLLevelScenePvpSimulator levelScene) {
            _levelScene = levelScene;
            _heroes = new List<SimPlayer>();
        }

        public void SyncHero() {
            var pvpHeroes = _levelScene.LevelView.GetPvpHeroes();
            _heroes.Clear();
            foreach (var iter in pvpHeroes) {
                _heroes.Add(new SimPlayer(iter));
            }
        }

        public void Step(float delta) {
            foreach (var counter in _counters.Values) {
                counter.Step(delta);
            }

            var bombs = _levelScene.LevelView.EntityManager.FindEntities<BombPvp>();
            foreach (var bomb in bombs) {
                bomb.UpdateCountDown(delta);
            }

            for (var i = 0; i < _heroes.Count; i++) {
                var hero = _heroes[i];
                if (!hero.Player.IsAlive) {
                    return;
                }
                if (hero.Player.IsInJail) {
                    //trong tutorial nhân vật chính không bị giết khi trong tù.
                    if (i == _levelScene.ParticipantSlot) {
                        return;
                    }
                    CheckOtherKillHeroInJail(i, hero);
                    return;
                }
                if (hero.IsNewLocation()) {
                    CheckTakeItem(i, hero);
                }
            }
        }

        private void CheckTakeItem(int slot, SimPlayer hero) {
            var location = hero.Location;
            var tileType = _levelScene.GetTileType(location.x, location.y);
            if (tileType != TileType.Item) {
                return;
            }
            var itemType = _levelScene.GetItemType(location.x, location.y);
            var rewardValue = itemType switch {
                ItemType.GoldX1 => 10,
                ItemType.GoldX5 => 50,
                _ => 1
            };
            var heroItem = itemType switch {
                ItemType.BombUp => HeroItem.BombUp,
                ItemType.FireUp => HeroItem.FireUp,
                ItemType.Boots => HeroItem.Boots,
                ItemType.GoldX1 => HeroItem.Gold,
                ItemType.GoldX5 => HeroItem.Gold,
            };
            _levelScene.SetItemToPlayer(slot, rewardValue, heroItem);
            _levelScene.RemoveItemOnMap(location);
        }

        private void CheckOtherKillHeroInJail(int slot, SimPlayer hero) {
            if (hero.IsKill) {
                return;
            }
            for (var i = 0; i < _heroes.Count; i++) {
                if (i == slot) {
                    continue;
                }
                var other = _heroes[i];
                if (other.IsKill) {
                    continue;
                }
                if (!other.Player.IsAlive) {
                    continue;
                }
                if (other.Player.IsInJail) {
                    continue;
                }
                if (hero.Location != other.Location) {
                    continue;
                }
                EndPrison(slot);
                other.IsKill = true;
                break;
            }
        }

        private void EndPrison(int slot) {
            var player = _heroes[slot].Player;
            if (!player.IsAlive) {
                return;
            }
            player.KillHero(slot);
        }

        public void AddPrisonCounter(int slot) {
            if (_levelScene.ParticipantSlot == slot) {
                return;
            }
            if (!_counters.ContainsKey(slot)) {
                _counters[slot] = new PrisonCounter(slot, EndPrison);
            }
            _counters[slot].StartCount();
        }

        public void SetHealth(int slot, float value) {
            var hero = _heroes[slot];
            var max = hero.Player.Health.GetMaxHealth();
            var hp = Math.Min(value, max);
            hero.Player.Health.SetCurrentHealth(hp);
            _levelScene.OnUpdateHealthUi(slot, (int) hp);
        }

        public void TakeDamageFromExplosions(List<CellExplosion> cellExplosions) {
            BombTakeDamageFromCellExplosion(cellExplosions);
            for (var i = 0; i < _heroes.Count; i++) {
                var hero = _heroes[i];
                HeroTakeDamageFromCellExplosions(i, hero.Player, cellExplosions);
            }
        }

        private void BombTakeDamageFromCellExplosion(List<CellExplosion> cellExplosions) {
            foreach (var iter in cellExplosions) {
                var tileType = _levelScene.GetTileType(iter.Location.x, iter.Location.y);
                if (tileType == TileType.Bomb) {
                    _levelScene.ExplodeBomb(iter.Location);
                }
            }
        }

        private void HeroTakeDamageFromCellExplosions(int slot, PlayerPvp hero,
            List<CellExplosion> cellExplosions) {
            if (!hero.IsAlive) {
                return;
            }
            CellExplosion cellExEffect = null;
            var location = hero.GetTileLocation();
            foreach (var iter in cellExplosions) {
                if (location != iter.Location) {
                    continue;
                }
                cellExEffect = iter;
                break;
            }
            if (cellExEffect == null) {
                return;
            }
            if (hero.Immortal) {
                _levelScene.SetShielded(slot, false, HeroEffectReason.Damaged);
                return;
            }
            if (hero.IsInJail) {
                return;
            }
            var hp = hero.Health.GetCurrentHealth();
            hp -= cellExEffect.Damage;
            SetHealth(slot, hp);
            if (hp <= 0) {
                _levelScene.SetImprisoned(slot, true);
            }
        }
    }
}