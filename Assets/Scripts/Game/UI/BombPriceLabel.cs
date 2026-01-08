using System.Threading;

using App;

using Cysharp.Threading.Tasks;

using Senspark;

using Scenes.FarmingScene.Scripts;

using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class BombPriceLabel : MonoBehaviour {
        [SerializeField]
        private Text bombPriceTxt;

        private DialogShopCoinController _controller;
        private CancellationTokenSource _cancellation;

        private void Awake() {
            bombPriceTxt.text = null;
            var blockChainManager = ServiceLocator.Instance.Resolve<IBlockchainManager>();
            var storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            var blockchainStorageManager = ServiceLocator.Instance.Resolve<IBlockchainStorageManager>();
            _controller = new DialogShopCoinController(blockChainManager, blockchainStorageManager, storageManager);
            _cancellation = new CancellationTokenSource();
            UniTask.Void(async (token) => {
                await _controller.SyncData();
                var price = _controller.Info.price;
                bombPriceTxt.text = $"{price} USDT ≈ 1 BOMB";
            }, _cancellation.Token);
        }

        private void OnDestroy() {
            _cancellation.Cancel();
            _cancellation.Dispose();
        }
    }
}