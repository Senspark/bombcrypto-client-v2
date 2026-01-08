using System;
using System.Collections.Generic;
using System.Linq;

using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.Delta;
using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Info;
using BLPvpMode.Engine.Manager;
using BLPvpMode.Engine.Strategy.State;
using BLPvpMode.Engine.User;
using BLPvpMode.Engine.Utility;

using Engine.Entities;

using JetBrains.Annotations;

using PvpMode.Services;

using Scenes.PvpModeScene.Scripts;

using Senspark;

using UnityEngine;

namespace BLPvpMode.Manager {
    public struct PvpManagerCallback {
        // OnResponseServer
        public Action<IMatchData> OnResponseFinishRound;
        public Action OnResponseStartReady;
        public Action<int[]> OnResponseLatency;
        public Action<int, int> OnUpdateHealth;
        public Action<IPvpResultInfo> OnResponseFinishMatch;
        public Action<int, int> OnResponseUseEmoji;
        public Action<IFallingBlockInfo[]> OnResponseFallingBlock;

        // OnChangeState
        public Action<int, HeroDamageSource> OnKillHero;
        public Action<int, bool, HeroEffectReason> SetShielded;
        public Action<int, bool, HeroEffectReason> OnChangeStateImprisoned;
        public Action<int, int, Dictionary<Direction, int>> OnExplodeBomb;
        public Action<int, HeroItem, int> AddItemToPlayer;
        public Action<int, HeroEffect, int> SetSkullHeadToPlayer;
        public Action<Vector2Int, ItemType> AddItem;
        public Action<Vector2Int> RemoveBlock;
        public Action<int, int> RemoveBomb;
        public Action<Vector2Int> RemoveItem;
        public Action<int, int, Vector2Int, int, BombReason> SpawnBomb;
        public Action<long, int, Vector2, Direction> SyncPosPlayer;
    }

    public class BLPvpClientBridge : INetworkStats, IDisposable {
        [NotNull]
        private readonly ILogManager _logManager;

        [NotNull]
        private readonly IUser _user;

        [NotNull]
        private readonly IMatch _match;

        private readonly PvpStorageData _pvpStorageData;

        /// <summary>
        /// Latencies of all users.
        /// </summary>
        private int[] _latencies;

        /// <summary>
        /// Time deltas of all users.
        /// </summary>
        private int[] _timeDelta;

        private readonly List<IMatchObserveData> _dataStateWait;

        private readonly ObserverHandle _handle;
        private bool _firstData;
        private readonly IMatchState _firstMatchState;
        private readonly IMatchStateComparer _matchStateComparer;
        private bool _disposed;

        public PvpManagerCallback PvpCallback { get; set; }

