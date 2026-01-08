using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using BLPvpMode.Engine.Config;
using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Info;
using BLPvpMode.Engine.User;
using BLPvpMode.Engine.Utility;
using BLPvpMode.Manager;
using BLPvpMode.Manager.Api;

using JetBrains.Annotations;

using PvpMode.Services;

using Senspark;

using UnityEngine;
using UnityEngine.Assertions;

namespace BLPvpMode.Engine.Manager {
    internal class MatchManagerInfo {
        public IMatch Match { get; set; }
        public IMapInfo MapInfo { get; set; }
        public StepTimeManager TimeManager { get; set; }
        public IStateManager StateManager { get; set; }
        public IObserveDataFactory DataFactory { get; set; }
        public ICommandManager CommandManager { get; set; }
        public IUpdater Updater { get; set; }
    }

    internal class HeroConfig : IHeroConfig {
        public int ExplodeDuration { get; set; }
        public int ShieldedDuration { get; set; }
        public int InvincibleDuration { get; set; }
        public int ImprisonedDuration { get; set; }
        public int SkullEffectDuration { get; set; }
    }

    internal class ResultInfo {
        public bool IsFinished { get; set; }
        public bool IsDraw { get; set; }
        public int WinningTeam { get; set; }
    }

    public class DefaultMatchController : IMatchController {
        private const int ReadyTimeOutDuration = 30000;
        private const int StepDuration = 16;

        [NotNull]
        private readonly IMatchFactory _matchFactory;

        [NotNull]
        private readonly IMatchInfo _matchInfo;

        [NotNull]
        private readonly IScheduler _scheduler;

        [NotNull]
        private readonly IMatchManager _matchManager;

        [NotNull]
        private readonly IMessageBridge _messageBridge;

        [NotNull]
        private readonly IMapGenerator _mapGenerator;

        [NotNull]
        private readonly ITimeManager _timeManager = new EpochTimeManager();

        [NotNull]
        private readonly ILogManager _logger;

        [NotNull]
        private readonly IRandom _random;

        [NotNull]
        private readonly IParticipantController[] _participantControllers;

        [NotNull]
        private readonly IUserController[] _controllers;

        [NotNull]
        private readonly INetworkManager _networkManager;

        [NotNull]
        private readonly IPacketManager _packetManager = new DefaultPacketManager();

        [NotNull]
        private readonly IMatchData _matchData;

        [NotNull]
        private readonly IHeroListener _heroListener;

        [NotNull]
        private readonly IBombListener _bombListener;

        [NotNull]
        private readonly IMapListener _mapListener;

        private int _networkStepDuration = 0;

        private IMatch _match;
        private IMapInfo _mapInfo;
        private StepTimeManager _matchTimeManager;
        private IStateManager _stateManager;
        private IObserveDataFactory _dataFactory;
        private ICommandManager _commandManager;
        private IUpdater _updater;
        private IPvpResultInfo _matchResult;

        private IUser[] ActiveUsers => _controllers
            .Select(it => it.User)
            .Where(it => it != null)
            .ToArray();

