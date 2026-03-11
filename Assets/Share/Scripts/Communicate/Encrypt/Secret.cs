using System.Linq;

using App;

namespace Communicate.Encrypt {
    public static class Secret {
        public static byte[] ReactPermutationOrder32 => AppConfig.EncryptionData?.reactPermutationOrder32 ??
                                                        Enumerable.Range(0, 32).Select(i => (byte)i).ToArray();

        public static int SmartFoxAppendBytes => AppConfig.EncryptionData?.smartFoxAppendBytes ?? 16;
        public static int ApiAppendBytes => AppConfig.EncryptionData?.apiAppendBytes ?? 1;
    }
}