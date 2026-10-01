using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using App;

using Engine.Components;
using Engine.Entities;
using Engine.Manager;

using JetBrains.Annotations;

using Senspark;

using UnityEngine;

namespace Game.Treasure {
    public interface ITreasurePlaybackHost {
        [CanBeNull]
        IEntityManager EntityManager { get; }

        // Makes the scene's map match the snapshot in place; false = the host reloads the scene instead.
        bool TryApplySnapshotMap(TreasureSnapshot snapshot);

        void ApplyExplode(ITreasureExplode data, HeroId heroId, bool showEffects, bool markBreak);
        void OnHeroLeave(HeroId heroId, [CanBeNull] Player player, string reason);

        // NEW_MAP just arrived (not played yet): the host pauses the server so the new map isn't played unseen.
        void OnNewMapReceived();

        // Every event before NEW_MAP was played; the host shows the dialog and loads the new map.
        void OnNewMap(TreasureEvent newMap);
    }

    /// <summary>
    /// Re-enacts the server's TREASURE_EVENTS stream (docs/treasure-server-driven-client-guide.md).
    /// The client never decides anything here: heroes only walk/plant/leave when an event says so.
    /// </summary>
    public class TreasurePlayback {
        private const long JitterMs = 250;
        private const long FastForwardMs = 8000;
        private const long MinBombVisibleMs = 400;
        private const long NewMapGraceMs = 2000;
        private const double SilenceResyncSeconds = 20;
        private const double ResyncRetrySeconds = 3;
        private const double ResyncRetryMaxSeconds = 15;
        private const double ResyncTimeoutSeconds = 15;

        private readonly ITreasurePlaybackHost _host;
        private readonly Func<Task<TreasureSnapshot>> _startTreasureMode;
        private readonly ILogManager _logManager;
        private readonly Func<double> _realtime;

        private readonly Dictionary<int, HeroTimeline> _heroes = new();
        private readonly Dictionary<(int, int), BombState> _bombs = new();
        private readonly List<TreasureEvent> _orphanExplodes = new();
        private readonly List<TreasureEvent> _pending = new();
        private readonly List<int> _removals = new();
        private readonly List<BombState> _detonations = new();

        private bool _ready;
        private bool _stopped;
        private bool _resyncing;
        private int _resyncId;
        private double _resyncStartedAt;
        // Last resync failed or got no answer: Update keeps retrying until one succeeds.
        private bool _resyncFailed;
        private double _resyncRetryDelay = ResyncRetrySeconds;
        private double _resyncRetryAt;
        private long _lastSeq;
        private double _offset;
        private double _lastEventRealtime;
        private double _lastUpdateRealtime;
        [CanBeNull]
        private TreasureEvent _newMapBarrier;
        private long _newMapDeadline;
        private double _pausedRealtime = -1;

        public TreasurePlayback(ITreasurePlaybackHost host, Func<Task<TreasureSnapshot>> startTreasureMode,
            ILogManager logManager = null, Func<double> realtime = null) {
            _host = host;
            _startTreasureMode = startTreasureMode;
            _logManager = logManager ?? ServiceLocator.Instance.Resolve<ILogManager>();
            _realtime = realtime ?? (() => Time.realtimeSinceStartupAsDouble);
        }

        private double NowMs => _realtime() * 1000;
        private long PlayTime => (long) (NowMs + _offset) - JitterMs;

        #region INTAKE

        public void OnEvents(List<TreasureEvent> events) {
            _lastEventRealtime = _realtime();
            foreach (var e in events) {
                if (_stopped) {
                    return;
                }
                if (!_ready || _resyncing) {
                    _pending.Add(e);
                    continue;
                }
                Intake(e);
            }
        }