        public DefaultMatchController(
            [NotNull] IMatchFactory matchFactory,
            [NotNull] IMatchInfo matchInfo,
            long startTimestamp,
            [NotNull] ILogManager logger,
            [NotNull] IScheduler scheduler,
            [NotNull] IMatchManager matchManager,
            [NotNull] IMessageBridge messageBridge,
            [NotNull] IMapGenerator mapGenerator
        ) {
            _matchFactory = matchFactory;
            _matchInfo = matchInfo;
            _logger = logger;
            _scheduler = scheduler;
            _matchManager = matchManager;
            _messageBridge = messageBridge;
            _mapGenerator = mapGenerator;
            _random = new DefaultRandom(startTimestamp);
            _participantControllers = _matchInfo.Info.Select((info, index) =>
                (IParticipantController) new ParticipantController(
                    info: info,
                    teamId: Array.FindIndex(matchInfo.Team, it => it.Slots.Contains(index)),
                    slot: index,
                    timeManager: _timeManager
                )
            ).ToArray();
            _controllers = _participantControllers
                .Select(it => (IUserController) it)
                .ToArray();
            _networkManager = new DefaultNetworkManager(
                _participantControllers,
                _messageBridge,
                _timeManager,
                300,
                30,
                3
            );
            _matchData = new MatchData(
                id: _matchInfo.Id,
                status: (int) MatchStatus.MatchStarted,
                observerCount: 0,
                startTimestamp: startTimestamp,
                readyStartTimestamp: 0,
                roundStartTimestamp: 0,
                round: 0,
                results: new List<IMatchResultInfo>()
            );
            _heroListener = new HeroListener {
                OnDamaged = (hero, amount, source) =>
                    _logger.Log($"[HeroListener:OnDamaged] slot={hero.Slot} amount={amount} source={source}"),
                OnItemChanged = (hero, item, amount, oldAmount) =>
                    _logger.Log(
                        $"[HeroListener:onItemChanged] slot={hero.Slot} item={item} amount={amount} oldAmount={oldAmount}"),
                OnEffectBegan = (hero, effect, reason, duration) =>
                    _logger.Log($"[HeroListener:OnEffectBegan] slot={hero.Slot} effect={effect} reason={reason}"),
                OnEffectEnded = (hero, effect, reason) =>
                    _logger.Log($"[HeroListener:OnEffectEnded] slot={hero.Slot} effect={effect} reason={reason}"),
            };
            _bombListener = new BombListener {
                OnAdded = (bomb, reason) =>
                    _logger.Log(
                        $"[BombListener:OnAdded] slot={bomb.Slot} id={bomb.Id} x={bomb.Position.x} y={bomb.Position.y} reason={reason}"),
                OnRemoved = (bomb, reason) =>
                    _logger.Log(
                        $"[BombListener:OnRemoved] slot={bomb.Slot} id={bomb.Id} x={bomb.Position.x} y={bomb.Position.y} reason={reason}"),
                OnExploded = (bomb, ranges) =>
                    _logger.Log(
                        $"[BombListener:OnExploded] slot={bomb.Slot} id={bomb.Id} x={bomb.Position.x} y={bomb.Position.y} ranges={ranges[Direction.Left]} {ranges[Direction.Right]} {ranges[Direction.Up]} {ranges[Direction.Down]}"),
            };
            _mapListener = new MapListener {
                OnAdded =
                    (block, reason) =>
                        _logger.Log(
                            $"[MapListener:OnAdded] x={block.Position.x} y={block.Position.y} type={block.Type} reason={reason}"),
                OnRemoved = (block, reason) =>
                    _logger.Log(
                        $"[MapListener:OnRemoved] x={block.Position.x} y={block.Position.y} type={block.Type} reason={reason}"),
            };
        }

        public void Initialize() {
            _scheduler.Schedule(nameof(Step), 0, StepDuration, () => { //
                Step(StepDuration);
            });
            _scheduler.Schedule(nameof(StepNetwork), 0, 300, () => { //
                StepNetwork(300);
            });
            StartReady();
        }

        private bool CheckStatus(MatchStatus status) {
            return _matchData.Status == status;
        }

