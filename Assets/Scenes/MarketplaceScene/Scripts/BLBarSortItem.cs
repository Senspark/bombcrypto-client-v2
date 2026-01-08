using System;

using Controller;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class BLBarSortItem : MonoBehaviour {
        [SerializeField]
        private Text content;

        [SerializeField]
        private ItemMarketSort type;

        private Action<BLBarSortItem> _onSort;

        public ItemMarketSort Type => type;
        
        public string TextContent => content.text;

        public void SetOnSort(Action<BLBarSortItem> onSort) {
            _onSort = onSort;
        }

        public void OnBtSort() {
            _onSort?.Invoke(this);
            Debug.Log("OnBtSort");
        }
    }
}