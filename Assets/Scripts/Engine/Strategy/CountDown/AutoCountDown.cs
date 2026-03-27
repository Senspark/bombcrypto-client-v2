using System;

namespace Engine.Strategy.CountDown
{
    public class AutoCountDown : ICountDown
    {
        private CodeStage.AntiCheat.ObscuredTypes.ObscuredFloat cooldown;
        private CodeStage.AntiCheat.ObscuredTypes.ObscuredFloat elapsed;
        private bool enable = true;

        public float Progress => Math.Min(elapsed / cooldown, 1);
        public bool IsTimeEnd => elapsed >= cooldown;
        public float TimeRemain => Math.Max(cooldown - elapsed, 0);
        
        public AutoCountDown(float cooldown, bool enable = true)
        {
            this.cooldown = cooldown;
            this.enable = enable;
        }

        public void SetEnable(bool value) {
            enable = value;
        }

        public void ResetTime(float value)
        {
            cooldown = value;
        }

        public void ExpanseTime(float value)
        {
            cooldown += value;
        }

        public void Reset()
        {
            elapsed = 0;
        }

        public void Update(float delta)
        {
            if (enable) {
                elapsed += delta;
            }
        }
    }
}