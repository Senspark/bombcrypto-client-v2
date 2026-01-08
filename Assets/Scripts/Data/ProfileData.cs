using System;

namespace Data {
    public class ProfileData {
        public string Hash;
        public DateTime LoginTime;
    }
    
    public enum Platform {
        Unknown,
        WebPC,
        WebTelegram,
        WebOther,
        MobileTelegram,
        AndroidNative,
        IOSNative,
        Editor,
        IosTelegram,
        AndroidTelegram,
    }
}