        public void JoinRoom(IUser user) {
            _logger.Log($"[MatchController:JoinRoom] user={user.MatchInfo.Slot} isPlayer={user.IsParticipant}");
            void EndAction() {
                if (user.IsParticipant) {
                    var info = user.MatchInfo;
                    _participantControllers[info.Slot].Join(user);
                } else {
                    // FIXME.
                }
            }
            if (CheckStatus(MatchStatus.Ready)) {
                _messageBridge.StartReady(new[] { user });
                EndAction();
            }
            if (CheckStatus(MatchStatus.Started)) {
                SendStartRound(false, new[] { user });
                var changeData = _stateManager.AccumulativeChangeData;
                if (changeData != null) {
                    var data = new MatchObserveData(
                        id: -1,
                        timestamp: _timeManager.Timestamp,
                        matchId: _matchInfo.Id,
                        heroDelta: changeData.Hero,
                        bombDelta: changeData.Bomb,
                        blockDelta: changeData.Block
                    );
                    SendChangeData(data, false, new[] { user });
                }
                EndAction();
            }
            if (CheckStatus(MatchStatus.Finished)) {
                var data = new MatchFinishData {
                    MatchData = _matchData, //
                };
                _messageBridge.FinishRound(data, new[] { user });
                EndAction();
            }
            if (CheckStatus(MatchStatus.MatchFinished)) {
                _messageBridge.FinishMatch(_matchResult, new[] { user });
                EndAction();
            }
        }

        public void LeaveRoom(IUser user) {
            _logger.Log($"[MatchController:LeaveRoom] user={user.MatchInfo.Slot} isPlayer={user.IsParticipant}");
            if (user.IsParticipant) {
                var info = user.MatchInfo;
                _participantControllers[info.Slot].Leave();
            } else {
                // FIXME.
            }
        }

        private void StartReady() {
            _logger.Log("[MatchController:StartReady]");
            _scheduler.ScheduleOnce(nameof(ForceReady), ReadyTimeOutDuration, () => { //
                ForceReady();
            });
            _matchData.Status = MatchStatus.Ready;
            _matchData.ReadyStartTimestamp = _timeManager.Timestamp;
            _participantControllers.ForEach(it => { //
                it.Reset();
            });
            _messageBridge.StartReady(ActiveUsers);
        }

        private void FinishReady() {
            _logger.Log("[MatchController:FinishReady]");
            Assert.IsTrue(CheckStatus(MatchStatus.Ready), "Invalid match status");
            _scheduler.Clear(nameof(ForceReady));
            _messageBridge.FinishReady(ActiveUsers);
            StartRound();
        }

        private void ForceReady() {
            _logger.Log("[MatchController:ForceReady]");
            for (var i = 0; i < _participantControllers.Length; ++i) {
                Ready(i);
            }
        }

        public void Ready(IUser user) {
            Assert.IsTrue(user.IsParticipant, "User is not a participant");
            var info = user.MatchInfo;
            Ready(info.Slot);
        }

        private void Ready(int slot) {
            _logger.Log($"[MatchController:Ready] slot={slot}");
            Assert.IsTrue(CheckStatus(MatchStatus.Ready), "Invalid match status");
            var item = _participantControllers[slot];
            if (item.IsReady) {
                return;
            }
            item.Ready();
            var data = new MatchReadyData {
                MatchId = _matchInfo.Id, //
                Slot = slot,
            };
            _messageBridge.Ready(data, ActiveUsers);
        }