        private void Intake(TreasureEvent e) {
            if (e.Type == TreasureEventType.Resync) {
                _logManager.Log($"[TREASURE] RESYNC reason={e.Reason} seq={e.Seq}");
                ApplySnapshot(e.Snapshot);
                return;
            }
            if (e.Seq <= _lastSeq) {
                return;
            }
            if (e.Seq > _lastSeq + 1) {
                _logManager.Log($"[TREASURE] seq gap: expected {_lastSeq + 1} got {e.Seq}");
                _pending.Add(e);
                _ = Resync("seq_gap");
                return;
            }
            _lastSeq = e.Seq;
            _offset = Math.Max(_offset, e.At - NowMs);
            if (_newMapBarrier != null) {
                // Belongs to the next map; the scene rebuilds from a fresh snapshot.
                return;
            }
            switch (e.Type) {
                case TreasureEventType.Move:
                case TreasureEventType.HeroJoin:
                case TreasureEventType.HeroLeave:
                    GetOrCreateHero(e.Hero, e.At).Enqueue(e);
                    break;
                case TreasureEventType.Plant:
                    _bombs[(e.Hero.Id, e.Num)] = new BombState {
                        HeroId = e.Hero.Id,
                        HeroType = e.Hero.HeroType,
                        Num = e.Num,
                        Cell = e.Hero.Cell,
                        ExplodeAt = e.ExplodeAt,
                        FuseMs = Math.Max(0, e.ExplodeAt - e.PlantedAt),
                    };
                    GetOrCreateHero(e.Hero, e.At).Enqueue(e);
                    break;
                case TreasureEventType.Explode:
                    if (_bombs.TryGetValue((e.Hero.Id, e.Num), out var bomb)) {
                        bomb.Explode = e;
                    } else {
                        _orphanExplodes.Add(e);
                    }
                    break;
                case TreasureEventType.NewMap:
                    _newMapBarrier = e;
                    _newMapDeadline = e.At + NewMapGraceMs;
                    _host.OnNewMapReceived();
                    break;
            }
        }

        private HeroTimeline GetOrCreateHero(TreasureHeroPosition hero, long at) {
            if (!_heroes.TryGetValue(hero.Id, out var timeline)) {
                timeline = new HeroTimeline(hero.Id, hero.HeroType, hero.Cell, at);
                _heroes[hero.Id] = timeline;
            }
            if (hero.HeroType >= 0) {
                timeline.HeroType = hero.HeroType;
            }
            return timeline;
        }

        #endregion

        #region SNAPSHOT / RESYNC

        public void ApplySnapshot(TreasureSnapshot snapshot) {
            _resyncing = false;
            _resyncFailed = false;
            _resyncRetryDelay = ResyncRetrySeconds;
            _resyncRetryAt = 0;
            if (!_host.TryApplySnapshotMap(snapshot)) {
                Stop();
                return;
            }
            foreach (var bomb in _bombs.Values) {
                if (bomb.Visual != null && bomb.Visual.IsAlive) {
                    bomb.Visual.DestroyMe();
                }
            }
            _bombs.Clear();
            _orphanExplodes.Clear();
            _newMapBarrier = null;
            _lastSeq = snapshot.Seq;
            _offset = snapshot.ServerTime - NowMs;
            _lastEventRealtime = _realtime();

            var previous = new Dictionary<int, HeroTimeline>(_heroes);
            _heroes.Clear();
            foreach (var hero in snapshot.Heroes) {
                var timeline = new HeroTimeline(hero.Id, hero.HeroType, hero.Cell, snapshot.ServerTime);
                if (previous.TryGetValue(hero.Id, out var old)) {
                    timeline.Player = old.Player;
                    previous.Remove(hero.Id);
                }
                _heroes[hero.Id] = timeline;
            }
            foreach (var old in previous.Values) {
                old.StopPuppet();
            }
            foreach (var b in snapshot.Bombs) {
                _bombs[(b.HeroId, b.Num)] = new BombState {
                    HeroId = b.HeroId,
                    HeroType = -1,
                    Num = b.Num,
                    Cell = b.Cell,
                    ExplodeAt = b.ExplodeAt,
                    FuseMs = Math.Max(0, b.ExplodeAt - b.PlantedAt),
                    Planted = true,
                    ElapsedMs = Math.Max(0, snapshot.ServerTime - b.PlantedAt),
                };
            }
            _ready = true;

            var buffered = new List<TreasureEvent>(_pending);
            _pending.Clear();
            foreach (var e in buffered) {
                if (_stopped || _resyncing) {
                    _pending.Add(e);
                    continue;
                }
                Intake(e);
            }
            // Jump over whatever happened while loading / waiting for the snapshot, no visuals.
            if (!_stopped) {
                Step(0, true);
            }
        }