        public BLPvpClientBridge(
            ILogManager logManager,
            [NotNull] IUser user,
            [NotNull] IMapInfo mapInfo,
            PvpStorageData pvpStorageData
        ) {
            _logManager = logManager;
            _user = user;
            _pvpStorageData = pvpStorageData;
            _dataStateWait = new List<IMatchObserveData>();
            _handle = new ObserverHandle();
            _handle.AddObserver(user, new UserObserver {
                OnPing = OnPing,
                OnStartReady = OnStartReady,
                OnUseEmoji = OnUseEmoji,
                OnStartRound = OnStartRound,
                OnFallingBlocks = OnFallingBlock,
                OnChangeState = OnChangeState,
                OnFinishRound = OnFinishRound,
                OnFinishMatch = OnFinishMatch,
            });
            _matchStateComparer = new DefaultMatchStateComparer();
            var matchInfo = _user.MatchInfo;
            var timeManager = new EpochTimeManager();
            var controllers = matchInfo.Info.Select((info, index) =>
                (IParticipantController) new ParticipantController(
                    info: info,
                    teamId: Array.FindIndex(matchInfo.Team, it => it.Slots.Contains(index)),
                    slot: index,
                    timeManager: timeManager
                )
            ).ToArray();
            var matchState = new MatchState(
                heroState: new HeroManagerState(
                    _user.MatchInfo.Info
                        .Select((value, index) => (value, index))
                        .ToDictionary(
                            it => it.index,
                            it => (IHeroState) new HeroState(
                                isAlive: true,
                                position: mapInfo.StartingPositions[it.index],
                                direction: Direction.Down,
                                health: it.value.Hero.Health,
                                damageSource: HeroDamageSource.Null,
                                items: new Dictionary<HeroItem, int>(),
                                effects: new Dictionary<HeroEffect, IHeroEffectState>()
                            )
                        )
                ),
                bombState: new BombManagerState(0, new Dictionary<int, IBombState>()),
                mapState: MapManagerState.Create(mapInfo, true)
            );
            var heroListener = new HeroListener {
                OnDamaged = (hero, amount, source) =>
                    _logManager.Log($"[HeroListener:OnDamaged] slot={hero.Slot} amount={amount} source={source}"),
                OnHealthChanged = (hero, amount, oldAmount) => { //
                    // FIXME: call directly will trigger invincible animation.
                    if (amount == oldAmount) {
                        return;
                    }
                    PvpCallback.OnUpdateHealth(hero.Slot, amount);
                },
                OnItemChanged = (hero, item, amount, oldAmount) => {
                    if (_user.MatchInfo.Slot == hero.Slot) {
                        // FIXME.
                        // _pvpStorageData.ItemsTake[result.Item]++;
                    }
                    PvpCallback.AddItemToPlayer(hero.Slot, item, amount - oldAmount);
                },
                OnEffectBegan = (hero, effect, reason, duration) => {
                    _logManager.Log($"[HeroListener:OnEffectBegan] slot={hero.Slot} effect={effect} reason={reason}");
                    if (effect == HeroEffect.Shield) {
                        PvpCallback.SetShielded(hero.Slot, true, reason);
                    }
                    if (effect == HeroEffect.Imprisoned) {
                        PvpCallback.OnChangeStateImprisoned(hero.Slot, true, reason);
                    }
                    if (effect
                        is HeroEffect.ReverseDirection
                        or HeroEffect.SpeedTo1
                        or HeroEffect.SpeedTo10
                        or HeroEffect.PlantBombRepeatedly) {
                        if (_user.MatchInfo.Slot == hero.Slot) {
                            _pvpStorageData.ItemSkullHeadTake++;
                        }
                        PvpCallback.SetSkullHeadToPlayer(hero.Slot, effect, duration);
                    }
                },
                OnEffectEnded = (hero, effect, reason) => {
                    _logManager.Log($"[HeroListener:OnEffectEnded] slot={hero.Slot} effect={effect} reason={reason}");
                    if (effect == HeroEffect.Shield) {
                        PvpCallback.SetShielded(hero.Slot, false, reason);
                    }
                    if (effect == HeroEffect.Imprisoned) {
                        PvpCallback.OnChangeStateImprisoned(hero.Slot, false, reason);
                    }
                },
            };
            var bombListener = new BombListener {
                OnAdded = (bomb, reason) => {
                    _logManager.Log(
                        $"[BombListener:OnAdded] slot={bomb.Slot} id={bomb.Id} x={bomb.Position.x} y={bomb.Position.y} reason={reason}");
                    switch (reason) {
                        case BombReason.Planted or BombReason.PlantedBySkull: {
                            var position = new Vector2Int(
                                Mathf.FloorToInt(bomb.Position.x),
                                Mathf.FloorToInt(bomb.Position.y)
                            );
                            PvpCallback.SpawnBomb(bomb.PlantTimestamp, bomb.Slot, position, bomb.Id, reason);
                            break;
                        }
                        default: {
                            Debug.LogError($"Invalid bomb reason: [false, true] {reason}");
                            break;
                        }
                    }
                },
                OnRemoved = (bomb, reason) => {
                    _logManager.Log(
                        $"[BombListener:OnRemoved] slot={bomb.Slot} id={bomb.Id} x={bomb.Position.x} y={bomb.Position.y} reason={reason}");
                    switch (reason) {
                        case BombReason.Exploded: {
                            break;
                        }
                        case BombReason.Removed: {
                            PvpCallback.RemoveBomb(bomb.Slot, bomb.Id);
                            break;
                        }
                        default: {
                            Debug.LogError($"Invalid bomb reason: [true, false] {reason}");
                            break;
                        }
                    }
                },
                OnExploded = (bomb, ranges) => {
                    _logManager.Log(
                        $"[BombListener:OnExploded] slot={bomb.Slot} id={bomb.Id} x={bomb.Position.x} y={bomb.Position.y} ranges={ranges[Direction.Left]} {ranges[Direction.Right]} {ranges[Direction.Up]} {ranges[Direction.Down]}");
                    PvpCallback.OnExplodeBomb(bomb.Slot, bomb.Id, ranges);
                },
            };
            var mapListener = new MapListener {
                OnAdded = (block, reason) => {
                    _logManager.Log(
                        $"[MapListener:OnAdded] x={block.Position.x} y={block.Position.y} type={block.Type} reason={reason}");
                    if (reason == BlockReason.Falling) {
                        // Handled separately.
                        return;
                    }
                    if (reason == BlockReason.Dropped) {
                        PvpCallback.AddItem(block.Position, ItemMappers[block.Type]);
                        return;
                    }
                },
                OnRemoved = (block, reason) => {
                    _logManager.Log(
                        $"[MapListener:OnRemoved] x={block.Position.x} y={block.Position.y} type={block.Type} reason={reason}");
                    if (block.Type == BlockType.Soft) {
                        PvpCallback.RemoveBlock(block.Position);
                        return;
                    }
                    if (block.Type is >= BlockType.BombUp and <= BlockType.PlatinumChest) {
                        PvpCallback.RemoveItem(block.Position);
                        return;
                    }
                },
            };
            _firstMatchState = matchState;
            _match = new Match(
                controllers: controllers,
                teamInfo: matchInfo.Team,
                heroInfo: matchInfo.Info.Select(it => it.Hero).ToArray(),
                mapInfo: mapInfo,
                heroConfig: new HeroConfig(),
                initialState: matchState,
                logger: _logManager,
                timeManager: timeManager,
                random: new DefaultRandom(0),
                heroListener: heroListener,
                bombListener: bombListener,
                mapListener: mapListener
            );
        }