        private MatchManagerInfo CreateMatch() {
            var matchTimeManager = new StepTimeManager();
            var heroConfig = new HeroConfig {
                ExplodeDuration = 3000,
                ShieldedDuration = 8000,
                InvincibleDuration = 3000,
                ImprisonedDuration = 5000,
                SkullEffectDuration = 10000,
            };
            var mapInfo = _mapGenerator.Generate();
            var match = new Match(
                controllers: _participantControllers,
                teamInfo: _matchInfo.Team,
                heroInfo: _matchInfo.Info.Select(it => it.Hero).ToArray(),
                mapInfo: mapInfo,
                heroConfig: heroConfig,
                initialState: new MatchState(
                    heroState: new HeroManagerState(
                        _matchInfo.Info.AssociateIndex((value, index) =>
                            (index, (IHeroState) new HeroState(
                                    isAlive: true,
                                    position: mapInfo.StartingPositions[index],
                                    direction: Direction.Down,
                                    health: value.Hero.Health,
                                    damageSource: HeroDamageSource.Null,
                                    items: new Dictionary<HeroItem, int>(),
                                    effects: new Dictionary<HeroEffect, IHeroEffectState>()
                                )
                            )
                        )
                    ),
                    bombState: new BombManagerState(0, new Dictionary<int, IBombState>()),
                    mapState: MapManagerState.Create(mapInfo, true)
                ),
                logger: _logger,
                timeManager: matchTimeManager,
                random: _random,
                heroListener: _heroListener,
                bombListener: _bombListener,
                mapListener: _mapListener
            );
            var blockListener = new FallingBlockManagerListener {
                OnBlockDidFall = position => {
                    try {
                        var bomb = match.BombManager.GetBomb(position);
                        // ReSharper disable once UseNullPropagation
                        if (bomb != null) {
                            bomb.Kill(BombReason.Removed);
                        }
                        var block = match.MapManager.GetBlock(position);
                        // ReSharper disable once UseNullPropagation
                        if (block != null) {
                            block.Kill(BlockReason.Removed);
                        }
                        match.MapManager.AddBlock(Block.CreateHardBlock(position, BlockReason.Falling, _logger,
                            match.MapManager));
                    } catch (Exception ex) {
                        // Block exist.
                    }
                    match.HeroManager.DamageFallingBlock(position);
                },
                OnBuffered = blocks => {
                    _logger.Log($"[FallingBlockManagerListener:OnBuffered] blocks={blocks.Count}");
                    var data = new FallingBlockData(_matchInfo.Id, blocks.ToArray());
                    _messageBridge.BufferFallingBlocks(data, ActiveUsers);
                }
            };
            var fallingBlockManager = new FallingBlockManager(mapInfo.FallingBlocks, blockListener);
            var stateManager = new DefaultStateManager(match);
            var dataFactory = new ObserveDataFactory(_matchInfo);
            var commandManager =
                new DefaultCommandManager(_logger, match, _matchData, _packetManager, stateManager, dataFactory);
            var updater = new DefaultUpdater(new IUpdater[] { fallingBlockManager, match });
            return new MatchManagerInfo {
                Match = match,
                MapInfo = mapInfo,
                TimeManager = matchTimeManager,
                StateManager = stateManager,
                DataFactory = dataFactory,
                CommandManager = commandManager,
                Updater = updater,
            };
        }

        private void StartRound() {
            _logger.Log("[MatchController:StartRound]");
            Assert.IsTrue(CheckStatus(MatchStatus.Ready), "Invalid match status");
            _matchData.Status = MatchStatus.Started;
            _matchData.RoundStartTimestamp = _timeManager.Timestamp;
            var info = CreateMatch();
            _match = info.Match;
            _mapInfo = info.MapInfo;
            _matchTimeManager = info.TimeManager;
            _stateManager = info.StateManager;
            _dataFactory = info.DataFactory;
            _commandManager = info.CommandManager;
            _updater = info.Updater;
            SendStartRound(true, ActiveUsers);
        }

        private void SendStartRound(bool report, IUser[] users) {
            var data = new MatchStartData(_matchData, _mapInfo);
            if (report) {
                // FIXME.
            }
            _messageBridge.StartRound(data, users);
        }

        private void FinishRound(bool isDraw, int winningTeam) {
            _logger.Log($"[MatchController:FinishRound] isDraw={isDraw} winningTeam={winningTeam}");
            Assert.IsTrue(CheckStatus(MatchStatus.Started), "Invalid match status");
            _matchData.Status = MatchStatus.Finished;
            ++_matchData.Round;
            var resultInfo = GenerateRoundResult(isDraw, winningTeam);
            _matchData.Results.Add(resultInfo);
            var data = new MatchFinishData {
                MatchData = _matchData, //
            };
            _messageBridge.FinishRound(data, ActiveUsers);
        }

        public void Quit(IUser user) {
            Assert.IsTrue(user.IsParticipant, "User is not a participant");
            var info = user.MatchInfo;
            Quit(info.Slot);
        }

