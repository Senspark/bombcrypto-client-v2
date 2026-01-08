using System;
using System.Linq;

using BLPvpMode.Engine.Info;

namespace BLPvpMode.Manager.Builders {
    public class MatchInfoBuilder : IMatchInfoBuilder {
        public Engine.Info.PvpMode Mode { get; set; }
        public int Round { get; set; }
        public int Slot { get; set; }
        public IMatchUserInfoBuilder[] UserBuilders { get; set; }

        public IMatchInfo Build() {
            return new MatchInfo {
                Mode = Mode,
                Rule = new MatchRuleInfo {
                    Round = Round, //
                },
                Team = Mode switch {
                    Engine.Info.PvpMode.FFA_2 => new[] {
                        new MatchTeamInfo {
                            Slots = new[] { 0 }, //
                        },
                        new MatchTeamInfo {
                            Slots = new[] { 1 }, //
                        },
                    },
                    _ => throw new ArgumentOutOfRangeException()
                },
                Slot = Slot,
                Info = UserBuilders
                    .Select(item => item.Build())
                    .ToArray(),
            };
        }
    }
}