        ~BLPvpClientBridge() => Dispose(false);

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing) {
            if (_disposed) {
                return;
            }
            if (disposing) {
                _handle.Dispose();
            }
            _disposed = true;
        }

        public int GetLatency(int slot) {
            return _latencies == null ? 0 : _latencies[slot];
        }

        public int GetTimeDelta(int slot) {
            return _timeDelta == null ? 0 : _timeDelta[slot];
        }

        private void OnPing(IPingPongData data) {
            _latencies = data.Latencies;
            _timeDelta = data.TimeDelta;
            PvpCallback.OnResponseLatency(data.Latencies);
        }

        private void OnStartReady() {
            PvpCallback.OnResponseStartReady();
        }

        private void OnUseEmoji(IUseEmojiData data) {
            if (data.MatchId != _user.MatchInfo.Id) {
                return;
            }
            PvpCallback.OnResponseUseEmoji(data.Slot, data.ItemId);
        }

        private void OnFallingBlock(IFallingBlockData data) {
            if (data.MatchId != _user.MatchInfo.Id) {
                return;
            }
            PvpCallback.OnResponseFallingBlock(data.Blocks);
        }

        private void OnChangeState(IMatchObserveData data) {
            _dataStateWait.Add(data);
        }

        private void OnStartRound(IMatchStartData data) {
            _firstData = true;
        }

        private void OnFinishRound(IMatchFinishData data) {
            PvpCallback.OnResponseFinishRound(data.MatchData);
        }

        private void OnFinishMatch(IPvpResultInfo data) {
            if (data.Id != _user.MatchInfo.Id) {
                return;
            }
            PvpCallback.OnResponseFinishMatch(data);
        }

        private static readonly Dictionary<BlockType, ItemType> ItemMappers = new() {
            [BlockType.BombUp] = ItemType.BombUp,
            [BlockType.FireUp] = ItemType.FireUp,
            [BlockType.Boots] = ItemType.Boots,
            [BlockType.Shield] = ItemType.Armor,
            [BlockType.Skull] = ItemType.SkullHead,
            [BlockType.GoldX1] = ItemType.GoldX1,
            [BlockType.GoldX1] = ItemType.GoldX1,
            [BlockType.GoldX5] = ItemType.GoldX1,
            [BlockType.BronzeChest] = ItemType.BronzeChest,
            [BlockType.SilverChest] = ItemType.SilverChest,
            [BlockType.GoldChest] = ItemType.GoldChest,
            [BlockType.PlatinumChest] = ItemType.PlatinumChest,
        };

        public void Step(float delta) {
            if (_dataStateWait.Count <= 0) {
                return;
            }
            foreach (var data in _dataStateWait) {
                ProcessChangeState(data);
            }
            _dataStateWait.Clear();
        }

        private void ProcessChangeState(IMatchObserveData data) {
            if (data.MatchId != _user.MatchInfo.Id) {
                return;
            }
            if (data.BombDelta.Length > 0) {
                // Use to debug blocks.
                // Debug.Log(
                //     $@"t={data.Timestamp} blocks={data.BombDelta.Length} items={string.Join("|", data.BombDelta.Select(item => {
                //         var state = BombState.Decode(item.State);
                //         var lastState = BombState.Decode(item.LastState);
                //         return $"slot={item.Slot} id={item.Id} current={state.IsAlive} {state.Position} {state.Reason} last={lastState.IsAlive} {lastState.Position} {lastState.Reason}";
                //     }))}");
            }
            var clientTimestamp = data.Timestamp - _timeDelta[_user.MatchInfo.Slot];
            var delta = (IMatchStateDelta) new MatchStateDelta(data.HeroDelta, data.BombDelta, data.BlockDelta);
            if (_firstData) {
                _firstData = false;
                // StartRound => send full changed data since start (not incremental changed data).
                var serverState = _firstMatchState.Apply(MatchState.DecodeDelta(delta));
                delta = _matchStateComparer.Compare(serverState, _match.State);
            }
            if (delta == null) {
                // StartRound => send delta data => no change.
                return;
            }
            var matchState = MatchState.DecodeDelta(delta);
            _match.ApplyState(matchState);
            foreach (var hero in data.HeroDelta) {
                // Base state.
                if (hero.Base != null) {
                    var state = HeroBaseState.Decode(hero.Base.State);
                    // Alive state.
                    if (!state.IsAlive) {
                        PvpCallback.OnKillHero(hero.Slot, state.DamageSource);
                        if (_user.MatchInfo.Slot == hero.Slot) {
                            _pvpStorageData.ReasonLose = (PvpDamageDealerType) state.DamageSource;
                        }
                    }
                }
                // Position state.
                if (hero.Position != null) {
                    var state = HeroPositionState.Decode(hero.Position.State);
                    // Update position.
                    // NOTE: only affects observers.
                    PvpCallback.SyncPosPlayer(clientTimestamp, hero.Slot, state.Position, state.Direction);
                }
            }
        }
    }
}