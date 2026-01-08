using System;
using System.Collections;
using System.Collections.Generic;
using Engine.Entities;
using UnityEngine;

namespace Engine.Components
{
    using TakeDamageCallback = Action<Entity>;

    public class DamageReceiver : MonoBehaviour
    {
        private TakeDamageCallback takeDamageCallback;

        public void SetOnTakeDamage(TakeDamageCallback callback)
        {
            takeDamageCallback = callback;
        }

        public void TakeDamage(Entity dealer)
        {
            if (dealer != null && dealer.IsAlive)
            {
                takeDamageCallback?.Invoke(dealer);
            }
        }

    }
}
