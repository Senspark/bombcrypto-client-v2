namespace BLPvpMode.Engine.Utility {
    public interface IBitDecoder {
        bool PopBoolean();
        int PopInt(int size);
        float PopFloat(int precision, int size);
    }

    public struct IntBitDecoder : IBitDecoder {
        private static readonly float[] Powers = { 1f, 10, 100, 1000, 10000, 100000, 1000000, };
        private int _value;

        public IntBitDecoder(int value) {
            _value = value;
        }

        public bool PopBoolean() {
            var result = (_value & 1) == 1;
            _value >>= 1;
            return result;
        }

        public int PopInt(int size) {
            var result = _value & ((1 << size) - 1);
            _value >>= size;
            return result;
        }

        public float PopFloat(int precision, int size) {
            var multiplier = Powers[precision];
            var result = (_value & ((1 << size) - 1)) / multiplier;
            _value >>= size;
            return result;
        }
    }

    public struct LongBitDecoder : IBitDecoder {
        private static readonly float[] Powers = { 1f, 10, 100, 1000, 10000, 100000, 1000000, };
        private long _value;

        public LongBitDecoder(long value) {
            _value = value;
        }

        public bool PopBoolean() {
            var result = (_value & 1) == 1;
            _value >>= 1;
            return result;
        }

        public int PopInt(int size) {
            var result = _value & ((1L << size) - 1);
            _value >>= size;
            return (int) result;
        }

        public float PopFloat(int precision, int size) {
            var multiplier = Powers[precision];
            var result = (_value & ((1L << size) - 1)) / multiplier;
            _value >>= size;
            return result;
        }
    }
}