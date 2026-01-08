using System;

using Cysharp.Threading.Tasks;

using UnityEngine;
using UnityEngine.Networking;

namespace Utils {
    public class CheckInternetConnectionHelper {
        public async UniTask<bool> CheckConnection() {
            try {
                if (Application.platform == RuntimePlatform.WebGLPlayer) {
                    return true;
                }
                if (Application.internetReachability != NetworkReachability.NotReachable) {
                    const string url = "https://www.google.com/";
                    using var request = UnityWebRequest.Get(url);
                    request.timeout = 10;
                    await request.SendWebRequest();
                    var result = request.result;
                    if (result == UnityWebRequest.Result.Success) {
                        return true;
                    }
                }
                return false;
            } catch (Exception e) {
                return false;
            }
        }
    }
}