using System;
using System.Collections.Generic;
using System.Linq;

namespace Senspark.Server {
    using List = List<object>;
    using Dict = Dictionary<string, object>;

    public static class ListExtensions {
        public static List ToDataField(this List<bool> list) {
            return list.ConvertAll(e => (object) e);
        }

        public static List ToDataField(this List<long> list) {
            return list.ConvertAll(e => (object) e);
        }

        public static List ToDataField(this List<double> list) {
            return list.ConvertAll(e => (object) e);
        }

        public static List ToDataField(this List<string> list) {
            return list.ConvertAll(e => (object) e);
        }

        public static List ToDataField(this List<DateTime> list) {
            return list.ConvertAll(e => (object) e);
        }

        public static List<T> ConvertTo<T>(this List list) {
            if (list.Count > 0 && list[0] is T) {
                return list.ConvertAll(e => (T) e);
            }
            return new List<T>();
        }
    }

    public static class DictionaryExtensions {
        public static Dict ToDataField(this Dictionary<string, bool> dict) {
            return dict.ToDictionary<KeyValuePair<string, bool>, string, object>(pair => pair.Key,
                pair => pair.Value);
        }

        public static Dict ToDataField(this Dictionary<string, long> dict) {
            return dict.ToDictionary<KeyValuePair<string, long>, string, object>(pair => pair.Key,
                pair => pair.Value);
        }

        public static Dict ToDataField(this Dictionary<string, double> dict) {
            return dict.ToDictionary<KeyValuePair<string, double>, string, object>(pair => pair.Key,
                pair => pair.Value);
        }

        public static Dict ToDataField(this Dictionary<string, string> dict) {
            return dict.ToDictionary<KeyValuePair<string, string>, string, object>(pair => pair.Key,
                pair => pair.Value);
        }

        public static Dict ToDataField(this Dictionary<string, DateTime> dict) {
            return dict.ToDictionary<KeyValuePair<string, DateTime>, string, object>(pair => pair.Key,
                pair => pair.Value);
        }

        public static Dictionary<string, T> ConvertTo<T>(this Dict dict) {
            if (dict.Count > 0 && dict.First().Value is T) {
                return dict.ToDictionary(pair => pair.Key, pair => (T) pair.Value);
            }
            return new Dictionary<string, T>();
        }
    }
}