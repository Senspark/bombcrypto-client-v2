using System;
using System.Collections.Generic;

using Engine.Entities;
using Engine.Manager;
using Engine.Utils;

using UnityEngine;

namespace Engine.MapRenderer {
    public class PoolWallDropHelper {
        private static int CompareWallDrop(WallDrop w1, WallDrop w2) {
            return w1.Id - w2.Id;
        }

        private readonly IEntityManager _entityManager;
        private readonly int _tileIndex;
        private bool _isWallDropTakeDamage;
        private readonly Transform _tfForeground;
        private readonly List<WallDrop> _wallDrops = new();
        private readonly long[,] _timeWallDrop; // lưu thời gian sắp FallDown, để giúp Bot kiểm tra vùng an toàn

        public PoolWallDropHelper(IEntityManager entityManager, int col, int row,
            int tileIndex,
            Transform tfForeground) {
            _entityManager = entityManager;
            _tileIndex = tileIndex;
            _tfForeground = tfForeground;
            _isWallDropTakeDamage = false;
            _timeWallDrop = new long[col, row];
        }

        public void Init(int col, int row) {
            var endX = col - 1;
            var endY = row - 1;
            for (var i = 0; i <= endX; i++) {
                for (var j = 0; j <= endY; j++) {
                    CreateWallDrop();
                }
            }
        }

        public bool IsHasWallDrop(int i, int j, long timestamp) {
            return _timeWallDrop[i, j] > 0 && timestamp >= _timeWallDrop[i, j] - 2000;
        }

        public long LongTimeWallDrop(int i, int j) {
            return _timeWallDrop[i, j];
        }

        public void WallDropTakeDamage(int i, int j) {
            _isWallDropTakeDamage = true;
            // var wallDrops = _entityManager.FindEntities<WallDrop>();
            var wallDrops = _wallDrops;
            wallDrops.Sort(CompareWallDrop);
            var isJumpToEnd = true;
            foreach (var wall in wallDrops) {
                if (wall.IsAlive) {
                    if (isJumpToEnd) {
                        wall.JumpToEnd();
                    } else {
                        wall.StopFallDown();
                    }
                }
                if (wall.Location.x == i && wall.Location.y == j) {
                    isJumpToEnd = false;
                }
            }
        }

        public WallDrop CreateWallDrop(int index, int i, int j, Vector2 localPosition, float hDeltaDrop, float delay,
            bool isHardBlock) {
            if (_isWallDropTakeDamage) {
                return null;
            }
            if (_wallDrops.Count == 0) {
                return null;
            }
            var drop = _wallDrops[0];
            drop.Id = index;
            drop.gameObject.SetActive(true);
            _wallDrops.RemoveAt(0);
            var transform = drop.transform;
            transform.localPosition = localPosition;

            drop.SetSpriteAndInfo(_tileIndex, i, j, hDeltaDrop, delay, isHardBlock);
            _timeWallDrop[i, j] = Epoch.GetUnixTimestamp(TimeSpan.TicksPerMillisecond) + (long) (delay * 1000);
            return drop;
        }

        private void CreateWallDrop() {
            var wallDrop = (WallDrop) _entityManager.MapManager.CreateEntity(EntityType.WallDrop);
            wallDrop.transform.SetParent(_tfForeground);
            wallDrop.Id = int.MaxValue;
            _entityManager.AddEntity(wallDrop);
            _wallDrops.Add(wallDrop);
            wallDrop.gameObject.SetActive(false);
        }
    }
}