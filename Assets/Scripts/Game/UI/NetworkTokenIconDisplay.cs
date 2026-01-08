using App;

using Senspark;

using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class NetworkTokenIconDisplay : MonoBehaviour {
        [SerializeField]
        private Sprite polygonIcon, bscIcon;
        
        private Image _icon;
        private INetworkConfig _network;
    
        private void Awake() {
            _icon = TryGetComponent<Image>(out var image) ? image : gameObject.AddComponent<Image>();
            _network = ServiceLocator.Instance.Resolve<INetworkConfig>();
        }

        private void OnEnable() {
            _icon.sprite = _network.NetworkType == NetworkType.Binance ? bscIcon : polygonIcon;
        }
    }
}
