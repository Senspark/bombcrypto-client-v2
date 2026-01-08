using System;
using System.Collections.Generic;

using App;

using Engine.Entities;

using PvpMode.Services;

using UnityEngine;

namespace Engine.Manager {
    public class BombExplodeEvent {
        public Vector3 Position { get; }

        public bool IsShaking { get; }

        // Bomb
        public int ExplosionLength { get; }

        public bool ThroughBrick { get; }

        // public IProvider ExplosionProvider { get; }
        public int BombId { get; }
        public HeroId OwnerId { get; }
        public bool IsEnemy { get; }
        public float Damage { get; }

        public int ExplosionSkin { get; }

        // PVP
        public Dictionary<Direction, int> Ranges { get; }
        public (int, int)[] BrokenList { get; }

        public BombExplodeEvent(Vector3 position, Bomb bomb, bool isShaking) : this(position, bomb,
            null, Array.Empty<(int, int)>(), isShaking) { }

        public BombExplodeEvent(Vector3 position, Bomb bomb, Dictionary<Direction, int> ranges, (int, int)[] brokenList,
            bool isShaking) {
            Position = position;
            IsShaking = isShaking;
            ExplosionLength = bomb.ExplosionLength;
            ThroughBrick = bomb.ThroughBrick;
            BombId = bomb.BombId;
            OwnerId = bomb.OwnerId;
            IsEnemy = bomb.IsEnemy;
            Damage = bomb.Damage;
            ExplosionSkin = bomb.ExplosionSkin;
            Ranges = ranges;
            BrokenList = brokenList;
        }

        public BombExplodeEvent(Vector3 position,
            int explosionLength,
            bool throughBrick,
            bool isEnemy,
            float damage,
            int explosionSkin,
            bool isShaking = false) {
            Position = position;
            IsShaking = isShaking;
            ExplosionLength = explosionLength;
            ThroughBrick = throughBrick;
            IsEnemy = isEnemy;
            Damage = damage;
            ExplosionSkin = explosionSkin;
            Ranges = null;
            BrokenList = Array.Empty<(int, int)>();
        }
    }

    public class BombExplodeAni {
        private BombExplodeEvent _explode;
        private List<(int x, int y, ExplosionPose pose)> _explodesAni;

        public BombExplodeAni(BombExplodeEvent explode) {
            _explode = explode;
            _explodesAni = new List<(int x, int y, ExplosionPose pose)>();
        }

        public void Push(int x, int y, ExplosionPose pose) {
            _explodesAni.Add((x, y, pose));
        }

        public void Show(IEntityManager entityManager) {
            foreach (var ani in _explodesAni) {
                var position = entityManager.MapManager.GetTilePosition(ani.x, ani.y);
                var explosion = (BombExplosion) entityManager.MapManager.CreateEntity(EntityType.BombExplosion);
                explosion.Type = EntityType.BombExplosion;
                explosion.BombId = _explode.BombId;
                explosion.OwnerId = _explode.OwnerId;
                explosion.IsEnemy = _explode.IsEnemy;
                explosion.Damage = _explode.Damage;
                explosion.SetExplodeSpriteSheet(_explode.ExplosionSkin);

                var explosionTransform = explosion.transform;
                explosionTransform.SetParent(entityManager.View.transform, false);
                explosionTransform.localPosition = position;

                explosion.Init(ani.pose);
                entityManager.AddEntity(explosion);
            }
        }
    }

    public class CellExplosion {
        public Vector2Int Location { get; }
        public float Damage { get; }

        public CellExplosion(int x, int y, float damage) {
            Location = new Vector2Int(x, y);
            Damage = damage;
        }
    }

    public class DefaultExplodeEventManager : IExplodeEventManager {
        protected class ResultExplode {
            public BombExplodeEvent Explode;
            public List<Vector2Int> BrokenList;
            public List<Vector2Int> RemovedItem;
            public List<CellExplosion> CellExplosionList;
        }

        protected readonly IEntityManager EntityManager;
        private readonly List<BombExplodeEvent> _explodeList;

        private readonly List<ResultExplode> _resultExplodeList = new();
        protected float Elapse;

        private readonly List<BombExplodeAni> _explodesAniWait;

        protected DefaultExplodeEventManager(IEntityManager entityManager) {
            EntityManager = entityManager;
            _explodeList = new List<BombExplodeEvent>();
            _explodesAniWait = new List<BombExplodeAni>();
        }

        public void PushEvent(BombExplodeEvent explode) {
            _explodeList.Add(explode);
        }

