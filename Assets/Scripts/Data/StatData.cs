using Constant;

namespace Data {
    public class StatData {
        public int StatId { get; }
        public int Level { get; private set; }

        /// <summary>
        /// Chỉ số tối có thể cộng lên được
        /// </summary>
        public int Max { get; private set; }

        /// <summary>
        /// Chỉ số hiện tại
        /// </summary>
        public int Value { get; private set; }

        /// <summary>
        /// Chỉ số tối đa có thể nâng cấp (áp dụng cho Hero)
        /// </summary>
        public int MaxUpgradeValue { get; private set; }

        public StatData(
            StatId statId,
            int level,
            int max,
            int value
        ) : this((int)statId, level, max, value) {
        }

        public StatData(
            StatId statId,
            int level,
            int max,
            int value,
            int maxUpgradeValue
        ) : this((int)statId, level, max, value, maxUpgradeValue) {
        }

        public StatData(int statId, int level, int max, int value) {
            StatId = statId;
            Level = level;
            Max = max;
            Value = value;
        }

        public StatData(int statId, int level, int max, int value, int maxUpgradeValue) {
            StatId = statId;
            Level = level;
            Max = max;
            Value = value;
            MaxUpgradeValue = maxUpgradeValue;
        }

        public void PlusAssign(StatData stat) {
            Level += stat.Level;
            Max += stat.Max;
            Value += stat.Value;
        }
    }
}