using System;

using Controller;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class BLBarSort : MonoBehaviour {
        private const string PlayerPrefMarketSortID = "BlMarketSortIndex";

        [SerializeField]
        private Text content;

        [SerializeField]
        private GameObject sort;

        private Action<ItemMarketSort> _onSort;

        [SerializeField]
        private ItemMarketSort currentSort;

        public ItemMarketSort CurrentSort => currentSort;

        private Action _onSortUpdate;

        private int _countFrameSkipCheckHide = 0;
        private int _delayHide = 0;

        protected void Awake() {
            sort.SetActive(false);
            currentSort = (ItemMarketSort)PlayerPrefs.GetInt(PlayerPrefMarketSortID, (int)currentSort);
            if (currentSort == ItemMarketSort.ModifyDateDesc || currentSort == ItemMarketSort.ModifyDateAsc) {
                currentSort = ItemMarketSort.PriceDesc;
            }
            var items = sort.transform.GetComponentsInChildren<BLBarSortItem>();
            foreach (var it in items) {
                it.SetOnSort(OnSort);
                if (it.Type == currentSort) {
                    content.text = it.TextContent;
                }
            }
        }

        public void SetOnSortUpdate(Action onSortUpdate) {
            _onSortUpdate = onSortUpdate;
        }

        public void OnBtShowSort() {
            sort.SetActive(true);
            _countFrameSkipCheckHide = 20; // skip check touch after 0.1s
        }

        private void OnSort(BLBarSortItem it) {
            _countFrameSkipCheckHide = 20; // skip check touch after 0.1s
            sort.SetActive(false);
            if (currentSort == it.Type) {
                return;
            }
            content.text = it.TextContent;
            currentSort = it.Type;
            _onSortUpdate?.Invoke();
            PlayerPrefs.SetInt(PlayerPrefMarketSortID, (int)currentSort);
        }

        protected void LateUpdate() {
            if (_countFrameSkipCheckHide > 0) {
                _countFrameSkipCheckHide--;
                _delayHide = 0;
                return;
            }
            if (Input.touches.Length > 0 || Input.GetMouseButtonUp(0)) {
                _delayHide = 10;
            }
            if (_delayHide <= 0) {
                return;
            }
            _delayHide--;
            if (_delayHide <= 0) {
                sort.SetActive(false);
            }
        }
    }
}