        private void PushResult(ResultExplode result) {
            Elapse = 0;
            _resultExplodeList.Add(result);
        }

        protected ResultExplode PopResult() {
            if (_resultExplodeList.Count == 0) {
                return null;
            }
            var result = _resultExplodeList[0];
            _resultExplodeList.RemoveAt(0);
            return result;
        }

        private BombExplodeEvent PopEvent() {
            if (_explodeList.Count == 0) {
                return null;
            }
            var explodeEvent = _explodeList[0];
            _explodeList.RemoveAt(0);
            return explodeEvent;
        }

        public void UpdateProcess(float delta) {
            if (ProcessEvents(PopEvent())) {
                return;
            }
            if (_resultExplodeList.Count > 0) {
                ProcessExplodeResult(delta);
            }
            while (ProcessExplodeAni()) {
                // Keep process ani
            }
            if (_explodeList.Count == 0 && _resultExplodeList.Count == 0 && _explodesAniWait.Count == 0) {
                EntityManager.MapManager.ClearMarkBreakBrick();
            }
        }

        private bool ProcessEvents(BombExplodeEvent explode) {
            if (explode == null) {
                return false;
            }
            var result = ExplodeBomb(explode);
            PushResult(result);
            return true;
        }

        protected BombExplodeAni CreateBombExplodeAni(BombExplodeEvent explode) {
            var explodeAniData = new BombExplodeAni(explode);
            _explodesAniWait.Add(explodeAniData);
            return explodeAniData;
        }

