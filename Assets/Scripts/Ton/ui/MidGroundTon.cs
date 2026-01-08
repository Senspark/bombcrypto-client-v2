using System.Collections.Generic;

using App;

using Senspark;

using UnityEngine;

namespace Ton.ui {
    public class MidGroundTon : MonoBehaviour {
        [SerializeField]
        private GameObject borderBG;

        [SerializeField]
        private List<GameObject> maps;

        private IPlayerStorageManager _playerStoreManager;

        public void Awake() {
            _playerStoreManager = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
        }

        public void Start() {
            var tileIndex = _playerStoreManager.TileSet;
            // tileSet số lẻ là polygon số chẵn là bsc
            if (tileIndex % 2 == 0) {
                borderBG.SetActive(true);
                gameObject.SetActive(false);
            } else {
                borderBG.SetActive(false);
                gameObject.SetActive(true);
                var index = tileIndex / 2;
                for (var i = 0; i < maps.Count; i++) {
                    if (i == index) {
                        maps[i].SetActive(true);
                    } else {
                        maps[i].SetActive(false);
                    }
                }
            }
        }
    }
}