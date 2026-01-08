using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace BLPvpMode.Manager.Api {
    public static class Extensions {
        [NotNull]
        public static R Let<T, R>([NotNull] this T item, [NotNull] Func<T, R> block) {
            return block(item);
        }

        [NotNull]
        public static T Apply<T>([NotNull] this T item, [NotNull] Action<T> block) where T : class {
            block(item);
            return item;
        }

        public static bool IsEmpty<T>([NotNull] this T[] source) {
            return source.Length == 0;
        }

        public static bool IsEmpty<T>([NotNull] this List<T> source) {
            return source.Count == 0;
        }

        public static void ForEach<T>([NotNull] this T[] source, [NotNull] Action<T> action) {
            foreach (var element in source) {
                action(element);
            }
        }

        public static void ForEach<T>([NotNull] this HashSet<T> source, [NotNull] Action<T> action) {
            foreach (var element in source) {
                action(element);
            }
        }

        public static void ForEach<T>([NotNull] this T[] source, [NotNull] Action<T, int> action) {
            for (var i = 0; i < source.Length; ++i) {
                action(source[i], i);
            }
        }

        public static void ForEach<T, U>([NotNull] this (T, U)[] source, [NotNull] Action<T, U> action) {
            foreach (var (t, u) in source) {
                action(t, u);
            }
        }

        public static void ForEach<K, V>([NotNull] this Dictionary<K, V> source, [NotNull] Action<K, V> action) {
            foreach (var (k, v) in source) {
                action(k, v);
            }
        }

        [NotNull]
        public static Dictionary<K, V> Associate<T, K, V>([NotNull] this T[] source,
            [NotNull] Func<T, (K, V)> transform) {
            var result = new Dictionary<K, V>(source.Length);
            source.ForEach(t => {
                var (k, v) = transform(t);
                result.Add(k, v);
            });
            return result;
        }

        [NotNull]
        public static Dictionary<K, V> AssociateIndex<T, K, V>(
            [NotNull] this T[] source,
            [NotNull] Func<T, int, (K, V)> transform
        ) {
            var result = new Dictionary<K, V>(source.Length);
            source.ForEach((t, index) => {
                var (k, v) = transform(t, index);
                result.Add(k, v);
            });
            return result;
        }

        [NotNull]
        public static Dictionary<K, V> Associate<T, U, K, V>(
            [NotNull] this (T, U)[] source,
            [NotNull] Func<T, U, (K, V)> transform
        ) {
            var result = new Dictionary<K, V>(source.Length);
            source.ForEach((t, u) => {
                var (k, v) = transform(t, u);
                result.Add(k, v);
            });
            return result;
        }

        [NotNull]
        public static Dictionary<K, V> AssociateWith<K, V>([NotNull] this K[] source, [NotNull] Func<K, V> transform) {
            var result = new Dictionary<K, V>(source.Length);
            source.ForEach(element => { //
                result.Add(element, transform(element));
            });
            return result;
        }

        public static Dictionary<K, R> MapValues<K, V, R>(
            [NotNull] this IEnumerable<KeyValuePair<K, V>> source,
            [NotNull] Func<K, V, R> transform
        ) {
            return source.ToDictionary(
                it => it.Key,
                it => transform(it.Key, it.Value)
            );
        }

        [NotNull]
        public static List<R> MapNotNull<K, V, R>(
            [NotNull] this Dictionary<K, V> source,
            [NotNull] Func<K, V, R> transform
        ) {
            var result = new List<R>();
            foreach (var (key, value) in source) {
                var element = transform(key, value);
                if (element != null) {
                    result.Add(element);
                }
            }
            return result;
        }

        [CanBeNull]
        public static R FirstNotNullOfOrNull<T, R>([NotNull] this T[] source, Func<T, R> transform) {
            foreach (var element in source) {
                var result = transform(element);
                if (result != null) {
                    return result;
                }
            }
            return default;
        }
    }
}