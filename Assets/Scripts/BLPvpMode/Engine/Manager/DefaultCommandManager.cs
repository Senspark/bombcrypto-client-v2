using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using BLPvpMode.Engine.Command;
using BLPvpMode.Engine.Data;
using BLPvpMode.Manager.Api;

using JetBrains.Annotations;

using Senspark;

using UnityEngine;

namespace BLPvpMode.Engine.Manager {
    public class DefaultCommandManager : ICommandManager {
        [NotNull]
        private readonly ILogManager _logger;

        [NotNull]
        private readonly IMatch _match;

        [NotNull]
        private readonly IMatchData _matchData;

        [NotNull]
        private readonly IPacketManager _packetManager;

        [NotNull]
        private readonly IStateManager _stateManager;

        [NotNull]
        private readonly IObserveDataFactory _dataFactory;

        [NotNull]
        private readonly List<ICommand> _commands;

        public DefaultCommandManager(
            [NotNull] ILogManager logger,
            [NotNull] IMatch match,
            [NotNull] IMatchData matchData,
            [NotNull] IPacketManager packetManager,
            [NotNull] IStateManager stateManager,
            [NotNull] IObserveDataFactory dataFactory
        ) {
            _logger = logger;
            _match = match;
            _matchData = matchData;
            _packetManager = packetManager;
            _stateManager = stateManager;
            _dataFactory = dataFactory;
            _commands = new List<ICommand>();
        }

        public async Task<IMoveHeroData> MoveHero(int slot, int timestamp, Vector2 position) {
            var tcs = new TaskCompletionSource<IMoveHeroData>();
            _commands.Add(
                new MoveHeroCommand(
                    _logger,
                    _packetManager,
                    timestamp,
                    _match,
                    tcs,
                    slot,
                    position
                )
            );
            return await tcs.Task;
        }

        public async Task<IPlantBombData> PlantBomb(int slot, int timestamp) {
            var tcs = new TaskCompletionSource<IPlantBombData>();
            _commands.Add(
                new PlantBombCommand(
                    _logger,
                    _packetManager,
                    timestamp,
                    _match,
                    tcs,
                    slot
                )
            );
            return await tcs.Task;
        }

        public async Task ThrowBomb(int slot, int timestamp) {
            var tcs = new TaskCompletionSource<object>();
            _commands.Add(
                new ThrowBombCommand(
                    _packetManager,
                    timestamp,
                    _match,
                    tcs,
                    slot
                )
            );
            await tcs.Task;
        }

        public async Task UseBooster(int slot, int timestamp, Booster item) {
            var tcs = new TaskCompletionSource<object>();
            _commands.Add(
                new UseBoosterCommand(
                    _logger,
                    _packetManager,
                    timestamp,
                    _match,
                    tcs,
                    slot,
                    item
                )
            );
            await tcs.Task;
        }

        public List<IMatchObserveData> ProcessCommands() {
            var commandsByTimestamp = _commands
                .OrderBy(it => it.Timestamp)
                .GroupBy(it => it.Timestamp)
                .ToDictionary(
                    it => it.Key,
                    it => it.ToList());
            _commands.Clear();
            var dataList = commandsByTimestamp.MapNotNull((timestamp, commands) => {
                commands.ForEach(it =>
                    it.Handle()
                );
                var stateDelta = _stateManager.ProcessState();
                return stateDelta == null
                    ? null
                    : _dataFactory.Generate(timestamp + _matchData.RoundStartTimestamp, stateDelta);
            }).ToList();
            return dataList;
        }
    }
}