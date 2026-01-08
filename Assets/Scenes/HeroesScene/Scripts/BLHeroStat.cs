using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class BLHeroStat : MonoBehaviour {
        [SerializeField]
        private Image[] bars;

        [SerializeField]
        private Sprite[] barSprites;

        private int _value;
        private int _maxValue;
        private int _equipPlus;
        
        public void SetValue(int value, int maxValue) {
            if (value > 10) {
                value = 10;
            }
            if (maxValue > 10) {
                maxValue = 10;
            }
            _value = value;
            _maxValue = maxValue;
            _equipPlus = 0;
            
            for (var i = 1; i <= value; i++) {
                bars[i - 1].sprite = barSprites[0];
            }

            for (var i = value + 1; i <= maxValue; i++) {
                bars[i - 1].sprite = barSprites[1];
            }

            for (var i = maxValue + 1; i <= bars.Length; i++) {
                bars[i - 1].sprite = barSprites[2];
            }
        }

        public void ResetBars() {
            SetValue(_value, _maxValue);
        }
        
        public void SetEquipValue(int value) {
            _equipPlus += value;
            var sprite = barSprites[3];
            for (var i = _value + 1; i <= _value + _equipPlus && i <= _maxValue; i++) {
                bars[i - 1].sprite = sprite;
            }
        }
    }
}
