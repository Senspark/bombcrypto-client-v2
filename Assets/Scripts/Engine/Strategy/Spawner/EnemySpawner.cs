using System.Linq;

using Animation;

using App;

using Cysharp.Threading.Tasks;

using DG.Tweening;

using Engine.Components;
using Engine.Entities;
using Engine.Manager;

using Senspark;

namespace Engine.Strategy.Spawner {
    public class EnemySpawner : ISpawner {

        private IEntityManager _entityManager;
        private IEnemyManager _enemyManager;
        private Enemy _enemy;
        
        public EnemySpawner(Enemy enemy) {
            _entityManager = enemy.EntityManager;
            _enemyManager = _entityManager.EnemyManager;
            _enemy = enemy;
        }

        public Entity Spawn(Spawnable spawnable) {

            // Fix lỗi có request spawn Enemy mà không spawn do không còn bomb.
            if (_enemy.EnemyType == EnemyType.BeetlesKing) {
                SpawnEnemiesFromBomb();
                return null;
            }
            
            var storyModeManager = ServiceLocator.Instance.Resolve<IStoryModeManager>();
            UniTask.Void(async () => {
                var result = await storyModeManager.SpawnEnemies((int)_enemy.EnemyType + 1);
                if (result != null && result.Length > 0) {
                    PlaySpawnSound();
                    if (_enemy.EnemyType == EnemyType.DumplingsMaster || 
                             _enemy.EnemyType == EnemyType.PumpkinLord) {
                        SpawnEnemiesFromTop(result);    
                    } else {
                        var tileLocation = ((EnemySpawnable) spawnable).SpawnLocation;
                        _enemyManager.CreateEnemies(result, tileLocation);
                    }
                }
            });
            return null;
        }

        private void PlaySpawnSound() {
            var audioSound = GetAudioSpawn();
            if (audioSound != Audio.None) {
                ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(audioSound);
            }
        }
        
        private Audio GetAudioSpawn() {
                switch (_enemy.EnemyType) {
                    case EnemyType.CandyKing:
                        return Audio.KingSpawn;
                    case EnemyType.DeceptionsHeadQuater:
                        return Audio.RobotSpawn;
                    case EnemyType.LordPirates:
                        return Audio.PirateSpawn;
                    case EnemyType.DumplingsMaster:
                        return Audio.ChefSpawn;
                    case EnemyType.PumpkinLord:
                        return Audio.PumpkinSpawn;
                    case EnemyType.JesterKing:
                        return Audio.ChefSpawn;
                }
            return Audio.None;
        }

        private void SpawnEnemiesFromBomb() {
            var bombs = _entityManager.FindEntities<Bomb>();
            if (bombs.Count <= 0) {
                return;
            }
            _enemy.GetComponent<EnemyAnimator>().PlayShoot();;
            foreach (var bomb in bombs.Where(bomb => bomb.IsAlive)) {
                bomb.SetCountDownEnable(false);
                RequestCreateEnemyFromBomb(bomb);
            }
        }
        
        private void RequestCreateEnemyFromBomb(Bomb bomb) {
            var storyModeManager = ServiceLocator.Instance.Resolve<IStoryModeManager>();
            UniTask.Void(async () => {
                var result = await storyModeManager.SpawnEnemies((int)_enemy.EnemyType + 1);
                if (result != null && result.Length > 0) {
                    PlaySpawnSound();
                    if (bomb.IsAlive) {
                        var location = _entityManager.MapManager.GetTileLocation(bomb.transform.localPosition);
                        bomb.DestroyMe();
                        _enemyManager.CreateEnemy(result[0], location);
                    } else {
                        // Nếu bomb đã bị nổ trước thì tạo quái rơi từ trên xuống.
                        SpawnEnemiesFromTop(result);
                    }
                }
            });
        }

        private async void SpawnEnemiesFromTop(IEnemyDetails[] enemies) {
            var locations =  _entityManager.MapManager.GetRandomEmptyLocations(enemies.Length, 5);
            for (var i = 0; i < enemies.Length && i < locations.Count; i++) {
                var target = locations[i];
                target.y += 5;
                var enemy = await _enemyManager.CreateEnemy(enemies[i], target);
                var position = _entityManager.MapManager.GetTilePosition(locations[i]);

                enemy.IsFallingDown = true;
                var falling = enemy.transform.DOLocalMove(position, 0.5f);
                DOTween.Sequence()
                    .Append(falling)
                    .AppendCallback(() => enemy.IsFallingDown = false);
            }
        }
    }
}
