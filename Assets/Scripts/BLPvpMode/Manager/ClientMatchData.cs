using System.Collections.Generic;

using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.Info;

using JetBrains.Annotations;

namespace BLPvpMode.Manager {
    public class ClientMatchData : IMatchData {
        [NotNull]
        private readonly IMatchData _data;

        [NotNull]
        private readonly INetworkStats _stats;

        private readonly int _slot;

        public string Id => _data.Id;

        public MatchStatus Status {
            get => _data.Status;
            set => throw new System.NotImplementedException();
        }

        public int ObserverCount => _data.ObserverCount;
        public long StartTimestamp => _data.StartTimestamp;

        public long ReadyStartTimestamp {
            get => _data.ReadyStartTimestamp - _stats.GetTimeDelta(_slot);
            set => throw new System.NotImplementedException();
        }

        public long RoundStartTimestamp {
            get => _data.RoundStartTimestamp - _stats.GetTimeDelta(_slot);
            set => throw new System.NotImplementedException();
        }

        public int Round {
            get => _data.Round;
            set => throw new System.NotImplementedException();
        }

        public List<IMatchResultInfo> Results => _data.Results;

        public ClientMatchData(
            [NotNull] IMatchData data,
            [NotNull] INetworkStats stats,
            int slot
        ) {
            _data = data;
            _stats = stats;
            _slot = slot;
        }
    }
}