        private void Quit(int slot) {
            _logger.Log($"[MatchController:Quit] slot={slot}");
            _participantControllers[slot].Quit();
        }

        private ResultInfo CheckRoundResult() {
            Assert.IsTrue(CheckStatus(MatchStatus.Started), "Invalid match status");
            var userStates = _participantControllers.Select((it, index) => {
                var state = _match.HeroManager.GetHero(index).State;
                return state.IsAlive && !it.IsQuited;
            }).ToArray();
            var teamStates = _matchInfo.Team.Select(team => // 
                team.Slots.Any(it => userStates[it])
            ).ToArray();
            var (isFinished, isDraw, winningTeam) = teamStates.Count(it => it) switch {
                0 => (true, true, -1),
                1 => (true, false, Array.FindIndex(teamStates, it => it)),
                _ => (false, false, -1),
            };
            return new ResultInfo {
                IsFinished = isFinished, //
                IsDraw = isDraw,
                WinningTeam = winningTeam,
            };
        }

        private ResultInfo CheckMatchResult() {
            var teamScores = _matchInfo.Team.Select((_, index) => 0).ToArray();
            _matchData.Results.ForEach(it => {
                if (it.IsDraw) {
                    // Ignored.
                } else {
                    ++teamScores[it.WinningTeam];
                }
            });
            var bestScores = teamScores.OrderByDescending(it => it).ToArray();
            var maxScore = bestScores[0];
            bool isFinished;
            bool isDraw;
            int winningTeam;
            var rule = _matchInfo.Rule;
            var remainingRound = rule.Round - _matchData.Round;
            if (remainingRound > 0) {
                isDraw = false;
                if (bestScores[1] + remainingRound < maxScore) {
                    isFinished = true;
                    winningTeam = Array.FindIndex(teamScores, it => it == maxScore);
                } else {
                    isFinished = false;
                    winningTeam = -1;
                }
            } else {
                (isFinished, isDraw, winningTeam) = teamScores.Count(it => it == maxScore) switch {
                    1 => (true, false, Array.FindIndex(teamScores, it => it == maxScore)),
                    _ => rule.CanDraw
                        ? (true, true, -1)
                        : (false, false, -1)
                };
            }
            return new ResultInfo {
                IsFinished = isFinished, //
                IsDraw = isDraw,
                WinningTeam = winningTeam,
            };
        }

        private IMatchResultInfo GenerateRoundResult(bool isDraw, int winningTeam) {
            return new MatchResultInfo {
                IsDraw = isDraw,
                WinningTeam = winningTeam,
                Scores = _matchInfo.Team.Select((_, index) => index == winningTeam ? 1 : 0).ToArray(),
                Duration = (int) (_timeManager.Timestamp - _matchData.RoundStartTimestamp),
                StartTimestamp = _matchData.RoundStartTimestamp,
                Info = _participantControllers.Select((it, index) => {
                    return (IMatchResultUserInfo) new MatchResultUserInfo {
                        ServerId = it.Info.ServerId,
                        IsTest = it.Info.IsTest,
                        IsBot = it.Info.IsBot,
                        TeamId = Array.FindIndex(_matchInfo.Team, team => team.Slots.Contains(index)),
                        UserId = it.Info.UserId,
                        Username = it.Info.Username,
                        MatchCount = it.Info.MatchCount,
                        WinMatchCount = it.Info.WinMatchCount,
                        Boosters = it.Info.Boosters,
                        UsedBoosters = new Dictionary<int, int>(), // FIXME
                        Quit = it.IsQuited,
                    };
                }).ToArray(),
            };
        }

