using System;

using JetBrains.Annotations;

using Newtonsoft.Json;

namespace Senspark.Notifications {
    /// <summary>
    /// Hẹn giờ theo giây
    /// </summary>
    [Serializable]
    public class ScheduleDataTime {
        #region Properties
        // @formatter:off
        [JsonProperty("notificationId")] public int Id { get; private set; }
        [JsonProperty("body")] public string Message { get; private set; }
        [JsonProperty("title")] [CanBeNull] public string Title { get; set; }
        [JsonProperty("extraData")] [CanBeNull] public string ExtraData { get; set; }
        [JsonProperty("titleColor")] [CanBeNull] public string TitleColor { get; private set; }
        [JsonProperty("bodyColor")] [CanBeNull] public string MessageColor { get; private set; }
        [JsonProperty("backgroundIndex")] public int BackgroundIndex { get; private set; } = -1;
        [JsonProperty("delaySeconds")] public int DelaySeconds { get; private set; }
        [JsonProperty("repeatSeconds")] public int RepeatSeconds { get; private set; }
        [JsonIgnore] public bool UseBackground => BackgroundIndex >= 0;
        // @formatter:on
        #endregion

        public ScheduleDataTime(int id, string title, string message) {
            Id = id;
            Title = title;
            Message = message;
        }

        public ScheduleDataTime(int id, string message) : this(id, null, message) { }

        /// <summary>
        /// Bắt đầu xuất hiện sau ___ giây
        /// </summary>
        public ScheduleDataTime AddDelay(int delaySeconds) {
            DelaySeconds = delaySeconds;
            return this;
        }

        /// <summary>
        /// Lặp lại sau ___ giây
        /// </summary>
        public ScheduleDataTime AddRepeat(int repeatAfterSeconds) {
            RepeatSeconds = repeatAfterSeconds;
            return this;
        }

        public ScheduleDataTime AddExtraData(string extraData) {
            ExtraData = extraData;
            return this;
        }

        /// <summary>
        /// Sử dụng background với Index = [0, 1, 2, 3]
        /// </summary>
        public ScheduleDataTime AddBackground(int backgroundIndex) {
            BackgroundIndex = backgroundIndex;
            return this;
        }

        /// <summary>
        /// Set màu cho title và message
        /// </summary>
        /// <param name="titleColor">Hex color, vd: #FFFFFF</param>
        /// <param name="messageColor">Hex color, vd: #FFFFFF</param>
        public ScheduleDataTime AddColor([CanBeNull] string titleColor, [CanBeNull] string messageColor) {
            TitleColor = titleColor;
            MessageColor = messageColor;
            return this;
        }
    }

    /// <summary>
    /// Hẹn giờ theo ngày
    /// </summary>
    [Serializable]
    public class ScheduleDataDay {
        #region Properties
        // @formatter:off
        [JsonProperty("notificationId")] public int Id { get; private set; }
        [JsonProperty("body")] public string Message { get; private set; }
        [JsonProperty("title")] [CanBeNull] public string Title { get; set; }
        [JsonProperty("extraData")] [CanBeNull] public string ExtraData { get; set; }
        [JsonProperty("titleColor")] [CanBeNull] public string TitleColor { get; private set; }
        [JsonProperty("bodyColor")] [CanBeNull] public string MessageColor { get; private set; }
        [JsonProperty("backgroundIndex")] public int BackgroundIndex { get; private set; } = -1;
        [JsonProperty("atHour")] public int AtHour { get; private set; }
        [JsonProperty("atMinute")] public int AtMinute { get; private set; }
        [JsonProperty("repeatDays")] public int RepeatAfterDays { get; private set; }
        [JsonIgnore] public bool UseBackground => BackgroundIndex >= 0;
        // @formatter:on
        #endregion

        public ScheduleDataDay(int id, string title, string message) {
            Id = id;
            Title = title;
            Message = message;
        }

        public ScheduleDataDay(int id, string message) : this(id, null, message) { }

        /// <summary>
        /// Bắt đầu xuất hiện tại ___ giờ ___ phút
        /// </summary>
        public ScheduleDataDay AddTime(int atHour, int atMinutes) {
            AtHour = atHour;
            AtMinute = atMinutes;
            RepeatAfterDays = 0;
            return this;
        }

        /// <summary>
        /// Bắt đầu xuất hiện sau ___ ngày
        /// </summary>
        public ScheduleDataDay AddRepeat(int repeatAfterDays) {
            RepeatAfterDays = repeatAfterDays;
            return this;
        }

        public ScheduleDataDay AddExtraData(string extraData) {
            ExtraData = extraData;
            return this;
        }

        /// <summary>
        /// Sử dụng background với Index = [0, 1, 2, 3]
        /// </summary>
        public ScheduleDataDay AddBackground(int backgroundIndex) {
            BackgroundIndex = backgroundIndex;
            return this;
        }
        
        /// <summary>
        /// Set màu cho title và message
        /// </summary>
        /// <param name="titleColor">Hex color, vd: #FFFFFF</param>
        /// <param name="messageColor">Hex color, vd: #FFFFFF</param>
        public ScheduleDataDay AddColor([CanBeNull] string titleColor, [CanBeNull] string messageColor) {
            TitleColor = titleColor;
            MessageColor = messageColor;
            return this;
        }
    }
}