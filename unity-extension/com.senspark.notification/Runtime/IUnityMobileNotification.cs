using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using JetBrains.Annotations;

namespace Senspark.Notifications {
    [Service(nameof(IUnityMobileNotification))]
    public interface IUnityMobileNotification : IObserverManager<NotificationObserver>, IDisposable {
        UniTask Initialize();

        /// <summary>
        /// Show Notification sau ___ giây kể từ thời điểm này 
        /// </summary>
        /// <param name="id">Số Id bất kỳ để có thể cancel/replace notification này</param>
        /// <param name="useBanner">Sử dụng Banner hình nền</param>
        /// <param name="title">Tiêu đề thông báo</param>
        /// <param name="message">Nội dung thông báo</param>
        /// <param name="extraData">Data đính kèm</param>
        /// <param name="delaySeconds">Sẽ hiển thị sau bao nhiêu giây kể từ thời điểm này?</param>
        /// <param name="repeatSeconds">Sẽ lặp lại sau bao nhiêu giây kể từ lúc hiển thị?</param>
        [Obsolete("Sử dụng method Schedule(ScheduleDataTime data) thay thế")]
        void RepeatFromNow(
            int id,
            bool useBanner,
            [CanBeNull] string title,
            string message,
            [CanBeNull] string extraData,
            int delaySeconds,
            int repeatSeconds
        );

        /// <summary>
        /// Giống method RepeatFromNow nhưng params gọn hơn
        /// </summary>
        void Schedule(ScheduleDataTime data);

        /// <summary>
        /// Show Notification tại chính xác thời gian trong ngày & lặp lại sau mỗi __ ngày
        /// </summary>
        /// <param name="id">Số Id bất kỳ để có thể cancel/replace notification này</param>
        /// <param name="useBanner">Sử dụng Banner hình nền</param>
        /// <param name="title">Tiêu đề thông báo</param>
        /// <param name="message">Nội dung thông báo</param>
        /// <param name="extraData">Data đính kèm</param>
        /// <param name="atHour">Vào lúc mấy giờ?</param>
        /// <param name="atMinute">Vào lúc mấy phút?</param>
        /// <param name="repeatAfterDays">Lặp lại sau mỗi bao nhiêu ngày?</param>
        [Obsolete("Sử dụng method Schedule(ScheduleDataDay data) thay thế")]
        void RepeatAtExactlyTime(
            int id,
            bool useBanner,
            [CanBeNull] string title,
            string message,
            [CanBeNull] string extraData,
            int atHour,
            int atMinute,
            int repeatAfterDays
        );

        /// <summary>
        /// Giống method RepeatAtExactlyTime nhưng params gọn hơn
        /// </summary>
        void Schedule(ScheduleDataDay data);

        void CancelNotification(List<int> ids);
        bool CanShowNotification();
        UniTask<bool> AskPermission();
        UniTask<NotificationCallbackData> GetNotificationData();
    }

    public class NotificationObserver {
        public Action<int, string> OnNotificationShown;
        public Action<int, string> OnNotificationClicked;
    }
}