using UnityEngine;
using UnityEngine.Assertions;

namespace BLPvpMode.Engine.Utility {
    public interface IBitEncoder<out T, out R> {
        T Value { get; }
        R Push(bool value);
        R Push(int value, int size);
        R Push(long value, int size);
        R Push(float value, int precision, int size);
    }

    public struct IntBitEncoder : IBitEncoder<int, IntBitEncoder> {
        private int _position;

        public int Value { get; private set; }

        public IntBitEncoder(int position = 0) {
            _position = position;
            Value = 0;
        }

        public IntBitEncoder Push(bool value) {
            Assert.IsTrue(_position + 1 <= 32, "Bit encoder out of range");
            Value |= (value ? 1 : 0) << _position;
            ++_position;
            return this;
        }

        public IntBitEncoder Push(int value, int size) {
            Assert.IsTrue(_position + size <= 32, "Bit encoder out of range");
            Value |= value << _position;
            _position += size;
            return this;
        }

        public IntBitEncoder Push(long value, int size) {
            Assert.IsTrue(_position + size <= 32, "Bit encoder out of range");
            Assert.IsTrue(false, "Cannot encode long value");
            return this;
        }

        public IntBitEncoder Push(float value, int precision, int size) {
            var multiplier = Mathf.Pow(10f, precision);
            return Push((int) (value * multiplier), size);
        }
    }

    public struct LongBitEncoder : IBitEncoder<long, LongBitEncoder> {
        private int _position;

        public long Value { get; private set; }

        public LongBitEncoder(int position = 0) {
            _position = position;
            Value = 0;
        }

        public LongBitEncoder Push(bool value) {
            Assert.IsTrue(_position + 1 <= 64, "Bit encoder out of range");
            Value |= (value ? 1L : 0L) << _position;
            ++_position;
            return this;
        }

        public LongBitEncoder Push(int value, int size) {
            Assert.IsTrue(_position + size <= 64, "Bit encoder out of range");
            Value |= (long) value << _position;
            _position += size;
            return this;
        }

        public LongBitEncoder Push(long value, int size) {
            Assert.IsTrue(_position + size <= 64, "Bit encoder out of range");
            Value |= value << _position;
            _position += size;
            return this;
        }

        public LongBitEncoder Push(float value, int precision, int size) {
            var multiplier = Mathf.Pow(10f, precision);
            return Push((int) (value * multiplier), size);
        }
    }
}