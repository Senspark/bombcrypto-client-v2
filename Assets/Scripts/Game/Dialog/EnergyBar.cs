using System;

using App;

using Game.UI;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class EnergyBar : MonoBehaviour {
        [SerializeField]
        private ProgressBar green;

        [SerializeField]
        private ProgressBar red;
        
        [SerializeField]
        private LocalizeText energyText;

        [SerializeField]
        private Image icon;
        
        [SerializeField]
        private Sprite[] iconShield;

        [SerializeField]
        private Sprite iconEnergy;

        [SerializeField]
        private bool showText;

        [SerializeField]
        private Text shieldText;

        private bool _hasShield;
        private int _shield;
        private int _maxShield;
        private int _energy;
        private int _maxEnergy;
    
        private void Awake() {
            if (shieldText) {
                shieldText.gameObject.SetActive(false);
            }
        }

        public void SetValue(bool hasShield, float energy, float maxEnergy, int shield, int maxShield) {
            _hasShield = hasShield;
            _shield = shield;
            _maxShield = maxShield;
            _energy = (int) energy;
            _maxEnergy = (int) maxEnergy;
            
            ShowEnergyTextAndIcon(showText);

            var e = Mathf.Min(energy / maxEnergy, 1);
            if (e >= 0.3f) {
                green.gameObject.SetActive(true);
                red.gameObject.SetActive(false);
                green.SetProcess(e);
            } else {
                green.gameObject.SetActive(false);
                red.gameObject.SetActive(true);
                red.SetProcess(e);
            }
        }

        public void UpdateUi(PlayerData player) {
            _hasShield = player.Shield != null;
            if (!_hasShield) {
                _shield = 0;
                _maxShield = 0;
            }

        }

        public void OnTriggerPointerEnter() {
            if (!_hasShield) {
                return;
            }
            ShowEnergyTextAndIcon(false);
            ShowShieldTextAndIcon(true);
        }

        public void OnTriggerPointerExit() {
            if (!_hasShield) {
                return;
            }
            ShowShieldTextAndIcon(false);
            ShowEnergyTextAndIcon(true);
        }

        private void ShowEnergyTextAndIcon(bool showEnergyText) {
            energyText.gameObject.SetActive(showEnergyText);
            
            if (_energy >= _maxEnergy) {
                energyText.SetNewKey(LocalizeKey.hero_energy_full);
            } else {
                energyText.SetNewText($"{_energy:N0}/{_maxEnergy:N0}");
            }
            
            if (icon && iconEnergy) {
                icon.sprite = iconEnergy;
            }
        }

        private void ShowShieldTextAndIcon(bool showShieldText) {
            if (!_hasShield || !shieldText || !icon || !showText) {
                return;
            }
            shieldText.gameObject.SetActive(showShieldText);
            shieldText.text = _shield < 1 ? "Need Repair" : $"{_shield:N0}/{_maxShield:N0}";
            icon.sprite = GetShieldIcon(_shield, _maxShield);
        }

        private Sprite GetShieldIcon(int amount, int total) {
            var t = amount / (float) total;
            var spr = t switch {
                > 0.7f => iconShield[3],
                > 0.5f => iconShield[2],
                > 0.3f => iconShield[1],
                _ => iconShield[0]
            };
            return spr;
        }
    }
}