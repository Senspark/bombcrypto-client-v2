using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Game.Dialog;
using Game.UI;
using Senspark;
using Services;
using UnityEngine;

namespace Controller {
    public class BLInventoryHeroesController : MonoBehaviour {
        [SerializeField]
        private BLHeroesContent content;

        private IInventoryManager _inventoryManager;
        public Action OnShowDialogInfo { set; private get; }

        public  List<InventoryHeroData> Heroes { set; get; }
        private bool _isUpdate;
        
        private void Awake() {
            _inventoryManager = ServiceLocator.Instance.Resolve<IInventoryManager>();
        }

        public void ClearCacheData() {
            Heroes = null;
            _isUpdate = true;
            content.ClearCacheData();
        }
        
        public async Task SetMaxItems() {
            await content.ItemList.SetMaxItems();
        }
        
        public async Task<List<UIHeroData>> LoadData() {
            if (Heroes == null) {
                var result = await _inventoryManager.GetHeroesAsync();
                Heroes = result.ToList();
            }
            var heroes = new List<UIHeroData>();
            foreach (var iter in Heroes) {
                heroes.Add(UIHeroData.ConvertFrom(iter));
            }
            content.SetPageData(heroes.Count);
            content.SetData(heroes, _isUpdate);
            _isUpdate = false;
            return heroes;
        }
        
        public void OnInfoButtonClicked() {
            OnShowDialogInfo?.Invoke();
        }
    }
}