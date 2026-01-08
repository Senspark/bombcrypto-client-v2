using System;

namespace Data {
    public class DailyRewardItemData {
        public int Id;
        public int Quantity;
        public string Type;
    }

    public class DailyRewardData {
        public enum DailyRewardStatus {
            None,
            Claimed,
            Locked,
            Countdown
        }

        public DateTime ClaimTime;
        public DailyRewardItemData[] Items;
        public bool Randomize;
        public DailyRewardStatus Status;
        public string RandomIcon;
    }
}