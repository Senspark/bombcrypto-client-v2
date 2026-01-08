using Engine.Entities;

using UnityEngine;

namespace Engine.Components
{
    public class BotCollectItem: MonoBehaviour
    {
        private BasicEnemy basicEnemy;
        private Movable movable;
        private Bombable bombable;
        private float currentSpeed;
        private int currentBombLength;
        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            basicEnemy = GetComponent<BasicEnemy>();
            movable = basicEnemy.GetEntityComponent<Movable>();
            bombable = basicEnemy.GetEntityComponent<Bombable>();
        }
        
        public void SetItemToPlayer(EntityType type)
        {
            //EE.ServiceLocator.Resolve<IAudioManager>().PlaySound(Audio.GetItem);

            //switch (type)
            //{
            //    case EntityType.FireUp:
            //        bombable.IncreaseExplosionLength();
            //        break;
            //    case EntityType.BombUp:
            //        bombable.IncreaseMaxBombNumber();
            //        break;
            //    case EntityType.Skate:
            //        movable.Speed += 0.5f;
            //        if (movable.Speed > 6.0f)
            //        {
            //            movable.Speed = 6.0f;
            //        }
            //        break;
            //    case EntityType.Kick:
            //        break;
            //    case EntityType.Skull:
            //        currentSpeed = movable.Speed;
            //        currentBombLength = bombable.ExplosionLength;
            //        StartSkullEffect();
            //        break;
            //    case EntityType.RemoteControl:
            //        break;
            //    case EntityType.PierceBomb:
            //        bombable.ThroughBrick = true;
            //        break;
            //    case EntityType.BombPass:
            //        basicEnemy.WalkThrough.ThroughBomb = true;
            //        bombable.SetBombsThroughAble(true);
            //        break;
            //    case EntityType.Door:
            //        break;
            //}
        }
        private void StartSkullEffect()
        {
            SkullType[] skulls = {
                SkullType.SpeedUp,
                SkullType.Slow,
                SkullType.Sick,
                SkullType.Exhausted
            };

            var r = UnityEngine.Random.Range(0, 4);
            var skullChoice = skulls[r];

            //Stop the current skull effect...
            StopSkullEffect();

            switch (skullChoice)
            {
                case SkullType.SpeedUp:
                    movable.Speed += 5;
                    break;
                case SkullType.Slow:
                    movable.Speed = 1;
                    break;
                case SkullType.Sick:
                    break;
                case SkullType.Exhausted:
                    bombable.ExplosionLength = 1;
                    break;
            }

            basicEnemy.Skull.StartSkullEffect(skullChoice == SkullType.Sick);

        }
        
        public void StopSkullEffect()
        {
            movable.Speed = currentSpeed;
            bombable.ExplosionLength = currentBombLength;
        }
    }
}