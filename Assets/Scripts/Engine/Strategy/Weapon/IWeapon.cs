using System;

using Engine.Components;
using Engine.Entities;
using UnityEngine;

namespace Engine.Strategy.Weapon
{
    public enum WeaponType
    {
        Bomb,
        Enemy
    }

    public interface IWeapon
    {
        WeaponType Type { get; }
        
        ///// <summary>
        ///// Begins shooting
        ///// </summary>
        //bool BeginSpawn();

        ///// <summary>
        ///// Ends the previous shooting call.
        ///// </summary>
        //bool EndSpawn();

        /// <summary>
        /// Update by the specified delta.
        /// </summary>
        /// <param name="delta">Time delta.</param>
        void Update(float delta);

        bool IsLoaderActive();

        Entity Spawn();

        Entity Shoot();

        void ChangeCooldown(float cooldown);
        
        void StopLoader();
        void StartLoader();
    }
}