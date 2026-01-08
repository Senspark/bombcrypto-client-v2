using System.Collections.Generic;

using Data;

namespace Utils {
    public static class StatUtils {
        public static Dictionary<int, int> Sum(this IEnumerable<StatData> stats) {
            Dictionary<int, int> result = new();
            foreach (var it in stats) {
                result.TryGetValue(it.StatId, out var value);
                result[it.StatId] = value + it.Value;
            }
            return result;
        }
    }
}