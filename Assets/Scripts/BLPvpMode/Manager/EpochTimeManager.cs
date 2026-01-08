using System;

using Engine.Utils;

namespace BLPvpMode.Manager {
    public class EpochTimeManager : ITimeManager {
        public long Timestamp => Epoch.GetUnixTimestamp(TimeSpan.TicksPerMillisecond);
    }
}