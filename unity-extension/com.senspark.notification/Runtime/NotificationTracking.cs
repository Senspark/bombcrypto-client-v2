using System;

namespace Senspark.Notifications {
    [Serializable] //
    internal class NotificationTracking : IAnalyticsEvent {
        public string Name { get; set; } = "track_notification";

        [AnalyticsParameter("type")] public int Type { get; }
        [AnalyticsParameter("time_stamp")] public long TimeStamp { get; }
        [AnalyticsParameter("id")] public int Id { get; }
        [AnalyticsParameter("message")] public string Message { get; }

        public NotificationTracking(int type, int id, long timeStamp, string message) {
            Type = type;
            Id = id;
            TimeStamp = timeStamp;
            Message = message;
        }
    }
}