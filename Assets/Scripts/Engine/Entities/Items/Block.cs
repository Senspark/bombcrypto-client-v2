using App;

using Engine.Components;
using Engine.UI;

using UnityEngine;

namespace Engine.Entities {
    public class Block : Entity {
        [SerializeField]
        private HealthBar healthBar;
        
        [SerializeField]
        private float token;

        public EntityType blockType { get; set; }
        public GameModeType GameModeType { get; set; }
        public Health health { get; protected set; }
        public bool IsBroken { get; protected set; } = false;

        private void Awake() {
            var updater = new Updater()
                .OnBegin(Init);
            AddEntityComponent<Updater>(updater);

            health = new Health(this, healthBar);
            AddEntityComponent<Health>(health);
        }

        private void Init() {
            health.SetOnIncapacitated(() => { Broken(); });
        }

        public void TakeDamage(float damage) {
            health.TakeDamage(damage);
        }

        public void SetCurrentHealth(float value) {
            health.SetCurrentHealth(value);
        }

        public float GetValue() {
            return health.GetCurrentHealth();
        }

        private void Broken() {
            IsBroken = true;
        }

        public void ShowBrickBreaking() {
            var position = transform.localPosition;
            var brick = (Brick) EntityManager.MapManager.CreateEntity(EntityType.Brick);
            brick.Init(GameModeType, blockType, EntityManager.MapManager.TileIndex);

            var brickTransform = brick.transform;
            brickTransform.SetParent(transform.parent, false);
            brickTransform.localPosition = position;

            EntityManager.AddEntity(brick);
            brick.PlayBroken(null);
        }
    }
}