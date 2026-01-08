using System;
using System.Linq;
using System.Threading.Tasks;
using Senspark;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class BLAltarGrindController : MonoBehaviour {
        [SerializeField]
        private BLAltarGrindContent content;
        
        [SerializeField]
        private Button showMoreButton;

        private ITRHeroManager _trHeroManager;
        public Action OnShowDialogInfo { set; private get; }

        private void Awake() {
            _trHeroManager = ServiceLocator.Instance.Resolve<ITRHeroManager>();
        }

        public async Task LoadData() {
            content.SetEnableShowMore(EnableShowMore);
            var result = await _trHeroManager.GetHeroesAsync("SOUL"); 
            content.SetData(result.ToList());
        }
        
        public void OnInfoButtonClicked() {
            OnShowDialogInfo?.Invoke();
        }
        
        private void EnableShowMore(bool state) {
            showMoreButton.gameObject.SetActive(state);
        }

        public void OnShowMoreButtonClicked() {
            content.UpdatePage();
        }
    }
}