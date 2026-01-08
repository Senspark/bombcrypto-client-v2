using System;
using System.Collections;
using System.Collections.Generic;

using Engine.Entities;
using Engine.UI;
using UnityEngine;

namespace Engine.Components
{
    using IncapacitatedCallback = Action;
    using OnTakeDamage = Action<float, DamageFrom>;

    public enum DamageFrom {
        BombExplode, Thunder
    }

    public class Health : EntityComponentV2
    {
        private readonly HealthBar _bar;
        private float _maxValue;

        private bool IsShowBar { get; set; } = false;
        private float TimeToHideBar { get; } = 2;
        
        public float Stamina { set; get; }
        public bool SaveBattery { set; private get; } = false;

        public void SetShowHealthBar(bool value) {
            _bar.SetShowHealthBar(value);
        }
        
        public void SetShowHealthBarWhenFull(bool value) {
            _bar.SetShowWhenFull(value);
        }
        
        public float MaxHealth
        {
            private get
            {
                return _maxValue;
            }
            set
            {
                _maxValue = value;
            }
        }

        private float _takenDamage;
        private IncapacitatedCallback _incapacitatedCallback;
        private OnTakeDamage _onTakeDamage;
        private Entity _entity;

        public float Value => Math.Max(0, GetMaxHealth() - _takenDamage);

        public Health(Entity entity, HealthBar healthBar) {
            _entity = entity;
            _bar = healthBar;
            ShowBar();
            // HideBar();
            _entity.GetEntityComponent<Updater>()
                .OnBegin(() =>
                {
                    _takenDamage = 0;
                });
        }
        
        public void SetOnIncapacitated(IncapacitatedCallback callback)
        {
            _incapacitatedCallback = callback;
        }

        public void SetOnTakeDamage(OnTakeDamage callback)
        {
            _onTakeDamage = callback;
        }

        public void TakeDamage(float damage)
        {
            if (_entity.Immortal) {
                return;
            }
            
            _takenDamage += damage;
            if (_takenDamage >= GetMaxHealth())
            {
                _takenDamage = GetMaxHealth();
            }

            if (_takenDamage >= GetMaxHealth())
            {
                _incapacitatedCallback?.Invoke();
                HideBar();
            }
            else
            {
                UpdateBar();
                ShowBar();
            }
            
            _onTakeDamage?.Invoke(GetCurrentHealth(), DamageFrom.BombExplode);
        }

        public bool GiveHealth(float health)
        {
            if (Mathf.Approximately(_takenDamage, 0))
            {
                return false;
            }

            _takenDamage -= health;
            if (_takenDamage < 0) {
                _takenDamage = 0;
            }
            return true;
        }

        public bool HealthIsFull()
        {
            return _takenDamage <= 0;
        }

        public float GetPercentHealth() {
            return GetCurrentHealth() / GetMaxHealth();
        }
        
        public float GetCurrentHealth()
        {
            return GetMaxHealth() - _takenDamage;
        }

        public void SetCurrentHealth(float currentHealth, DamageFrom damageFrom = DamageFrom.BombExplode)
        {
            _takenDamage = GetMaxHealth() - currentHealth;
            if (GetMaxHealth() - _takenDamage > 0) {
                UpdateBar();
                ShowBar();
            } else {
                HideBar();
            }

            _onTakeDamage?.Invoke(GetCurrentHealth(), damageFrom);
        }

        public float GetMaxHealth()
        {
            return _maxValue;
        }


        private void ShowBar(bool forceShow = false)
        {
            if (_bar == null)
            {
                return;
            }

            IsShowBar = true;
            _bar.gameObject.SetActive(true);
        }

        private void HideBar()
        {
            if (_bar == null)
            {
                return;
            }

            IsShowBar = false;
            _bar.gameObject.SetActive(false);
        }

        private void UpdateBar()
        {
            if (_bar == null || GetMaxHealth() == 0)
            {
                return;
            }
            var progress = Value / GetMaxHealth();
            if (progress > 1) {
                progress = 1;
            }
            _bar.Progress = progress;
        }

    }
}