        // Full resync through START_TREASURE_MODE (seq gap, silence, reconnect).
        // force: replaces the one in flight, whose request went out on a connection that is gone.
        public async Task Resync(string reason, bool force = false) {
            if (_stopped || (!force && (_resyncing || _realtime() < _resyncRetryAt))) {
                return;
            }
            var id = ++_resyncId;
            _resyncing = true;
            _resyncStartedAt = _realtime();
            _logManager.Log($"[TREASURE] resync reason={reason}");
            try {
                var snapshot = await _startTreasureMode();
                if (id == _resyncId && !_stopped) {
                    ApplySnapshot(snapshot);
                }
            } catch (Exception e) {
                if (id != _resyncId) {
                    return;
                }
                _logManager.Log($"[TREASURE] resync failed: {e.Message}");
                FailResync();
            }
        }

        private void FailResync() {
            _resyncing = false;
            _resyncFailed = true;
            _resyncRetryAt = _realtime() + _resyncRetryDelay;
            _resyncRetryDelay = Math.Min(_resyncRetryDelay * 2, ResyncRetryMaxSeconds);
        }

        /// <summary>
        /// Stops playing (scene leaving or reloading). Heroes stand still where they are.
        /// </summary>
        public void Stop() {
            _stopped = true;
            foreach (var hero in _heroes.Values) {
                hero.StopPuppet();
            }
        }

        public bool IsStopped => _stopped;

        // Scene pause: playback stops with the scene and resumes from the queue; the lag it leaves is never fast-forwarded.
        public void SetPaused(bool paused) {
            if (paused) {
                if (_pausedRealtime < 0) {
                    _pausedRealtime = _realtime();
                }
                return;
            }
            if (_pausedRealtime < 0) {
                return;
            }
            var pausedMs = (long) ((_realtime() - _pausedRealtime) * 1000);
            _pausedRealtime = -1;
            foreach (var hero in _heroes.Values) {
                hero.PauseDebt += pausedMs;
            }
            if (_newMapBarrier != null) {
                _newMapDeadline += pausedMs;
            }
        }

        public Vector2Int? GetHeroTile(int heroId) {
            return _heroes.TryGetValue(heroId, out var hero) && hero.Active ? hero.Tile : null;
        }

        #endregion

        #region PLAYBACK

        public void Update(float delta) {
            if (!_ready || _stopped) {
                return;
            }
            var realtime = _realtime();
            if (realtime - _lastUpdateRealtime > 1) {
                // Suspended (hidden tab, pause): queued pushes and answers arrive now, don't count that as silence.
                _lastEventRealtime = realtime;
                _resyncStartedAt = realtime;
            }
            _lastUpdateRealtime = realtime;
            if (_resyncing) {
                if (realtime - _resyncStartedAt > ResyncTimeoutSeconds) {
                    // The answer never came (request dropped by the server): give up on it and ask again.
                    _logManager.Log("[TREASURE] resync timed out");
                    _resyncId++;
                    FailResync();
                }
            } else if (_resyncFailed) {
                if (realtime >= _resyncRetryAt) {
                    _ = Resync("retry");
                }
            } else if (_heroes.Count > 0 && realtime - _lastEventRealtime > SilenceResyncSeconds) {
                _lastEventRealtime = realtime;
                _ = Resync("silence");
            }
            Step(delta, false);
        }

