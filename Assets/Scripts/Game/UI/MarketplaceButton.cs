// using App;
//
// using EE;
//
// using Marketplace.UI;
//
// using UnityEngine;
//
// namespace Game.UI {
//     public class MarketplaceButton : MonoBehaviour {
//         private ISoundManager _soundManager;
//         private ISceneLoader _sceneLoader;
//
//         private void Awake() {
//             _sceneLoader = ServiceLocator.Resolve<ISceneLoader>();
//             _soundManager = ServiceLocator.Resolve<ISoundManager>();
//             var featureManager = ServiceLocator.Resolve<IFeatureManager>();
//             gameObject.SetActive(featureManager.EnableMarketplace);
//         }
//
//         public void OnBtnClicked() {
//             _soundManager.PlaySound(Audio.Tap);
//             _sceneLoader.LoadScene<MarketplaceScene>(nameof(MarketplaceScene));
//         }
//     }
// }