using System.Collections.Generic;
using System.Threading.Tasks;

using Data;

using Senspark;

using Engine.Utils;

using Services;

using UnityEngine;

namespace Game.UI {
    public class BLRedDotInventoryButton : MonoBehaviour {
        [SerializeField]
        private AnimationZoom redDot;

        private IInventoryManager _inventoryManager;

        private void Awake() {
            _inventoryManager = ServiceLocator.Instance.Resolve<IInventoryManager>();
        }

        private void OnDestroy() {
            redDot = null;
        }

        public async Task CheckRedDot() {
            var result = await _inventoryManager.GetChestAsync();
            // redDot is null => did destroy
            if(!redDot) {
                return;
            }
            UpdateRedDot(result);
        }

        private void UpdateRedDot(IEnumerable<InventoryChestData> chestData) {
            var canOpen = false;
            foreach (var iter in chestData) {
                var stage = iter.GetChestStage();
                if (stage != InventoryChestStage.CanOpen) {
                    continue;
                }
                canOpen = true;
                break;
            }
            if (canOpen) {
                redDot.gameObject.SetActive(true);
                redDot.Play();
            } else {
                redDot.gameObject.SetActive(false);
            }
        }
    }
}