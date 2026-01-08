namespace Game.Dialog.Connects {
    public class StateReturnValue<T> {
        public readonly bool IsCancel;
        public readonly T Value;

        public StateReturnValue(bool isCancel, T value) {
            IsCancel = isCancel;
            Value = value;
        }
    }
}