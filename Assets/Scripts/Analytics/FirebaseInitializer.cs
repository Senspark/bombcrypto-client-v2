using System.Threading.Tasks;

// #if UNITY_ANDROID || UNITY_IOS
// using Firebase;
// using Firebase.Messaging;
// #endif

using UnityEngine;

namespace Analytics {
    public static class FirebaseInitializer {
        private static bool _initialized;

#if UNITY_ANDROID || UNITY_IOS
        public static async Task Init() {
            // if (_initialized) {
            //     return;
            // }
            // _initialized = true;
            // var t = await FirebaseApp.CheckAndFixDependenciesAsync();
            // if (t == DependencyStatus.Available) {
            //     FirebaseMessaging.TokenReceived += FirebaseMessagingOnTokenReceived;
            //     FirebaseMessaging.MessageReceived += FirebaseMessagingOnMessageReceived;
            // } else {
            //     Debug.LogError("Init firebase failed");
            // }
        }

        // private static void FirebaseMessagingOnMessageReceived(object sender, MessageReceivedEventArgs e) {
        //     // Debug.Log("Received Registration Token: " + e.Message.From);
        // }
        //
        // private static void FirebaseMessagingOnTokenReceived(object sender, TokenReceivedEventArgs e) {
        //     Debug.Log("FirebaseCloudMessage: Received a new message from: " + e.Token);
        // }
#else
        public static Task Init() {
            return Task.CompletedTask;
        }
#endif
    }
}