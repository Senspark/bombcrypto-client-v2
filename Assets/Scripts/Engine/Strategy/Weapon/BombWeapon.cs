using System;
using System.Collections;
using System.Collections.Generic;

using Engine.Components;
using Engine.Entities;
using Engine.Strategy.Spawner;

using UnityEngine;

namespace Engine.Strategy.Weapon {
    public class BombWeapon : IWeapon {
        private readonly ISpawner _spawner;
        private readonly Bombable _bombable;

        public WeaponType Type { get; }

        public BombWeapon(WeaponType type, ISpawner spawner, Bombable bombale) {
            Type = type;
            _spawner = spawner;
            _bombable = bombale;
        }

        public bool IsLoaderActive() {
            return false;
        }

        public Entity Spawn() {
            return _spawner.Spawn(_bombable);
        }

        public Entity Shoot() {
            return null;
        }

        public void ChangeCooldown(float cooldown) {
            //do nothing
        }

        public void StopLoader() {
            //do nothing
        }

        public void StartLoader() {
            //do nothing
        }

        public void Update(float delta) {
            // do nothing
        }
    }
}