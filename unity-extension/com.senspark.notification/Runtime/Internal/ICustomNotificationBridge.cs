using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using Newtonsoft.Json;

namespace Senspark.Notifications {
    public interface ICustomNotificationBridge {
        void Schedule(ScheduleDataTime data);
        void Schedule(ScheduleDataDay data);
        UniTask<NotificationCallbackData> GetNotificationData();
        void CancelNotification(List<int> ids);
        bool CanShowNotification();
        UniTask<bool> AskPermission();
    }

    [Serializable]
    public class NotificationCallbackData {
        [JsonProperty("id")]
        public readonly int ID;

        [JsonProperty("data")]
        public readonly string ExtraData;
    }
}