using System;

using Animation;

using Engine.Entities;
using Engine.Strategy.Weapon;
using UnityEngine;

namespace Engine.Components
{
    using SpawnLocationCallback = Func<Vector2Int>;
    public class EnemySpawnable : Spawnable
    {
        private SpawnLocationCallback _spawnLocationCallback;
        public IWeapon Weapon { get; private set; }

        public Vector2Int SpawnLocation => _spawnLocationCallback?.Invoke() ?? Vector2Int.zero;

        private Boss _boss;

        public bool Active { private get; set; } = true;
        
        public EnemySpawnable(Boss boss) {
            _boss = boss;
            _boss.GetEntityComponent<Updater>()
                .OnUpdate(delta => {
                    Weapon?.Update(delta);
                });
        }

        public void ChangeCooldown(float cooldown) {
            Weapon.ChangeCooldown(cooldown);
        }

        public void Spawn() {
            if (!Active) {
                return;
            }
            _boss.GetComponent<EnemyAnimator>().PlaySpawn(DoSpawn);
        }

        public void DoSpawn() {
            Weapon.StopLoader();
            Weapon.Spawn();
            _boss.OnAfterSpawn();
        }
        
        public EnemySpawnable SetWeapon(IWeapon weapon) {
            
            Weapon = weapon;
            return this;
        }
        public EnemySpawnable SetSpawnLocation(SpawnLocationCallback callback)
        {
            _spawnLocationCallback = callback;
            return this;
        }
        
    }

}