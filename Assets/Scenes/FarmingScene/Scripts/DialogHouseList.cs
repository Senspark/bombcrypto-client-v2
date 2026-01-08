using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;
using Game.Manager;

using Senspark;

using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.FarmingScene.Scripts
{
    public interface IDialogHouseList {
        void Show(Canvas canvas);
    }

    public static class DialogHouseListCreator {
        public static async UniTask<IDialogHouseList> Create() {
            if (ScreenUtils.IsIPadScreen()) {
                return await DialogHouseListPad.Create();
            }
            return await DialogHouseList.Create();
        }
    }
    
    public class DialogHouseList : Dialog, IDialogHouseList {
        [SerializeField] private ScrollRect scroller;
        [SerializeField] private HouseListItem housePrefab;
        [SerializeField] private Transform houseContain;

        [SerializeField] private Text houseName;

        private IServerManager _serverManager;
        private HouseListItem[] items;
        private IHouseStorageManager houseStore;
        private int indexChoose;

        public static async UniTask<IDialogHouseList> Create() {
            return await ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogHouseList>();
        }

        private void Start() {
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            houseStore = ServiceLocator.Instance.Resolve<IHouseStorageManager>();
            Init();
        }

        private void Init()
        {
            foreach (Transform child in houseContain)
            {
                Destroy(child.gameObject);
            }

            var num = houseStore.GetHouseCount();
            if (items == null)
            {
                items = new HouseListItem[num];
            }

            for (var i = 0; i < num; i++)
            {
                if (houseStore.GetHouseData(i) == null)
                {
                    items[i] = null;
                }
                else
                {
                    var item = Instantiate(housePrefab, houseContain, false);
                    items[i] = item.GetComponent<HouseListItem>();
                    items[i].SetInfo(i, houseStore.GetHouseData(i), OnItemClicked);
                }
            }

            var activeIndex = houseStore.GetActiveIndex();
            if (activeIndex > 0)
            {
                OnItemClicked(houseStore.GetActiveIndex(), houseStore.GetHouseData(activeIndex));
            }
            else
            {
                for (var i = 0; i < num; i++)
                {
                    if (items[i] != null)
                    {
                        OnItemClicked(i, houseStore.GetHouseData(i));
                        break;
                    }
                }
            }
        }

        private void OnItemClicked(int index, HouseData house)
        {
            indexChoose = index;

            foreach (var item in items)
            {
                if (item != null)
                {
                    item.SetActive(false);
                }
            }

            items[index].SetActive(true);

            var data = houseStore.GetHouseData(index);
            houseName.text = DefaultHouseStoreManager.GetHouseName(data.HouseType);
        }

        public void OnButtonHelpClicked()
        {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            DialogHouseHelp.Create().ContinueWith(dialog => {
                dialog.Show(DialogCanvas);
            });
        }

        public async void OnButtonUseClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            if (indexChoose == houseStore.GetActiveIndex()) {
                var dialog = await DialogActiveHouse.Create();
                dialog.Show(DialogCanvas);
                Hide();
            } else {
                if (_serverManager != null) {
                    var house = houseStore.GetHouseData(indexChoose);
                    var waiting = new WaitingUiManager(DialogCanvas);
                    waiting.Begin();
                    UniTask.Void(async () => {
                            await _serverManager.Pve.ActiveBomberHouse(house.genID, house.id);
                            waiting.End();
                        });
                }
                Hide();
            }
        }

        public void SetItemOn(int index)
        {
            OnItemClicked(index, null);
            if (index >= 6)
            {
                SnapTo(items[index].GetComponent<RectTransform>());
            }
        }

        public void OnCloseBtnClicked()
        {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            Hide();
        }

        protected override void OnYesClick() {
            // Do nothing
        }

        private void SnapTo(RectTransform child)
        {
            var contentPos = (Vector2)scroller.transform.InverseTransformPoint(scroller.content.position);
            var childPos = (Vector2)scroller.transform.InverseTransformPoint(child.position);
            var endPos = scroller.content.anchoredPosition;
            endPos.y = contentPos.y - childPos.y;
            scroller.content.anchoredPosition = endPos;
        }
    }
}
