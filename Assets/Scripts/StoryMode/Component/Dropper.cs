using System;

using App;

using Cysharp.Threading.Tasks;

using Engine.Entities;

using StoryMode.Entities;

using UnityEngine;

namespace Engine.Components {
    public class Dropper : KillTrigger {
        [SerializeField]
        protected GhostDie prefab;

        [SerializeField]
        private GhostDie prefabBomb;

        protected Sprite _sprite;
        private const string BaseCharPath = "Assets/Scenes/TreasureModeScene/Textures/Pack/";
        private const string BaseEnemyPath = "Assets/Scenes/MainMenuScene/Textures/Pack/";

        public bool IsExitFromDoor { set; private get; }

        private int ExplosionLength { get; } = 3;
        private bool _turnIntoBomb = false;
        private int _bombSkin;
        private int _explosionSkin;
        private float _timeToExplode;
        private Entity _entity;

        private void Awake() {
            _entity = GetComponent<Entity>();
        }

        public void SetTurnIntoBomb(int bombSkin, float timeToExplode = 3) {
            _turnIntoBomb = true;
            _bombSkin = bombSkin;
            _explosionSkin = 0; 
            _timeToExplode = timeToExplode;
        }

        public virtual async void SetHeroSprite(PlayerType charName, PlayerColor colorName) {
            var spriteSheetName = BaseCharPath + "Characters/" + charName + "/" + colorName + "/Front/player_front_00.png";
            _sprite = await LoadSprite(spriteSheetName);
            if (_sprite == null) {
                spriteSheetName = BaseCharPath + "Characters/" + charName + "/" + PlayerColor.White + "/Front/player_front_00.png";
                _sprite = await LoadSprite(spriteSheetName);
            }
        }

        public async void SetEnemySprite(EnemyType enemyType) {
            var spriteSheetName = BaseEnemyPath + "Enemies/" + enemyType + "/Front/front_00.png";
            _sprite = await LoadSprite(spriteSheetName);
        }

        public override void Trigger() {
            if (_turnIntoBomb) {
                CreateTurnIntoBombAnimationGhost();
            } else {
                CreateDieAnimationGhost();
            }
        }

        protected virtual void CreateDieAnimationGhost() {
            var ghost = Instantiate(prefab, transform.parent);
            ghost.InitForDie(_sprite, IsExitFromDoor);

            var ghostTransform = ghost.transform;
            ghostTransform.localPosition = transform.localPosition;
            _entity.EntityManager.AddEntity(ghost);
        }

        private void CreateTurnIntoBombAnimationGhost() {
            var enemy = GetComponent<Enemy>();
            var enemyId = new HeroId(enemy.Id, HeroAccountType.Trial);
            if (_timeToExplode == 0) {
                OnTurnToBomb(enemyId, _bombSkin, enemy.Damage);
                return;
            }

            var parent = transform.parent;
            var ghost = Instantiate(prefabBomb, parent);
            ghost.InitForTurnToBomb(enemyId, _bombSkin, _explosionSkin, enemy.Damage);
            var position = transform.localPosition;
            var location = _entity.EntityManager.MapManager.GetTileLocation(position);
            ghost.transform.localPosition = _entity.EntityManager.MapManager.GetTilePosition(location);

            _entity.EntityManager.AddEntity(ghost);
        }

        private void OnTurnToBomb(HeroId id, int bombSkin, float damage) {
            var bomb = _entity.EntityManager.LevelManager.CreateBomb(transform.localPosition);
            bomb.Init(0, id, _bombSkin, _explosionSkin, damage, 0, ExplosionLength, _timeToExplode, false, null, true);
        }

        private async UniTask<Sprite> LoadSprite(string path) {
            var sprite = await AddressableLoader.LoadAsset<Sprite>(path);
            return sprite;
        }
    }
}