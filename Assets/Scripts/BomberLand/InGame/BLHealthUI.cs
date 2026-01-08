using System.Collections.Generic;
using UnityEngine;

namespace BomberLand.InGame {
    public class BLHealthUI : MonoBehaviour {
        [SerializeField]
        private BLHealthIcon iconPrefab;

        private List<BLHealthIcon> _iconList;

        public void Initialized(int health) {
            _iconList = new List<BLHealthIcon>();
            for (var i = 0; i < health; i++) {
                var icon = Instantiate(iconPrefab, transform, false);
                _iconList.Add(icon);
            }
        }

        public void ResetHealth() {
            for (var i = 0; i < _iconList.Count; i++) {
                _iconList[i].ShowOn(true);
            }
        }
        
        public void UpdateHealth(int value) {
            if (value < 0) {
                value = 0;
            }
            if (value > _iconList.Count) {
                value = _iconList.Count;
            }
            for (var i = 0; i < value; i++) {
                _iconList[i].ShowOn(true);
            }
            for (var i = value; i < _iconList.Count; i++) {
                _iconList[i].ShowOn(false);
            }
        }
    }
}