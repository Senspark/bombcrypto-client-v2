using Engine.Entities;
using Engine.Strategy.CountDown;
using UnityEngine;

namespace Engine.Components
{
    public enum SkullType
    {
        SpeedUp,
        Slow,
        Sick,
        Exhausted
    }

    public class SkullComponent : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        private ICountDown CountDown { set; get; }

        private bool SkullEffect { set; get; }
        private bool PlayerIsSick { set; get; }
        public bool isBot;
        [SerializeField]
        private BotManager botManager;
        private void Awake() {
            var entity = GetComponent<Entity>();
            entity.GetEntityComponent<Updater>()
                .OnBegin(() =>
                {
                    CountDown = new AutoCountDown(10);
                })
                .OnUpdate(delta =>
                {
                    if (SkullEffect)
                    {
                        CountDown.Update(delta);
                        if (CountDown.IsTimeEnd)
                        {
                            StopSkullEffect();
                        }
                        else
                        {
                            if (PlayerIsSick)
                            {
                                if (!isBot)
                                {
                                    //EntityManager.PlayerManager.SpawnBomb();
                                }
                                else
                                {
                                    botManager.SpawnBomb();
                                }
                            }
                        }

                        FlashProcess();
                    }

                });

        }

        private float sign = 1;
        private void FlashProcess()
        {
            var color = spriteRenderer.color;
            color.a = color.a + (sign * 0.05f);
            if (color.a > 0.5f)
            {
                sign = -1;
            }
            else if (color.a < 0.0f)
            {
                sign = 1;
            }
            spriteRenderer.color = color;
        }

        public void StartSkullEffect(bool isSick)
        {
            PlayerIsSick = isSick;
            SkullEffect = true;
            CountDown.Reset();
        }

        private void StopSkullEffect()
        {
            SkullEffect = false;
            var color = spriteRenderer.color;
            color.a = 0;
            spriteRenderer.color = color;
            if (!isBot)
            {
                //EntityManager.PlayerManager.StopSkullEffect();
            }
            else
            {
                //botManager.StopSkullEffect();
            }
        }
    }
}