        private IMatchResultInfo GenerateMatchResult(bool isDraw, int winningTeam) {
            return new MatchResultInfo {
                IsDraw = isDraw,
                WinningTeam = winningTeam,
                Scores = _matchInfo.Team.Select((_, teamId) =>
                    _matchData.Results
                        .Select(result => result.WinningTeam)
                        .Count(it => it == teamId)
                ).ToArray(),
                Duration = _matchData.Results.Sum(it => it.Duration),
                StartTimestamp = _matchData.Results[0].StartTimestamp,
                Info = _participantControllers.Select((it, index) => {
                    return (IMatchResultUserInfo) new MatchResultUserInfo {
                        ServerId = it.Info.ServerId,
                        IsTest = it.Info.IsTest,
                        IsBot = it.Info.IsBot,
                        TeamId = Array.FindIndex(_matchInfo.Team, team => team.Slots.Contains(index)),
                        UserId = it.Info.UserId,
                        Username = it.Info.Username,
                        MatchCount = it.Info.MatchCount,
                        WinMatchCount = it.Info.WinMatchCount,
                        Boosters = it.Info.Boosters,
                        UsedBoosters = new Dictionary<int, int>(), // FIXME
                        Quit = it.IsQuited,
                    };
                }).ToArray(),
            };
        }

        private IPvpResultInfo CreateResultInfo(IMatchResultInfo resultInfo) {
            var info = new PvpResultInfo(
                id: _matchInfo.Id,
                mode: (int) _matchInfo.Mode,
                isDraw: resultInfo.IsDraw,
                winningTeam: resultInfo.WinningTeam,
                scores: resultInfo.Scores,
                info: resultInfo.Info.Select((userInfo, index) => {
                    var isWinner = resultInfo.WinningTeam == userInfo.TeamId && !resultInfo.IsDraw;
                    return (IPvpResultUserInfo) new PvpResultUserInfo(
                        serverId: userInfo.ServerId,
                        isBot: userInfo.IsBot,
                        teamId: userInfo.TeamId,
                        userId: userInfo.UserId,
                        username: userInfo.Username,
                        rank: 0,
                        point: 0,
                        matchCount: userInfo.MatchCount,
                        winMatchCount: userInfo.WinMatchCount + (isWinner ? 1 : 0),
                        deltaPoint: 0,
                        usedBoosters: new Dictionary<int, int>(),
                        quit: userInfo.Quit,
                        rewards: new Dictionary<int, float>()
                    );
                }).ToArray()
            );
            return info;
        }

        [NotNull]
        private IPvpResultInfo FinishMatch(bool isDraw, int winningTeam) {
            _logger.Log($"[MatchController:FinishMatch] isDraw={isDraw} winningTeam={winningTeam}");
            Assert.IsTrue(CheckStatus(MatchStatus.Finished), $"Invalid match status: {_matchData.Status}");
            _matchData.Status = MatchStatus.MatchFinished;
            var matchResultInfo = GenerateMatchResult(isDraw, winningTeam);
            var resultInfo = CreateResultInfo(matchResultInfo);
            _messageBridge.FinishMatch(resultInfo, ActiveUsers);
            try {
                _matchManager.Finish(resultInfo);
            } catch (Exception ex) {
                _logger.Log($"[MatchController:FinishMatch] exception={ex.Message}");
            }
            return resultInfo;
        }

        public void Ping(IUser user, long timestamp, int requestId) {
            if (user.IsParticipant) {
                var info = user.MatchInfo;
                _networkManager.Pong(_participantControllers[info.Slot], timestamp, requestId);
            } else {
                // FIXME.
            }
        }

        public async Task<IMoveHeroData> MoveHero(IUser user, long timestamp, Vector2 position) {
            Assert.IsTrue(user.IsParticipant, "User is not a participant");
            Assert.IsTrue(CheckStatus(MatchStatus.Started), $"Invalid match status: {_matchData.Status}");
            var info = user.MatchInfo;
            var slot = info.Slot;
            var serverTimestamp = timestamp + _networkManager.TimeDeltas[slot];
            var matchTimestamp = (int) (serverTimestamp - _matchData.RoundStartTimestamp);
            return await _commandManager.MoveHero(slot, matchTimestamp, position);
        }