        private void Step(float delta, bool forceFastForward) {
            var play = PlayTime;
            var fastForward = forceFastForward || (_newMapBarrier != null && play >= _newMapDeadline);
            foreach (var hero in _heroes.Values) {
                if (play - hero.T - hero.PauseDebt > FastForwardMs) {
                    fastForward = true;
                }
            }

            var deltaMs = delta * 1000.0;
            foreach (var bomb in _bombs.Values) {
                if (bomb.Planted) {
                    bomb.ElapsedMs += deltaMs;
                    bomb.VisibleMs += deltaMs;
                }
            }
            foreach (var hero in _heroes.Values) {
                hero.Advance(deltaMs, play, fastForward, this);
                if (hero.CanBeRemoved) {
                    _removals.Add(hero.Id);
                }
            }
            foreach (var id in _removals) {
                _heroes.Remove(id);
            }
            _removals.Clear();

            UpdateBombs(fastForward);
            UpdateOrphanExplodes(play, fastForward);
            SyncPuppets();

            if (_newMapBarrier != null && play >= _newMapBarrier.At && IsDrained()) {
                var newMap = _newMapBarrier;
                _logManager.Log($"[TREASURE] NEW_MAP seq={newMap.Seq}");
                Stop();
                _host.OnNewMap(newMap);
            }
        }

        private bool IsDrained() {
            foreach (var hero in _heroes.Values) {
                if (hero.HasQueuedActions) {
                    return false;
                }
            }
            foreach (var bomb in _bombs.Values) {
                if (bomb.Explode != null) {
                    return false;
                }
            }
            return _orphanExplodes.Count == 0;
        }

        private void SyncPuppets() {
            var entityManager = _host.EntityManager;
            if (entityManager?.PlayerManager == null) {
                return;
            }
            foreach (var hero in _heroes.Values) {
                if (hero.Player == null || !hero.Player.IsAlive) {
                    hero.Player = FindPlayer(entityManager, hero.Id, hero.HeroType);
                }
                hero.SyncPuppet();
            }
        }

        [CanBeNull]
        private static Player FindPlayer(IEntityManager entityManager, int heroId, int heroType) {
            var players = entityManager.PlayerManager.Players;
            foreach (var player in players) {
                if (player == null || !player.IsAlive || player.HeroId.Id != heroId) {
                    continue;
                }
                if (heroType < 0 || (int) player.HeroId.Type == heroType) {
                    return player;
                }
            }
            return null;
        }

        private HeroId ResolveHeroId(int heroId, int heroType) {
            if (heroType < 0 && _heroes.TryGetValue(heroId, out var hero)) {
                heroType = hero.HeroType;
            }
            var entityManager = _host.EntityManager;
            if (heroType < 0 && entityManager?.PlayerManager != null) {
                var player = FindPlayer(entityManager, heroId, -1);
                if (player != null) {
                    return player.HeroId;
                }
            }
            return new HeroId(heroId, (HeroAccountType) Math.Max(0, heroType));
        }

        #endregion

        #region HERO ACTIONS

        private void OnPlantPlayed(HeroTimeline hero, TreasureEvent e, bool fastForward) {
            if (!_bombs.TryGetValue((e.Hero.Id, e.Num), out var bomb)) {
                return;
            }
            bomb.Planted = true;
            bomb.ElapsedMs = Math.Max(0, hero.T - e.PlantedAt);
            // When fast-forwarding, UpdateBombs spawns it afterwards if it is still ticking.
            if (!fastForward) {
                bomb.Visual = SpawnBombVisual(hero.Player, bomb);
            }
        }

        private void OnLeavePlayed(HeroTimeline hero, TreasureEvent e) {
            _host.OnHeroLeave(ResolveHeroId(hero.Id, hero.HeroType), hero.Player, e.Reason ?? "");
        }

        [CanBeNull]
        private static Bomb SpawnBombVisual([CanBeNull] Player owner, BombState bomb) {
            if (owner == null || !owner.IsAlive) {
                return null;
            }
            return owner.Bombable.SpawnServerBomb(bomb.Num, bomb.Cell);
        }

        #endregion

        #region BOMBS

