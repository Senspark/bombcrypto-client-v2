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
    /// Heroes only walk/plant/leave when an event says so; while the server has a hero waiting, it roams locally.
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
        // Open-ended roaming goes this far (walking time) from the server tile: the whole map for most heroes.
        private const long RoamRangeMs = 8000;
        // Extra tiles of a walk back longer than this are hurried, up to MaxRush times the hero's speed.
        private const long DetourMs = 1500;
        private const double MaxRush = 3;
        private const long DefaultStepMs = 200;

        private static readonly Vector2Int[] Directions = {
            Vector2Int.left, Vector2Int.right, Vector2Int.down, Vector2Int.up,
        };

        private readonly ITreasurePlaybackHost _host;
        private readonly Func<Task<TreasureSnapshot>> _startTreasureMode;
        private readonly ILogManager _logManager;
        private readonly Func<double> _realtime;
        [CanBeNull]
        private readonly IMapManager _mapOverride;

        private readonly Dictionary<int, HeroTimeline> _heroes = new();
        private readonly Dictionary<(int, int), BombState> _bombs = new();
        private readonly List<TreasureEvent> _orphanExplodes = new();
        private readonly List<TreasureEvent> _pending = new();
        private readonly List<int> _removals = new();
        private readonly List<BombState> _detonations = new();
        private readonly System.Random _random = new();
        private readonly Queue<Vector2Int> _bfsQueue = new();
        private int[,] _distances;
        private int[,] _homeDistances;

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
            ILogManager logManager = null, Func<double> realtime = null, IMapManager mapOverride = null) {
            _mapOverride = mapOverride;
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
                    timeline.KeepRoamPlace(old);
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
            // The server's fuse ran on: bombs it already exploded pop now, so its next PLANT never stacks on them.
            foreach (var bomb in _bombs.Values) {
                if (bomb.Planted) {
                    bomb.ElapsedMs += pausedMs;
                }
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

        // Tile the hero's entity stands on right now; null when it is not on the map yet.
        private Vector2Int? ShownTile(HeroTimeline hero) {
            if (hero.Player == null || !hero.Player.IsAlive) {
                var entityManager = _host.EntityManager;
                if (entityManager?.PlayerManager == null) {
                    return null;
                }
                hero.Player = FindPlayer(entityManager, hero.Id, hero.HeroType);
            }
            if (hero.Player == null) {
                return null;
            }
            return Vector2Int.FloorToInt(hero.Player.transform.localPosition);
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

        #region LOCAL MOVEMENT

        // Steps from `origin` to every tile a hero can walk to (-1 = cannot), written into the reused `buffer`.
        [CanBeNull]
        private int[,] Distances([CanBeNull] Player player, Vector2Int origin, ref int[,] buffer) {
            var map = _mapOverride ?? (player != null && player.IsAlive ? player.EntityManager?.MapManager : null);
            var tiles = map?.GetTileTypeMap();
            if (tiles == null) {
                return null;
            }
            var cols = tiles.GetLength(0);
            var rows = tiles.GetLength(1);
            if (origin.x < 0 || origin.x >= cols || origin.y < 0 || origin.y >= rows) {
                return null;
            }
            if (buffer == null || buffer.GetLength(0) != cols || buffer.GetLength(1) != rows) {
                buffer = new int[cols, rows];
            }
            for (var i = 0; i < cols; i++) {
                for (var j = 0; j < rows; j++) {
                    buffer[i, j] = -1;
                }
            }
            var throughBrick = player != null && player.WalkThrough != null && player.WalkThrough.ThroughBrick;
            buffer[origin.x, origin.y] = 0;
            _bfsQueue.Clear();
            _bfsQueue.Enqueue(origin);
            while (_bfsQueue.Count > 0) {
                var cell = _bfsQueue.Dequeue();
                foreach (var direction in Directions) {
                    var next = cell + direction;
                    if (next.x < 0 || next.x >= cols || next.y < 0 || next.y >= rows || buffer[next.x, next.y] >= 0) {
                        continue;
                    }
                    // Bombs never block here: the server's heroes do not collide, this is only the walk shown.
                    if (!map.IsEmpty(next.x, next.y, false, true) && !(throughBrick && map.IsBrick(next.x, next.y))) {
                        continue;
                    }
                    buffer[next.x, next.y] = buffer[cell.x, cell.y] + 1;
                    _bfsQueue.Enqueue(next);
                }
            }
            return buffer;
        }

        private static int DistanceAt(int[,] distances, Vector2Int cell) {
            return cell.x < 0 || cell.x >= distances.GetLength(0) || cell.y < 0 || cell.y >= distances.GetLength(1)
                ? -1
                : distances[cell.x, cell.y];
        }

        // Neighbour of `cell` one step closer to the origin of `distances`; null when there is no way.
        private static Vector2Int? StepCloser(int[,] distances, Vector2Int cell) {
            var here = DistanceAt(distances, cell);
            Vector2Int? best = null;
            var bestDistance = here < 0 ? int.MaxValue : here;
            foreach (var direction in Directions) {
                var distance = DistanceAt(distances, cell + direction);
                if (distance >= 0 && distance < bestDistance) {
                    bestDistance = distance;
                    best = cell + direction;
                }
            }
            return best;
        }

        // Tiles to walk after `from` up to `to`; null when unreachable.
        [CanBeNull]
        private List<Vector2Int> FindPath([CanBeNull] Player player, Vector2Int from, Vector2Int to) {
            var distances = Distances(player, to, ref _distances);
            if (distances == null) {
                return null;
            }
            var path = new List<Vector2Int>();
            var cell = from;
            while (cell != to) {
                var next = StepCloser(distances, cell);
                if (next == null) {
                    return null;
                }
                cell = next.Value;
                path.Add(cell);
            }
            return path;
        }

        // Next tile for a roaming hero: towards a random `goal` anywhere it can still walk back from in time.
        private Vector2Int? PickRoamStep([CanBeNull] Player player, Vector2Int tile, Vector2Int anchor, long now,
            long until, long stepMs, ref Vector2Int? goal) {
            if (tile == anchor && until > 0 && now + 2 * stepMs > until) {
                // No time left for a step out and back.
                return null;
            }
            var home = Distances(player, anchor, ref _homeDistances);
            if (home == null) {
                return null;
            }
            for (var attempt = 0; attempt < 2; attempt++) {
                if (goal == null || goal == tile) {
                    goal = PickRoamGoal(player, tile, home, now, until, stepMs);
                }
                if (goal == null) {
                    break;
                }
                var toGoal = Distances(player, goal.Value, ref _distances);
                var next = toGoal == null ? null : StepCloser(toGoal, tile);
                if (next != null) {
                    var back = DistanceAt(home, next.Value);
                    if (until <= 0 || (back >= 0 && now + stepMs * (1 + back) <= until)) {
                        return next;
                    }
                }
                goal = null;
            }
            // Nowhere to go in the time left: head back to the server tile and wait there.
            return until > 0 && tile != anchor ? StepCloser(home, tile) : null;
        }

        // Random reachable tile: within RoamRangeMs of the server tile, or (timed) reachable and back before `until`.
        private Vector2Int? PickRoamGoal([CanBeNull] Player player, Vector2Int tile, int[,] home, long now, long until,
            long stepMs) {
            var fromHere = Distances(player, tile, ref _distances);
            if (fromHere == null) {
                return null;
            }
            Vector2Int? goal = null;
            var count = 0;
            for (var i = 0; i < fromHere.GetLength(0); i++) {
                for (var j = 0; j < fromHere.GetLength(1); j++) {
                    var there = fromHere[i, j];
                    var back = home[i, j];
                    if (there <= 0 || back < 0) {
                        continue;
                    }
                    var allowed = until > 0 ? now + stepMs * (there + back) <= until : back * stepMs <= RoamRangeMs;
                    if (allowed && _random.Next(++count) == 0) {
                        goal = new Vector2Int(i, j);
                    }
                }
            }
            return goal;
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
                // Local fuse starts when the plant is played, so a late hero never shortens it; a pause is added back on resume.
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
            // Current walk on the hero clock: `_path` from `_from`, started at `_moveAt`.
            [CanBeNull]
            private List<Vector2Int> _path;
            private Vector2Int _from;
            private long _moveAt;
            private long _stepMs = DefaultStepMs;
            // Detour to rejoin the server: T waits `_stallMs` while the walk runs on, `_bonusMs` is the time added so far.
            private double _rush = 1;
            private double _stallMs;
            private double _bonusMs;
            private double _bonusTotal;
            // Local roaming while the server has the hero waiting on `_anchor`.
            private bool _roam;
            private long _roamUntil;
            private Vector2Int _anchor;
            private Vector2Int? _roamTo;
            private Vector2Int? _roamGoal;
            private double _roamMs;
            // Kept its roaming place over a resync: its first action walks there even while fast-forwarding.
            private bool _keptPlace;
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

            // Resync: a hero that was roaming stays where it is shown and walks back, instead of jumping to the server tile.
            public void KeepRoamPlace(HeroTimeline old) {
                if (!old._roam || !old.Active) {
                    return;
                }
                _keptPlace = true;
                Tile = old.Tile;
                _stepMs = old._stepMs;
                _roamTo = old._roamTo;
                _roamMs = old._roamMs;
                _position = old._position;
                _direction = old._direction;
            }

            public void Advance(double deltaMs, long play, bool fastForward, TreasurePlayback playback) {
                if (fastForward) {
                    _stallMs = 0;
                    T = Math.Max(T, play);
                } else if (_stallMs > 0) {
                    var used = Math.Min(deltaMs * _rush, _stallMs);
                    _stallMs -= used;
                    _bonusMs = _stallMs > 0 ? _bonusMs + used : _bonusTotal;
                    // Like a pause: this lag is the hero's own and must not fast-forward everyone.
                    PauseDebt += (long) Math.Ceiling(deltaMs);
                } else {
                    // A roam step in progress ends on its tile before the next server action starts.
                    var limit = _roamTo != null && _queue.Count > 0
                        ? Math.Min(play, Math.Max(T, _queue.Peek().At))
                        : play;
                    if (_queue.Count == 0 && !IsWalking(T)) {
                        T = Math.Max(T, limit);
                    } else {
                        if (_queue.Count > 0 && !IsWalking(T)) {
                            // Standing idle while behind (e.g. right after a pause): jump to the next action, walks are never sped up.
                            T = Math.Max(T, Math.Min(limit, _queue.Peek().At));
                        }
                        _carryMs += deltaMs;
                        var stepMs = (long) _carryMs;
                        _carryMs -= stepMs;
                        T = Math.Max(T, Math.Min(limit, T + stepMs));
                    }
                    AdvanceRoamStep(deltaMs);
                }
                while (_queue.Count > 0 && _queue.Peek().At <= T) {
                    var smooth = !fastForward || _keptPlace;
                    if (smooth && (_stallMs > 0 || _roamTo != null || WalkBackFor(_queue.Peek(), playback))) {
                        break;
                    }
                    Start(_queue.Dequeue(), playback, fastForward && !_keptPlace);
                }
                if (!fastForward) {
                    StartRoamStep(playback);
                }
                PauseDebt = Math.Min(PauseDebt, Math.Max(0, play - T));
                Evaluate();
            }

            private bool IsWalking(long t) {
                return _path != null && t + _bonusMs < _moveAt + _path.Count * _stepMs;
            }

            private void Walk(Vector2Int from, List<Vector2Int> path, long at, double extraMs) {
                _from = from;
                _path = path;
                _moveAt = at;
                _bonusMs = 0;
                _bonusTotal = extraMs;
                _stallMs = extraMs;
                _rush = Math.Clamp(extraMs / DetourMs, 1, MaxRush);
            }

            private void StopWalk() {
                _path = null;
                _bonusMs = 0;
                _stallMs = 0;
            }

            // PLANT / stand on a tile the hero is not shown on (it roamed): walk there first. True = `e` waits for it.
            private bool WalkBackFor(TreasureEvent e, TreasurePlayback playback) {
                var stand = e.Type == TreasureEventType.Move && e.Path.Count == 0 && e.RoamUntil < 0;
                if (!Active || (e.Type != TreasureEventType.Plant && !stand)) {
                    return false;
                }
                Evaluate(e.At);
                if (Tile == e.Hero.Cell) {
                    return false;
                }
                var path = playback.FindPath(Player, Tile, e.Hero.Cell);
                if (path == null) {
                    return false;
                }
                _roam = false;
                Walk(Tile, path, e.At, path.Count * _stepMs);
                return true;
            }

            private void Start(TreasureEvent e, TreasurePlayback playback, bool fastForward) {
                // A new action always starts on the tile the hero reached at e.At (re-plan, plant, stop).
                Evaluate(e.At);
                var wasActive = Active;
                _keptPlace = false;
                _roam = false;
                _roamTo = null;
                switch (e.Type) {
                    case TreasureEventType.Move:
                        Active = true;
                        if (e.StepMs > 0) {
                            _stepMs = e.StepMs;
                        }
                        if (e.RoamUntil >= 0) {
                            // The hero keeps its place and wanders from there; the server tile is only where it must return.
                            StopWalk();
                            _roam = true;
                            _roamUntil = e.RoamUntil;
                            _anchor = e.Hero.Cell;
                            if (!wasActive) {
                                Tile = e.Hero.Cell;
                            }
                            break;
                        }
                        if (e.Path.Count == 0) {
                            StopWalk();
                            Tile = e.Hero.Cell;
                            break;
                        }
                        if (!fastForward && wasActive && Tile != e.Hero.Cell) {
                            // Shown elsewhere (it roamed): walk straight to the same destination, T waits for the extra tiles.
                            var path = playback.FindPath(Player, Tile, e.Path[e.Path.Count - 1]);
                            if (path != null) {
                                var extraMs = Math.Max(0, path.Count - e.Path.Count) * _stepMs;
                                if (path.Count == 0) {
                                    StopWalk();
                                } else {
                                    Walk(Tile, path, e.At, extraMs);
                                }
                                break;
                            }
                        }
                        Tile = e.Hero.Cell;
                        Walk(Tile, e.Path, e.At, 0);
                        break;
                    case TreasureEventType.Plant:
                        Active = true;
                        Tile = e.Hero.Cell;
                        StopWalk();
                        playback.OnPlantPlayed(this, e, fastForward);
                        break;
                    case TreasureEventType.HeroJoin:
                        Active = true;
                        // Already shown on the map (asleep): it walks from there, the server's spawn tile is random.
                        Tile = playback.ShownTile(this) ?? e.Hero.Cell;
                        StopWalk();
                        break;
                    case TreasureEventType.HeroLeave:
                        StopWalk();
                        Active = false;
                        playback.OnLeavePlayed(this, e);
                        StopPuppet();
                        break;
                }
                _position = TileCenter(Tile);
                _direction = Vector2.zero;
            }

            private void AdvanceRoamStep(double deltaMs) {
                if (_roamTo == null) {
                    return;
                }
                _roamMs += deltaMs;
                if (_roamMs < _stepMs) {
                    return;
                }
                _roamMs -= _stepMs;
                Tile = _roamTo.Value;
                _roamTo = null;
            }

            private void StartRoamStep(TreasurePlayback playback) {
                if (_roamTo != null) {
                    return;
                }
                var waiting = _queue.Count > 0 && _queue.Peek().At <= T;
                if (_roam && Active && _path == null && !waiting) {
                    _roamTo = playback.PickRoamStep(Player, Tile, _anchor, T, _roamUntil, _stepMs, ref _roamGoal);
                }
                if (_roamTo == null) {
                    _roamMs = 0;
                }
            }

            private void Evaluate() {
                Evaluate(T);
            }

            private void Evaluate(long t) {
                if (_path == null) {
                    _position = TileCenter(Tile);
                    _direction = Vector2.zero;
                    if (_roamTo != null) {
                        var done = Mathf.Clamp01((float) (_roamMs / _stepMs));
                        _position = Vector2.Lerp(_position, TileCenter(_roamTo.Value), done);
                        _direction = _roamTo.Value - Tile;
                    }
                    return;
                }
                var elapsed = Math.Max(0, t - _moveAt + _bonusMs);
                if (elapsed >= _path.Count * _stepMs) {
                    Tile = _path[_path.Count - 1];
                    StopWalk();
                    _position = TileCenter(Tile);
                    _direction = Vector2.zero;
                    return;
                }
                var k = (int) (elapsed / _stepMs);
                var from = k == 0 ? _from : _path[k - 1];
                var to = _path[k];
                var fraction = (float) ((elapsed - k * _stepMs) / _stepMs);
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
