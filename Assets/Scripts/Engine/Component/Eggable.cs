using System;
using System.Collections;
using System.Collections.Generic;
using Engine.Entities;
using Engine.Strategy.Weapon;
using UnityEngine;

namespace Engine.Components
{
    public class Eggable : Spawnable
    {

        public IWeapon Weapon { get; private set; }

        public Eggable(Entity entity) {
            entity.GetEntityComponent<Updater>()
                .OnBegin(Init)
                .OnUpdate(delta =>
                {
                    if (Weapon != null)
                    {
                        Weapon.Update(delta);
                    }
                });
        }

        private void Init()
        {
        }

        public Eggable SetWeapon(IWeapon weapon)
        {
            Weapon = weapon;
            return this;
        }
    }
}
