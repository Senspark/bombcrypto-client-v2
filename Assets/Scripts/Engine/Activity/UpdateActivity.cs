using Engine.Components;
using Engine.Manager;

namespace Engine.Activity
{
    public class UpdateActivity : IActivity
    {
        private const int MaxInstances = 5000;

        private readonly IEntityManager manager;
        private readonly Updater[] items = new Updater[MaxInstances];

        public UpdateActivity(IEntityManager manager)
        {
            this.manager = manager;
        }

        public void ProcessUpdate(float delta)
        {
            var components = manager.FindComponents<Updater>();

            var count = components.Count;
            for (var i = 0; i < count; ++i)
            {
                items[i] = components[i];
            }
            for (var i = 0; i < count; ++i)
            {
                items[i].ProcessUpdate(delta);
            }
        }
    }
}