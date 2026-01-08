using App;
using Cysharp.Threading.Tasks;
using Game.Dialog;
using Senspark;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.FarmingScene.Scripts
{
    public class DialogHouse : Dialog
    {
        [SerializeField] private Button btnBuy;
        [SerializeField] private Button btnList;

        private IHouseStorageManager _houseStore;
        private IFeatureManager _featureManager;

        public static UniTask<DialogHouse> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogHouse>();

        }

        private void Start() {
            _houseStore = ServiceLocator.Instance.Resolve<IHouseStorageManager>();
            _featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
            btnList.interactable = _houseStore.GetHouseCount() + _houseStore.GetLockedHouseCount() > 0;
            btnBuy.interactable = _featureManager.EnableShopForUserFi;
        }

        public async void OnBuyButtonClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            if (AppConfig.IsTon() || AppConfig.IsSolana()) {
                var dialogAirdrop = await DialogShopHouseAirdrop.Create();
                dialogAirdrop.Show(DialogCanvas);
            } else if (AppConfig.IsWebAirdrop()) {
                var dialog = await DialogShopHouseWebAirdrop.Create();
                dialog.Show(DialogCanvas);
            } else {
                var dialog = await DialogShopHouseCreator.Create();
                dialog.Show(DialogCanvas);
            }

            Hide();
        }

        public async void OnListButtonClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            if (AppConfig.IsAirDrop()) {
                var dialogAirdrop = await DialogHouseListAirdrop.Create();
                dialogAirdrop.Show(DialogCanvas);
            } else {
                var dialog = await DialogHouseListCreator.Create();
                dialog.Show(DialogCanvas);
            }

            Hide();
        }

        protected override void OnYesClick() {
            // Do nothing
        }

        public void OnBackBtnClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            Hide();
        }
    }
}