        public async Task<IPlantBombData> PlantBomb(IUser user, long timestamp) {
            Assert.IsTrue(user.IsParticipant, "User is not a participant");
            Assert.IsTrue(CheckStatus(MatchStatus.Started), $"Invalid match status: {_matchData.Status}");
            var info = user.MatchInfo;
            var slot = info.Slot;
            var serverTimestamp = timestamp + _networkManager.TimeDeltas[slot];
            var matchTimestamp = (int) (serverTimestamp - _matchData.RoundStartTimestamp);
            return await _commandManager.PlantBomb(slot, matchTimestamp);
        }

        public async Task ThrowBomb(IUser user, long timestamp) {
            Assert.IsTrue(user.IsParticipant, "User is not a participant");
            Assert.IsTrue(CheckStatus(MatchStatus.Started), $"Invalid match status: {_matchData.Status}");
            var info = user.MatchInfo;
            var slot = info.Slot;
            var serverTimestamp = timestamp + _networkManager.TimeDeltas[slot];
            var matchTimestamp = (int) (serverTimestamp - _matchData.RoundStartTimestamp);
            await _commandManager.ThrowBomb(slot, matchTimestamp);
        }

        public async Task UseBooster(IUser user, long timestamp, Booster item) {
            Assert.IsTrue(user.IsParticipant, "User is not a participant");
            Assert.IsTrue(CheckStatus(MatchStatus.Started), $"Invalid match status: {_matchData.Status}");
            var info = user.MatchInfo;
            var slot = info.Slot;
            var serverTimestamp = timestamp + _networkManager.TimeDeltas[slot];
            var matchTimestamp = (int) (serverTimestamp - _matchData.RoundStartTimestamp);
            await _commandManager.UseBooster(slot, matchTimestamp, item);
        }

        public void UseEmoji(IUser user, int itemId) {
            Assert.IsTrue(user.IsParticipant, "User is not a participant");
            Assert.IsTrue(CheckStatus(MatchStatus.Started), $"Invalid match status: {_matchData.Status}");
            var info = user.MatchInfo;
            var slot = info.Slot;
            var data = new UseEmojiData(_matchInfo.Id, slot, itemId);
            _messageBridge.UseEmoji(data, ActiveUsers);
        }

        private void SendChangeData(
            IMatchObserveData data,
            bool report,
            IUser[] users
        ) {
            if (data.HeroDelta.Length == 0 &&
                data.BombDelta.Length == 0 &&
                data.BlockDelta.Length == 0) {
                return;
            }
            _packetManager.Add(() => _messageBridge.ChangeState(data, users));
        }

        private void StepNetwork(int delta) {
            _networkManager.Step(delta);
        }

        private void Step(int delta) {
            if (CheckStatus(MatchStatus.Ready)) {
                var readied = _participantControllers.All(it => it.IsReady);
                if (readied) {
                    _networkStepDuration += delta;
                }
                if (_networkStepDuration >= 1000) {
                    FinishReady();
                }
                return;
            }
            if (CheckStatus(MatchStatus.Started)) {
                _updater.Step(delta);
                var dataList = _commandManager.ProcessCommands();
                dataList.ForEach(it => { //
                    SendChangeData(it, true, ActiveUsers);
                });
                var stateDelta = _stateManager.ProcessState();
                if (stateDelta != null) {
                    var observeData = _dataFactory.Generate(_timeManager.Timestamp, stateDelta);
                    SendChangeData(observeData, true, ActiveUsers);
                }
                _matchTimeManager.Step(delta);
                _packetManager.Flush();
                var result = CheckRoundResult();
                if (result.IsFinished) {
                    FinishRound(result.IsDraw, result.WinningTeam);
                }
                return;
            }
            if (CheckStatus(MatchStatus.Finished)) {
                var result = CheckMatchResult();
                if (result.IsFinished) {
                    _matchResult = FinishMatch(result.IsDraw, result.WinningTeam);
                } else {
                    StartReady();
                }
            }
        }
    }
}