using BLPvpMode.Manager;

namespace BLPvpMode.Engine.Manager {
    public class StepTimeManager : ITimeManager {
        public long Timestamp { get; private set; }

        public void Step(int delta) {
            Timestamp += delta;
        }
    }
}