using System;
using Engine.Manager;

namespace Engine.Activity
{
    public class DestructionActivity : IActivity
    {
        private readonly IEntityManager manager;

        public DestructionActivity(IEntityManager manager)
        {
            this.manager = manager;
        }

        public void ProcessUpdate(float delta)
        {
            manager.ProcessDestroy();
        }
    }
}