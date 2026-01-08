using UnityEngine;

using Engine.Manager;

using UnityEngine.Assertions;

using CodeStage.AntiCheat.ObscuredTypes;

using DG.Tweening;

using IEntityComponent = Engine.Components.IEntityComponent;

namespace Engine.Entities {
    
    public class Entity : MonoBehaviour, IEntity {
        public EntityType Type { get; set; }

        public IndexTree Index { get; } = new IndexTree();

        public IEntityManager EntityManager { get; set; }

        public ObscuredBool IsAlive { get; private set; } = true;

        public ObscuredBool Immortal { get; set; } = false;

        private readonly ComponentContainer _componentContainer = new ComponentContainer();

        private void OnDestroy() {
            DOTween.Kill(transform, true);
        }

        public void DeActive() //wait in a queue = > not active 
        {
            IsAlive = false;
        }

        public bool Resurrect() //phuc sinh
        {
            if (IsAlive) {
                return false;
            }
            Assert.IsTrue(!IsAlive);
            IsAlive = true;
            return true;
        }

        public bool Kill(bool trigger) {
            if (!IsAlive) {
                return false;
            }
            Assert.IsTrue(IsAlive);

            PlayKillSound();
            IsAlive = false;
            EntityManager.MarkDestroy(this, trigger);
            return true;
        }

        public void AddEntityComponent<T> (IEntityComponent component) where T : IEntityComponent {
            _componentContainer.AddComponent<T>(component);
        }
        
        public T GetEntityComponent<T>() where T : IEntityComponent {
            return _componentContainer.GetComponent<T>();
        }
        
        private void PlayKillSound() {
            //if (Type == EntityType.Bubbles || Type == EntityType.Doria)
            //{
            //    EE.ServiceLocator.Resolve<IAudioManager>().PlaySound(Audio.BossDestroy);
            //}
        }
    }

    public class EntityLocation : Entity {
        public int HashLocation { get; set; } = 0;
    }
}