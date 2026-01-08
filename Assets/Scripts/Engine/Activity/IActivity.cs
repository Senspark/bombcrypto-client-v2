namespace Engine.Activity
{
    /// <summary>
    /// Base class for all activities.
    /// </summary>
    public interface IActivity
    {
        /// <summary>
        /// Processes by an amount of time.
        /// </summary>
        /// <param name="delta">Amount of time to process in seconds.</param>
        void ProcessUpdate(float delta);
    }
}