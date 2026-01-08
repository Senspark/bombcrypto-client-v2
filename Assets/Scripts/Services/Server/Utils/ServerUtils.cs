using System;

using Newtonsoft.Json;

using Sfs2X.Entities.Data;

namespace App {
    public static partial class ServerUtils {
        public static Exception ParseErrorMessage(ISFSObject data, string defaultMessage = "Failed") {
            var ec = GetErrorCode(data);
            var msg = GetErrorMessage(data);
            if (string.IsNullOrWhiteSpace(msg)) {
                msg = SFSDefine.ErrorMessages.ContainsKey(ec) ? SFSDefine.ErrorMessages[ec] : defaultMessage;
            }
            return SFSDefine.SfsExceptions.GetException(ec, msg);
        }
        
        public static bool HasError(ISFSObject data) {
            return GetErrorCode(data) != 0;
        }
        
        public static int GetErrorCode(ISFSObject data) {
            var ec1 = data.GetInt("code");
            var ec2 = data.GetInt("ec");
            var ec = ec1 != 0 ? ec1 : ec2;
            return ec;
        }
        
        public static string GetErrorMessage(ISFSObject data) {
            if (!HasError(data)) {
                return string.Empty;
            }
            var m1 = data.GetUtfString("message");
            if (!string.IsNullOrWhiteSpace(m1)) {
                return m1;
            }
            
            var m2 = data.GetUtfString("es");
            if (!string.IsNullOrWhiteSpace(m2)) {
                return m2;
            }
            
            var ec = GetErrorCode(data);
            return SFSDefine.ErrorMessages.TryGetValue(ec, out var message) ? message : $"Error code: {ec}";
        }
        
        public static bool IsUserBaned(int errorCode) {
            return errorCode == ErrorCode.USER_BANNED;
        }
        
        public static ISFSObject ConvertFromJsonDataServer(ISFSObject response) {
            try
            {
                var data = response.GetUtfString("data");
                return SFSObject.NewFromJsonData(data);
            }
            catch (Exception) {
                return null;
            }
        }
        public static ISFSObject ConvertFromJsonDataServer(string data) {
            try
            {
                return SFSObject.NewFromJsonData(data);
            }
            catch (Exception) {
                return null;
            }
        }
    }
}