#if !UNITY_WEBGL
using System;
using System.Threading.Tasks;

using Notification;

using UnityEngine;

namespace Messaging {
    public class FirebaseMessagingManager : IMessagingManager {
        private readonly INotificationManager _notificationManager;
        private Action<string> _onTokenReceivedCallback;

        public FirebaseMessagingManager(INotificationManager notificationManager) {
            _notificationManager = notificationManager;
        }

        public async Task<bool> Initialize() {
            var result = await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();
            if (result != Firebase.DependencyStatus.Available) {
                return false;
            }
            Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
            Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;

            // On iOS, this will display the prompt to request permission to receive
            // notifications if the prompt has not already been displayed before. (If
            // the user already responded to the prompt, their decision is cached by
            // the OS and can be changed in the OS settings).
            // On Android, this will return successfully immediately, as there is no
            // equivalent system logic to run.
            await Firebase.Messaging.FirebaseMessaging.RequestPermissionAsync();

            //var token = await Firebase.Messaging.FirebaseMessaging.GetTokenAsync();

            return true;
        }

        public void SetOnTokenReceivedCallback(Action<string> callback) {
            _onTokenReceivedCallback = callback;
        }

        private void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token) {
            //UnityEngine.Debug.Log("Received Registration Token: " + token.Token);
            //_onTokenReceivedCallback?.Invoke(token.Token);
        }

        private void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e) {
            //UnityEngine.Debug.Log("Received a new message from: " + e.Message.From);
        }

        private static void DebugLog(string s) {
            Debug.Log(s);
        }
    }
}
#endif