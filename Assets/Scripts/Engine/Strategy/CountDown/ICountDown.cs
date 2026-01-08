namespace Engine.Strategy.CountDown
{
    public interface ICountDown
    {
        float Progress { get; }
        bool IsTimeEnd { get; }
        float TimeRemain { get; }
        
        void SetEnable(bool value);
        void ResetTime(float value);
        void ExpanseTime(float value);
        void Update(float delta);
        void Reset();
    }
}