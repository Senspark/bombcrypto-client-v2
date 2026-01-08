using System.Collections;
using System.Collections.Generic;

using Engine.Components;
using Engine.Entities;
using Engine.Strategy.Reloader;
using Engine.Strategy.Spawner;

using UnityEngine;

namespace Engine.Strategy.Weapon {
    public class EnemyWeapon : IWeapon {

        private readonly IReloader _reloader;
        private readonly ISpawner _spawner;
        private readonly EnemySpawnable _spawnable;

        public WeaponType Type { get; }

        public EnemyWeapon(WeaponType type, IReloader reloader, ISpawner spawner, EnemySpawnable spawnale)
        {
            Type = type;
            _reloader = reloader;
            _spawner = spawner;
            _spawnable = spawnale;
        }

        public bool IsLoaderActive() {
            if (_reloader != null) {
                return _reloader.IsActive;
            }
            return false;
        }

        public Entity Spawn()
        {
            return _spawner.Spawn(_spawnable);
        }

        public Entity Shoot() {
            return null;
        }

        public void ChangeCooldown(float cooldown) {
            _reloader?.ChangeCooldown(cooldown);
        }

        public void StopLoader() {
            _reloader?.Stop();
        }

        public void StartLoader() {
            _reloader?.Start();
        }

        public void Update(float delta) {
            if (_reloader != null) {
                _reloader.Update(delta);
                if (_reloader.IsReady) {
                    _reloader.Reload();
                    _spawnable.Spawn();
                }
            }
        }
    }
}
