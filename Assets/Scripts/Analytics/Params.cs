using System.Collections.Generic;
using System.Globalization;

using Senspark;

namespace Analytics {
    public class Parameter {
        public readonly string name;
        public readonly object value;

        public Parameter(string parameterName, bool parameterValue) {
            name = parameterName;
            value = parameterValue;
        }

        public Parameter(string parameterName, long parameterValue) {
            name = parameterName;
            value = parameterValue;
        }

        public Parameter(string parameterName, double parameterValue) {
            name = parameterName;
            value = parameterValue;
        }

        public Parameter(string parameterName, string parameterValue) {
            name = parameterName;
            value = parameterValue;
        }

        /// <summary>
        /// Format dấu phẩy động cho double, float
        /// </summary>
        /// <returns></returns>
        public string ToTrackingString() {
            return value switch {
                double d => d.ToString(CultureInfo.InvariantCulture),
                float f => f.ToString(CultureInfo.InvariantCulture),
                _ => value.ToString()
            };
        }

        public override string ToString() {
            return ToTrackingString();
        }
    }

    public class AnalyticsEvent : IAnalyticsEvent {
        public string Name { get; }
        public Dictionary<string, object> Parameters { get; }

        public AnalyticsEvent(string eventName, Parameter[] parameters) {
            Name = eventName;
            Parameters = new Dictionary<string, object>();

            foreach (var iter in parameters) {
                Parameters.Add(iter.name, iter.value);
            }
        }
    } 
}