        private void UpdateBombs(bool fastForward) {
            foreach (var bomb in _bombs.Values) {
                if (!bomb.Planted) {
                    continue;
                }
                if (bomb.Visual == null && bomb.Explode == null && !fastForward) {
                    // Owner wasn't on screen yet when it planted (scene start, resync).
                    if (_heroes.TryGetValue(bomb.HeroId, out var owner)) {
                        bomb.Visual = SpawnBombVisual(owner.Player, bomb);
                    }
                }
                if (bomb.Explode == null) {
                    continue;
                }
                // Local fuse only runs while playing, so a pause or a late hero never shortens it.
                if (fastForward ||
                    (bomb.ElapsedMs >= bomb.FuseMs && bomb.VisibleMs >= Math.Min(bomb.FuseMs, MinBombVisibleMs))) {
                    _detonations.Add(bomb);
                }
            }
            foreach (var bomb in _detonations) {
                _bombs.Remove((bomb.HeroId, bomb.Num));
                Detonate(bomb, !fastForward);
            }
            _detonations.Clear();
        }

        private void Detonate(BombState bomb, bool showEffects) {
            var visual = bomb.Visual;
            var markBreak = false;
            if (visual != null && visual.IsAlive) {
                if (showEffects) {
                    markBreak = !visual.ThroughBrick;
                    visual.StartExplode(visual.transform.localPosition);
                } else {
                    visual.DestroyMe();
                }
            }
            var explode = bomb.Explode;
            _host.ApplyExplode(explode.Explode, ResolveHeroId(explode.Hero.Id, explode.Hero.HeroType), showEffects,
                markBreak);
        }

        private void UpdateOrphanExplodes(long play, bool fastForward) {
            if (_orphanExplodes.Count == 0) {
                return;
            }
            var count = 0;
            while (count < _orphanExplodes.Count && _orphanExplodes[count].At <= play) {
                var e = _orphanExplodes[count];
                _host.ApplyExplode(e.Explode, ResolveHeroId(e.Hero.Id, e.Hero.HeroType), !fastForward, false);
                count++;
            }
            _orphanExplodes.RemoveRange(0, count);
        }

        private class BombState {
            public int HeroId;
            public int HeroType;
            public int Num;
            public Vector2Int Cell;
            public long ExplodeAt;
            public long FuseMs;
            public bool Planted;
            public double ElapsedMs;
            public double VisibleMs;
            [CanBeNull]
            public Bomb Visual;
            [CanBeNull]
            public TreasureEvent Explode;
        }

        #endregion

        /// <summary>
        /// One hero's queue of MOVE / PLANT / JOIN / LEAVE, played on its own clock <see cref="T"/>
        /// (server ms). T always advances at real speed: a hero that fell behind stays behind (never sped up).
        /// </summary>
        private class HeroTimeline {
            public readonly int Id;
            public int HeroType;
            public long T;
            // Lag left by a scene pause; excluded from the fast-forward check until the hero catches up.
            public long PauseDebt;
            public Vector2Int Tile { get; private set; }
            public bool Active { get; private set; } = true;
            [CanBeNull]
            private Player _player;
            [CanBeNull]
            private BotManager _botManager;

            [CanBeNull]
            public Player Player {
                get => _player;
                set {
                    _player = value;
                    _botManager = value != null ? value.GetComponent<BotManager>() : null;
                }
            }

            private readonly Queue<TreasureEvent> _queue = new();
            [CanBeNull]
            private TreasureEvent _move;
            private Vector2 _position;
            private Vector2 _direction;
            private double _carryMs;

            public bool HasQueuedActions => _queue.Count > 0;
            public bool CanBeRemoved => !Active && _queue.Count == 0;

            public HeroTimeline(int id, int heroType, Vector2Int tile, long t) {
                Id = id;
                HeroType = heroType;
                Tile = tile;
                T = t;
                _position = TileCenter(tile);
            }

            public void Enqueue(TreasureEvent e) {
                if (_queue.Count == 0 && e.At < T) {
                    // Arrived late: the hero was standing on `from` meanwhile, replay from its real start.
                    T = e.At;
                }
                _queue.Enqueue(e);
            }

