using App;
using Cysharp.Threading.Tasks;
using Senspark;
using Scenes.FarmingScene.Scripts;
using UnityEngine;

namespace Game.UI {
    public class HouseButton : MonoBehaviour {
        [SerializeField]
        private Canvas canvasDialog;
        
        [SerializeField]
        private LevelScene levelScene;

        private ISoundManager _soundManager;
        private IHouseStorageManager _houseStorageManager;
        private ObserverHandle _handle;

        private void Awake() {
            var featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
            var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _houseStorageManager = ServiceLocator.Instance.Resolve<IHouseStorageManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            var canShow = featureManager.EnableShopForUserFi || AppConfig.IsSolana();
            
            var active = true;
            if (!canShow) {
                active = _houseStorageManager.GetHouseCount() > 0;
                gameObject.SetActive(active);
            }

            if (active) {
                _handle = new ObserverHandle();
                _handle.AddObserver(serverManager, new ServerObserver {
                    OnSyncHouse = OnSyncHouse
                });
            }
        }

        private void OnDestroy() {
            _handle?.Dispose();
        }

        public async void OnBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            if (levelScene != null) {
                levelScene.EnableDialogBackground(true);
            }
            var activeIndex = _houseStorageManager.GetActiveIndex();
            if (activeIndex >= 0) {
                var dialog = await DialogActiveHouse.Create();
                dialog.Show(canvasDialog);
                dialog.OnDidHide(() => {
                    if (levelScene) {
                        levelScene.ResetButtonEvents();
                        levelScene.EnableDialogBackground(false);
                    }
                });
            } else {
                var dialog = await DialogHouse.Create();
                dialog.Show(canvasDialog);
                dialog.OnDidHide(() => {
                    if (levelScene) {
                        levelScene.ResetButtonEvents();
                        levelScene.EnableDialogBackground(false);
                    }
                });
            }
        }
        
        private void OnSyncHouse(ISyncHouseResponse response) {
            var newIds = response.NewIds;
            if (newIds.Length > 0) {
                var newHouse = _houseStorageManager.GetHouseDataFromId(newIds[0]);
                DialogNewHouse.Create().ContinueWith(dialogNew => {
                    dialogNew.SetInfo(newHouse);
                    dialogNew.OnDidHide(() => { ShowHouseListWithIndexOn(_houseStorageManager.GetIndexFromId(newHouse.id)); });
                    dialogNew.Show(canvasDialog);
                });
            }
        }
        
        private void ShowHouseListWithIndexOn(int _) {
            DialogHouseListCreator.Create().ContinueWith(dialog => { dialog.Show(canvasDialog); });
        }
    }
}