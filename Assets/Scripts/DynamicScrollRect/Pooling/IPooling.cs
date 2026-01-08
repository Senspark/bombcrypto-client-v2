namespace pooling {
    public interface IPooling {
        string ObjectName { get; }
        bool IsUsing { get; set; }
        void OnCollect();
        void OnRelease();
    }
}