        protected virtual ResultExplode ExplodeBomb(BombExplodeEvent explode) {
            if (explode.IsShaking) {
                ShakeCamera();
            }

            var pos = explode.Position;
            var mapManager = EntityManager.MapManager;
            var tileLocation = EntityManager.MapManager.GetTileLocation(pos);
            var i = tileLocation.x;
            var j = tileLocation.y;

            var brokenList = new List<Vector2Int>();
            var itemsRemovedList = new List<Vector2Int>();
            var cellExplosions = new List<CellExplosion>();

            // Create Explosion
            //// Center
            mapManager.RemoveHadBomb(tileLocation);
            mapManager.RemoveBomb(tileLocation);

            var explodeAniData = CreateBombExplodeAni(explode);

            explodeAniData.Push(i, j, ExplosionPose.Center);

            cellExplosions.Add(new CellExplosion(i, j, explode.Damage));

            var length = 1;
            var righLeftUpDown = new bool[] { true, true, true, true };

            while (length <= explode.ExplosionLength) {
                //// Right
                var r = i + length;
                if (righLeftUpDown[0]) {
                    if (mapManager.IsMarkBreakBrick(r, j)) {
                        righLeftUpDown[0] = false;
                    } else if (mapManager.IsEmpty(r, j, false, true)) {
                        var pose = length == explode.ExplosionLength
                            ? ExplosionPose.EndRight
                            : ExplosionPose.MidleHori;
                        explodeAniData.Push(r, j, pose);
                        cellExplosions.Add(new CellExplosion(r, j, explode.Damage));
                    } else {
                        if (explode.ThroughBrick) {
                            if (mapManager.IsEmpty(r, j, true, true)) {
                                var pose = length == explode.ExplosionLength
                                    ? ExplosionPose.EndRight
                                    : ExplosionPose.MidleHori;
                                explodeAniData.Push(r, j, pose);
                                cellExplosions.Add(new CellExplosion(r, j, explode.Damage));
                            } else {
                                righLeftUpDown[0] = false;
                            }
                        } else {
                            righLeftUpDown[0] = false;
                        }
                    }

                    if (LocationIsItem(r, j)) {
                        itemsRemovedList.Add(new Vector2Int(r, j));
                    }
                    if (BreakBrick(r, j)) {
                        brokenList.Add(new Vector2Int(r, j));
                    }
                }

                //// Left
                var l = i - length;
                if (righLeftUpDown[1]) {
                    if (mapManager.IsMarkBreakBrick(l, j)) {
                        righLeftUpDown[1] = false;
                    } else if (mapManager.IsEmpty(l, j, false, true)) {
                        var pose = length == explode.ExplosionLength
                            ? ExplosionPose.EndLeft
                            : ExplosionPose.MidleHori;
                        explodeAniData.Push(l, j, pose);
                        cellExplosions.Add(new CellExplosion(l, j, explode.Damage));
                    } else {
                        if (explode.ThroughBrick) {
                            if (mapManager.IsEmpty(l, j, true, true)) {
                                var pose = length == explode.ExplosionLength
                                    ? ExplosionPose.EndLeft
                                    : ExplosionPose.MidleHori;
                                explodeAniData.Push(l, j, pose);
                                cellExplosions.Add(new CellExplosion(l, j, explode.Damage));
                            } else {
                                righLeftUpDown[1] = false;
                            }
                        } else {
                            righLeftUpDown[1] = false;
                        }
                    }

                    if (LocationIsItem(l, j)) {
                        itemsRemovedList.Add(new Vector2Int(l, j));
                    }
                    if (BreakBrick(l, j)) {
                        brokenList.Add(new Vector2Int(l, j));
                    }
                }

                //// Up
                var u = j + length;
                if (righLeftUpDown[2]) {
                    if (mapManager.IsMarkBreakBrick(i, u)) {
                        righLeftUpDown[2] = false;
                    } else if (mapManager.IsEmpty(i, u, false, true)) {
                        var pose = length == explode.ExplosionLength
                            ? ExplosionPose.EndUp
                            : ExplosionPose.MidleVert;
                        explodeAniData.Push(i, u, pose);
                        cellExplosions.Add(new CellExplosion(i, u, explode.Damage));
                    } else {
                        if (explode.ThroughBrick) {
                            if (mapManager.IsEmpty(i, u, true, true)) {
                                var pose = length == explode.ExplosionLength
                                    ? ExplosionPose.EndUp
                                    : ExplosionPose.MidleVert;
                                explodeAniData.Push(i, u, pose);
                                cellExplosions.Add(new CellExplosion(i, u, explode.Damage));
                            } else {
                                righLeftUpDown[2] = false;
                            }
                        } else {
                            righLeftUpDown[2] = false;
                        }
                    }

                    if (LocationIsItem(i, u)) {
                        itemsRemovedList.Add(new Vector2Int(i, u));
                    }
                    if (BreakBrick(i, u)) {
                        brokenList.Add(new Vector2Int(i, u));
                    }
                }

                //// Down
                var d = j - length;
                if (righLeftUpDown[3]) {
                    if (mapManager.IsMarkBreakBrick(i, d)) {
                        righLeftUpDown[3] = false;
                    } else if (mapManager.IsEmpty(i, d, false, true)) {
                        var pose = length == explode.ExplosionLength
                            ? ExplosionPose.EndDown
                            : ExplosionPose.MidleVert;
                        explodeAniData.Push(i, d, pose);
                        cellExplosions.Add(new CellExplosion(i, d, explode.Damage));
                    } else {
                        if (explode.ThroughBrick) {
                            if (mapManager.IsEmpty(i, d, true, true)) {
                                var pose = length == explode.ExplosionLength
                                    ? ExplosionPose.EndDown
                                    : ExplosionPose.MidleVert;
                                explodeAniData.Push(i, d, pose);
                                cellExplosions.Add(new CellExplosion(i, d, explode.Damage));
                            } else {
                                righLeftUpDown[3] = false;
                            }
                        } else {
                            righLeftUpDown[3] = false;
                        }
                    }

                    if (LocationIsItem(i, d)) {
                        itemsRemovedList.Add(new Vector2Int(i, d));
                    }
                    if (BreakBrick(i, d)) {
                        brokenList.Add(new Vector2Int(i, d));
                    }
                }

                length += 1;
            }
            return new ResultExplode {
                Explode = explode,
                BrokenList = brokenList,
                RemovedItem = itemsRemovedList,
                CellExplosionList = cellExplosions
            };
        }

        protected virtual void ProcessExplodeResult(float delta) {
            var result = PopResult();
            while (result != null) {
                AfterExplode(result.Explode, result.BrokenList, result.RemovedItem);
                result = PopResult();
            }
        }

        protected virtual void AfterExplode(BombExplodeEvent explode, List<Vector2Int> brokenList,
            List<Vector2Int> itemsRemovedList) {
            // do nothing
        }

        private bool ProcessExplodeAni() {
            if (_explodesAniWait.Count <= 0) {
                return false;
            }
            // Hiện tại chỉ ani 1 bomb / frame
            _explodesAniWait.Pop().Show(EntityManager);
            return true;
        }

        protected void ShakeCamera() {
            EntityManager.LevelManager.Camera?.Shaking(0.1f, 1);
        }

        private bool BreakBrick(int i, int j) {
            if (EntityManager.MapManager.TryGetBlock(i, j, out var block)) {
                return true;
            }
            return false;
        }

        private bool LocationIsItem(int i, int j) {
            return EntityManager.MapManager.LocationIsItem(i, j);
        }
    }
}