            public void Advance(double deltaMs, long play, bool fastForward, TreasurePlayback playback) {
                if (fastForward) {
                    T = Math.Max(T, play);
                } else if (_queue.Count == 0 && !IsWalking(T)) {
                    T = Math.Max(T, play);
                } else {
                    if (_queue.Count > 0 && !IsWalking(T)) {
                        // Standing idle while behind (e.g. right after a pause): jump to the next action, walks are never sped up.
                        T = Math.Max(T, Math.Min(play, _queue.Peek().At));
                    }
                    _carryMs += deltaMs;
                    var stepMs = (long) _carryMs;
                    _carryMs -= stepMs;
                    T = Math.Max(T, Math.Min(play, T + stepMs));
                }
                while (_queue.Count > 0 && _queue.Peek().At <= T) {
                    Start(_queue.Dequeue(), playback, fastForward);
                }
                PauseDebt = Math.Min(PauseDebt, Math.Max(0, play - T));
                Evaluate();
            }

            private bool IsWalking(long t) {
                return _move != null && t < _move.At + _move.Path.Count * Math.Max(1, _move.StepMs);
            }

            private void Start(TreasureEvent e, TreasurePlayback playback, bool fastForward) {
                // A new action always starts on the tile the hero reached at e.At (re-plan, plant, stop).
                Evaluate(e.At);
                switch (e.Type) {
                    case TreasureEventType.Move:
                        Active = true;
                        Tile = e.Hero.Cell;
                        _move = e.Path.Count > 0 ? e : null;
                        break;
                    case TreasureEventType.Plant:
                        Active = true;
                        Tile = e.Hero.Cell;
                        _move = null;
                        playback.OnPlantPlayed(this, e, fastForward);
                        break;
                    case TreasureEventType.HeroJoin:
                        Active = true;
                        Tile = e.Hero.Cell;
                        _move = null;
                        break;
                    case TreasureEventType.HeroLeave:
                        _move = null;
                        Active = false;
                        playback.OnLeavePlayed(this, e);
                        StopPuppet();
                        break;
                }
                _position = TileCenter(Tile);
                _direction = Vector2.zero;
            }

            private void Evaluate() {
                Evaluate(T);
            }

            private void Evaluate(long t) {
                if (_move == null) {
                    _position = TileCenter(Tile);
                    _direction = Vector2.zero;
                    return;
                }
                var step = Math.Max(1, _move.StepMs);
                var path = _move.Path;
                var elapsed = Math.Max(0, t - _move.At);
                if (elapsed >= path.Count * step) {
                    Tile = path[path.Count - 1];
                    _move = null;
                    _position = TileCenter(Tile);
                    _direction = Vector2.zero;
                    return;
                }
                var k = (int) (elapsed / step);
                var from = k == 0 ? _move.Hero.Cell : path[k - 1];
                var to = path[k];
                var fraction = (float) (elapsed - k * step) / step;
                Tile = from;
                _position = Vector2.Lerp(TileCenter(from), TileCenter(to), fraction);
                _direction = to - from;
            }

            public void SyncPuppet() {
                if (!Active || Player == null || !Player.IsAlive) {
                    return;
                }
                if (_botManager != null && _botManager.IsSleeping) {
                    Player.Movable.SetPuppetMove(Vector2.zero);
                    return;
                }
                var transform = Player.transform;
                var z = transform.localPosition.z;
                transform.localPosition = new Vector3(_position.x, _position.y, z);
                Player.Movable.SetPuppetMove(_direction);
            }

            public void StopPuppet() {
                if (Player != null && Player.IsAlive) {
                    Player.Movable.SetPuppetMove(Vector2.zero);
                }
            }

            // Same layout as DefaultMapManagerV2.GetTilePosition.
            private static Vector2 TileCenter(Vector2Int tile) {
                return new Vector2(tile.x + 0.5f, tile.y + 0.5f);
            }
        }
    }
}
