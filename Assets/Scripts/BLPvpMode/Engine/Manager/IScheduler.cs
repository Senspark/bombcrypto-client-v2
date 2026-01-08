using System;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Manager {
    public interface IScheduler {
        void ScheduleOnce(
            [NotNull] string key,
            int delay,
            [NotNull] Action action
        );

        void Schedule(
            [NotNull] string key,
            int delay,
            int interval,
            [NotNull] Action action
        );

        void Clear([NotNull] string key);
        void ClearAll();
    }
}