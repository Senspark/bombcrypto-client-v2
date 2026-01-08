using System;

namespace Engine.Components
{
    public class CountDown : KillTrigger
    {
        public Action EnemiesCountDownCallback { set; private get; }

        public override void Trigger()
        {
            EnemiesCountDownCallback?.Invoke();
        }

    }
}


