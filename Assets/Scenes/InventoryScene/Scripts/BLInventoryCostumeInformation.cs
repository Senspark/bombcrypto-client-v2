using System;
using App;
using Constant;
using Cysharp.Threading.Tasks;
using Data;
using Senspark;
using Game.Dialog;
using Game.Dialog.BomberLand.BLGacha;
using Game.UI.Custom;
using Scenes.AltarScene.Scripts;
using Services;
using Share.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class BLInventoryCostumeInformation : BaseBLItemInformation {
        [SerializeField]
        private Text durationText;
        
        [SerializeField]
        private GameObject buttonSell;
        
        [SerializeField]
        private GameObject premiumFrame;

        [SerializeField]
        private CustomContentSizeFitter contentSizeFitter;

        private IFeatureManager _featureManager;

        protected void Awake() {
            _featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
        }

        public override void UpdateItem(ItemData data, bool showButton = true) {
            base.UpdateItem(data, showButton);
            UpdateDuration(data);
            if (!_featureManager.EnableInventoryListingItem || !showButton) {
                SetEnableSell(false);
            } else {
                SetEnableSell(ItemData.Sellable);
            }
            if (premiumFrame) {
                var productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
                premiumFrame.SetActive(productItemManager.GetItem(data.ItemId).ItemKind == ProductItemKind.Premium);
            }
            if (contentSizeFitter) {
                contentSizeFitter.ForceSnapVertical();
            }
        }

        private void SetEnableSell(bool value) {
            buttonSell.gameObject.SetActive(value);
        }

        private void UpdateDuration(ItemData data) {
            if (durationText == null) {
                return;
            }
            
            if (data.ExpirationAfter == 0) {
                durationText.text = "<color=yellow>FOREVER</color>";
            } else {
                if (data.Used) {
                    var duration = data.Expire - DateTime.UtcNow;
                    durationText.text = 
                        $"1 ITEM EXPIRE IN <color=yellow>{TimeUtil.ConvertTimeToStringDhm(duration).ToUpper()}</color>";
                } else {
                    var duration = TimeSpan.FromMilliseconds(data.ExpirationAfter);
                    durationText.text =
                        $"DURATION <color=#33FF00>{TimeUtil.ConvertTimeToStringDay(duration).ToUpper()}</color> AFTER EQUIPPED";
                }
            }
        }

        public void OnButtonGoToFuseClicked() {
            void OnLoaded(GameObject obj) {
                var altarScene = obj.GetComponent<AltarScene>();
                altarScene.SetDefaultTab(BLTabType.Fuse);
            }
            const string sceneName = "AltarScene";
            SceneLoader.LoadSceneAsync(sceneName, OnLoaded).Forget();
        }
    }
}