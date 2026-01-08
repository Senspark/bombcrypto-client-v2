using App;

namespace Communicate.Encrypt {
    public static class Secret {
        public static byte[] ReactPermutationOrder32 => AppConfig.ReactPermutationOrder32;
        public static int SmartFoxAppendBytes => AppConfig.SmartFoxAppendBytes;
        public static int ApiAppendBytes => AppConfig.ApiAppendBytes;
    }
}