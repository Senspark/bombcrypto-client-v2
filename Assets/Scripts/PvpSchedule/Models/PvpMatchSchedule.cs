using Newtonsoft.Json;

using Services;

namespace PvpSchedule.Models {
    public class PvpPlayerMatch : IPvpPlayerMatch {
        public int Score { get; }

        public int UserId { get; }

        public string UserName { get; }

        public string DisplayName { get; }
        public PvpRankType Rank { get; }

        [JsonConstructor]
        public PvpPlayerMatch(
            [JsonProperty("score")] string score,
            [JsonProperty("user_id")] int userId,
            [JsonProperty("username")] string userName,
            [JsonProperty("display_name")] string displayName,
            [JsonProperty("rank")] int rank
        ) {
            Score = int.TryParse(score, out var value) ? value : 0;
            UserId = userId;
            UserName = userName;
            DisplayName = displayName;
            Rank = rank switch {
                0 => PvpRankType.Iron1,
                1 => PvpRankType.Iron1,
                2 => PvpRankType.Iron2,
                3 => PvpRankType.Copper1,
                4 => PvpRankType.Copper2,
                5 => PvpRankType.Silver1,
                6 => PvpRankType.Silver2,
                7 => PvpRankType.Gold1,
                8 => PvpRankType.Gold2,
                9 => PvpRankType.Platinum1,
                10 => PvpRankType.Platinum2,
                11 => PvpRankType.Emerald1,
                12 => PvpRankType.Emerald2,
                13 => PvpRankType.Diamond1,
                14 => PvpRankType.Diamond2,
                _ => PvpRankType.Iron1
            };
        }
    }

    public class PvpMatchSchedule : IPvpMatchSchedule {
        public string MatchId { get; }
        public MatchStatus Status { get; set; }
        public BLPvpMode.Engine.Info.PvpMode Mode { get; }
        public long FindBeginTimestamp { get; }
        public long FindEndTimestamp { get; }
        public long StartTimestamp { get; }
        public long FinishTimestamp { get; }
        public IPvpPlayerMatch[] Players { get; }
        public int ObserverCount { get; }

        [JsonConstructor]
        public PvpMatchSchedule(
            [JsonProperty("id")] string matchId,
            [JsonProperty("status")] int status,
            [JsonProperty("mode")] int mode,
            [JsonProperty("find_begin_timestamp")] long findBeginTimestamp,
            [JsonProperty("find_end_timestamp")] long findEndTimestamp,
            [JsonProperty("start_timestamp")] long startTimestamp,
            [JsonProperty("finish_timestamp")] string finishTimestamp,
            [JsonProperty("info")] PvpPlayerMatch[] info,
            [JsonProperty("observer_count")] int observerCount
        ) {
            MatchId = matchId;
            Status = status switch {
                0 => MatchStatus.Upcoming,
                1 => MatchStatus.Waiting,
                2 => MatchStatus.InProgress,
                3 or 4 => MatchStatus.Ended,
                _ => MatchStatus.Unknown,
            };
            Mode = (BLPvpMode.Engine.Info.PvpMode) mode;
            FindBeginTimestamp = findBeginTimestamp;
            FindEndTimestamp = findEndTimestamp;
            StartTimestamp = startTimestamp;
            FinishTimestamp = long.TryParse(finishTimestamp, out var value) ? value : 0;;
            Players = new IPvpPlayerMatch[info.Length];
            for (var i = 0; i < info.Length; i++) {
                Players[i] = info[i];
            }
            ObserverCount = observerCount;
        }
    }
}