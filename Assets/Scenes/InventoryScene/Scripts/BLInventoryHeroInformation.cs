using App;

using Cysharp.Threading.Tasks;

using Data;
using Senspark;
using Game.Dialog;

using Scenes.AltarScene.Scripts;

using Services;

using Share.Scripts.Utils;

using UnityEngine;

namespace Game.UI {
    public class BLInventoryHeroInformation : BaseBLHeroInformation {
        [SerializeField]
        private GameObject buttonSell;

        [SerializeField]
        private GameObject premiumFrame;
        
        private IFeatureManager _featureManager;
        protected  override void Awake() {
            base.Awake();
            _featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>(); 
        }

        private void SetEnableSell(bool value) {
            buttonSell.gameObject.SetActive(value);
        }

        public override void UpdateHero(UIHeroData data, bool showButton = true) {
            base.UpdateHero(data, showButton);
            if (!_featureManager.EnableInventoryListingItem) {
                SetEnableSell(false);
                return;
            }
            if (!showButton) {
                SetEnableSell(false);
                return;
            }
            SetEnableSell(data.Sellable);
            if (premiumFrame) {
                var productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
                premiumFrame.SetActive(productItemManager.GetItem(data.HeroId).ItemKind == ProductItemKind.Premium);
            }
        }

        public void OnButtonGoToGrindClicked() {
            void OnLoaded(GameObject obj) {
                var altarScene = obj.GetComponent<AltarScene>();
                altarScene.SetDefaultTab(BLTabType.Grind);
            }
            const string sceneName = "AltarScene";
            SceneLoader.LoadSceneAsync(sceneName, OnLoaded).Forget();
        }
    }
}