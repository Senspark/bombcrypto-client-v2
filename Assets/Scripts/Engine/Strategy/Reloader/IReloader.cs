namespace Engine.Strategy.Reloader
{
    public interface IReloader
    {
        float Progress { get; }
        bool IsReady { get; }
        bool IsActive { get; }

        /// <summary>
        /// Attempts to reload.
        /// </summary>
        bool Reload();

        void ChangeCooldown(float cooldown);
        
        void Stop();

        void Start();
        
        void Update(float delta);
    }
}