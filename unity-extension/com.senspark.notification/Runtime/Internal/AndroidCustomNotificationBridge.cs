using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using Newtonsoft.Json;

using Senspark.Platforms;

namespace Senspark.Notifications {
#if UNITY_ANDROID
    public class AndroidCustomNotificationBridge : ICustomNotificationBridge {
        
        private const string KTag = "Notification";
        private const string KPrefix = KTag + "Bridge";
        private const string KScheduleAt = KPrefix + "ScheduleAt";
        private const string KScheduleIn = KPrefix + "ScheduleIn";
        private const string KGetNotificationData = KPrefix + "GetNotificationData";
        private const string KCancelNotification = KPrefix + "CancelNotification";
        private const string KCanShow = KPrefix + "CanShow";
        private const string KAskPermission = KPrefix + "CanShow";
        
        private readonly IMessageBridge _messageBridge;

        public AndroidCustomNotificationBridge(bool enableLog) {
            Platform.AddNativePlugin(KTag);
            _messageBridge = Platform.MessageBridge;
        }

        public void Schedule(ScheduleDataTime data) {
            try {
                var j = JsonConvert.SerializeObject(data);
                _messageBridge.CallAsync(KScheduleIn, j);
            } catch (Exception e) {
                FastLog.Error(e.Message);
            }
        }

        public void Schedule(ScheduleDataDay data) {
            try {
                var j = JsonConvert.SerializeObject(data);
                _messageBridge.CallAsync(KScheduleAt, j);
            } catch (Exception e) {
                FastLog.Error(e.Message);
            }
        }

        public async UniTask<NotificationCallbackData> GetNotificationData() {
            try {
                var r = await _messageBridge.CallAsync(KGetNotificationData);
                return JsonConvert.DeserializeObject<NotificationCallbackData>(r);
            }
            catch (Exception e) {
                FastLog.Error(e.Message);
                return null;
            }
        }

        public void CancelNotification(List<int> ids) {
            try {
                var j = JsonConvert.SerializeObject(ids);
                _messageBridge.CallAsync(KCancelNotification, j);
            } catch (Exception e) {
                FastLog.Error(e.Message);
            }
        }

        public bool CanShowNotification() {
            try {
                var s = _messageBridge.Call(KCanShow);
                return bool.Parse(s);
            } catch (Exception e) {
                FastLog.Error(e.Message);
                return false;
            }
        }

        public async UniTask<bool> AskPermission() {
            try {
                var s = await _messageBridge.CallAsync(KAskPermission);
                return bool.Parse(s);
            } catch (Exception e) {
                FastLog.Error(e.Message);
                return false;
            }
        }
    }
#endif
}