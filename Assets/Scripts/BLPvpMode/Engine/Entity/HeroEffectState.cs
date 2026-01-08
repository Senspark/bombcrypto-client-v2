using BLPvpMode.Engine.Utility;

namespace BLPvpMode.Engine.Entity {
    public class HeroEffectState : IHeroEffectState {
        public static IHeroEffectState Decode(long state) {
            var decoder = new LongBitDecoder(state);
            return new HeroEffectState(
                isActive: decoder.PopBoolean(),
                reason: (HeroEffectReason) decoder.PopInt(3),
                timestamp: decoder.PopInt(20),
                duration: decoder.PopInt(16)
            );
        }

        public bool IsActive { get; }
        public HeroEffectReason Reason { get; }
        public int Timestamp { get; }
        public int Duration { get; }

        public HeroEffectState(
            bool isActive,
            HeroEffectReason reason,
            int timestamp,
            int duration
        ) {
            IsActive = isActive;
            Reason = reason;
            Timestamp = timestamp;
            Duration = duration;
        }

        public long Encode() {
            var encoder = new LongBitEncoder()
                .Push(IsActive)
                .Push((int) Reason, 3)
                .Push(Timestamp, 20)
                .Push(Duration, 16);
            return encoder.Value;
        }
    }
}