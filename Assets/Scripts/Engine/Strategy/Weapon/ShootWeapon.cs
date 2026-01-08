using System;
using System.Collections;
using System.Collections.Generic;
using Engine.Components;
using Engine.Entities;
using Engine.Strategy.Reloader;
using Engine.Strategy.Shooter;
using UnityEngine;

namespace Engine.Strategy.Weapon
{
    public class ShootWeapon : IWeapon
    {
        private readonly IReloader _reloader;
        private readonly IShooter _shooter;
        private readonly Shootable _shootable;

        public WeaponType Type { get; }

        public bool IsLoaderActive() {
            if (_reloader != null) {
                return _reloader.IsActive;
            }
            return false;
        }

        public ShootWeapon(WeaponType type, IReloader reloader, IShooter shooter, Shootable shootable)
        {
            Type = type;
            _reloader = reloader;
            _shooter = shooter;
            _shootable = shootable;
        }

        public Entity Shoot() {
            return _shooter.Shoot(_shootable);
        }

        public Entity Spawn() {
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

        public void Update(float delta)
        {
            if (_reloader != null) {
                _reloader.Update(delta);
                if (_reloader.IsReady) {
                    _reloader.Reload();
                    _shootable.Shoot();
                }
            }
        }
    }
}