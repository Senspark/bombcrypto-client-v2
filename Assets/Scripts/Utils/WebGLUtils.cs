using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using UnityEngine;

namespace App {
    public static class WebGLUtils {
#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void WebGLUtils_OpenUrl(string url, bool newTab);

        [DllImport("__Internal")]
        private static extern bool WebGLUtils_IsCetusIntercepted();

        [DllImport("__Internal")]
        private static extern bool WebGLUtils_InitBeforeUnloadEvent();

        [DllImport("__Internal")]
        private static extern double WebGLUtils_GetTime();

        [DllImport("__Internal")]
        private static extern string WebGLUtils_GetDomain();
#endif

        public static void OpenUrl(string url, bool newTab = false) {
#if !UNITY_EDITOR && UNITY_WEBGL
            WebGLUtils_OpenUrl(url, newTab);
#else
            Application.OpenURL(url);
#endif
        }

        public static bool IsCetusIntercepted() {
#if !UNITY_EDITOR && UNITY_WEBGL
            return WebGLUtils_IsCetusIntercepted();
#endif
            return false;
        }

        public static void InitBeforeUnloadEvent() {
#if !UNITY_EDITOR && UNITY_WEBGL
             WebGLUtils_InitBeforeUnloadEvent();
#endif
        }

        public static long GetUnixTime() {
#if !UNITY_EDITOR && UNITY_WEBGL
            return (long) WebGLUtils_GetTime();
#endif
            return DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public static bool IsInvalidDomain() {
#if !UNITY_EDITOR && UNITY_WEBGL
            var domain = WebGLUtils_GetDomain();
            if (string.IsNullOrWhiteSpace(domain)) {
                return true;
            }
            return !CheckValidUrl(domain);
#endif
            return false;
        }

        public static async Task<int> GetPingMs(string host) {
#if UNITY_WEBGL
            var res = await JavascriptProcessor.Instance.CallMethod("Ping", host);
            var result = int.Parse(res);
            return result;
#endif
            return -1;
        }

        public static Dictionary<string, string> GetUrlParams() {
#if UNITY_WEBGL
            try {
                var url = Application.absoluteURL;
                var queryMark = url.IndexOf("?", StringComparison.Ordinal);
                if (queryMark >= 0) {
                    var parameters = url.Substring(queryMark + 1);
                    var data = parameters.Split('&', '=');
                    var result = new Dictionary<string, string>();
                    for (var i = 0; i < data.Length; i += 2) {
                        //Fix IndexOutOfRangeException exception trên telegram
                        if(i >= data.Length || i + 1 >= data.Length) break;
                        result[data[i]] = data[i + 1];
                    }
                    return result;
                }
            } catch (Exception e) {
                return new Dictionary<string, string>();
            }
            
#endif
            return new Dictionary<string, string>();
        }
    }
}