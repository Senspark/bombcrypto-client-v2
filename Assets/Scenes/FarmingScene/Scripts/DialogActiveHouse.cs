using System.Collections.Generic;
using App;
using Cysharp.Threading.Tasks;
using Game.Dialog;
using JetBrains.Annotations;
using Senspark;
using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.FarmingScene.Scripts
{
    public class DialogActiveHouse : Dialog {
        [SerializeField]
        private Text houseName;
        
        [SerializeField]
        private RectTransform content;
        
        [SerializeField]
        private ActiveHouse[] housese;
        
        [SerializeField][CanBeNull]
        private GameObject arrowLeft;
        
        [SerializeField][CanBeNull]
        private GameObject arrowRight;
        
        [SerializeField][CanBeNull]
        private Text otherHouseNameTxt;

        private IHouseStorageManager houseStore;
        private IPlayerStorageManager playerStore;

        private HouseData _houseShow;

        public static UniTask<DialogActiveHouse> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogActiveHouse>();
        }

        private void Start()
        {
            houseStore = ServiceLocator.Instance.Resolve<IHouseStorageManager>();
            playerStore = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
            // Fix tạm delay 2 frame để lấy localPosition của leftBottom và rightTop
            UniTask.Void(async () => {
                await UniTask.DelayFrame(2);
                Init(houseStore.GetActiveHouseData());
            });
            if (houseStore.GetQuantityHouseShow() == 1) {
                HideArrow();
            }
        }

        private async void Init(HouseData houseShow) {
            _houseShow = houseShow;
            var waiting = await DialogWaiting.Create();
            waiting.Show(DialogCanvas);
            for (var i = 0; i < housese.Length; i++)
            {
                await housese[i].SetActive(false);
            }
            var activeIndex = GetIndexFromType(houseShow.HouseType);

            var house = housese[activeIndex];

            houseName.text = GetHouseName(houseShow.HouseType);
            if (otherHouseNameTxt != null) {
                otherHouseNameTxt.text = GetHouseName(houseShow.HouseType);
            }
            if (!AppConfig.IsTon() && content != null) {
                var size = house.GetComponent<RectTransform>().sizeDelta;
                var csize = content.sizeDelta;
    
                //csize.x = size.x * (AppConfig.IsTon() ? 1 : 0.8f) - Screen.width/2;
                csize.x = size.x - (float) Screen.width / 2 * 1.1f;
                content.sizeDelta = csize;
    
                var pos = content.localPosition;
                pos.x = csize.x / 2f;
                content.localPosition = pos;
            }
            await house.SetActive(true);
            for (var i = 0; i < house.GetPlayerCount(); i++)
            {
                house.SetPlayerActive(i, false);
            }
            waiting.Hide();
            var data = playerStore.GetPlayerDataList(
                //DevHoang: Add new airdrop
                HeroAccountType.Nft, HeroAccountType.Trial, HeroAccountType.Ton, 
                HeroAccountType.Sol, HeroAccountType.Ron, HeroAccountType.Bas, HeroAccountType.Vic);
            var heroInHouse = houseStore.GetHeroRestInHouse(houseShow.id);
            var k = 0;
            for (var i = 0; i < data.Count; i++) {
                var p = data[i];
                if (p == null) {
                    continue;
                }

                if (p.stage != HeroStage.Home || !heroInHouse.Contains(p.heroId.Id)) {
                    continue;
                }

                house.ChangeImagePlayer(k, p);
                house.SetPlayerActive(k, true);
                k++;
            }
        }

        public async void OnHeroesButtonClicked()
        {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            var dialog = await DialogHeroesCreator.Create();
            dialog.Show(DialogCanvas);
            Hide();
        }

        public async void OnHouseListClicked()
        {
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

        public void OnBackBtnClicked()
        {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            Hide();
        }

        public void OnArrowLeftBtnClicked() {
            var nextHouseShow = houseStore.GetNextHouseShow(-1, _houseShow);
            Init(nextHouseShow);
        }

        public void OnArrowRightBtnClicked() {
            var nextHouseShow = houseStore.GetNextHouseShow(1, _houseShow);
            Init(nextHouseShow);
        }

        private void HideArrow() {
            if (arrowLeft == null || arrowRight == null) {
                return;
            }
            arrowLeft.SetActive(false);
            arrowRight.SetActive(false);
        }

        private int GetIndexFromType(HouseType type)
        {
            var dictionary = new Dictionary<HouseType, int>()
            {
                { HouseType.TinyHouse, 0 },
                { HouseType.MiniHouse, 1 },
                { HouseType.LuxHouse, 2 },
                { HouseType.PenHouse, 3 },
                { HouseType.Villa, 4 },
                { HouseType.SuperVilla, 5 },
            };

            return dictionary[type];
        }

        private string GetHouseName(HouseType type) {
            return type switch {
                HouseType.TinyHouse  => "Tiny House",
                HouseType.MiniHouse => "Mini House",
                HouseType.LuxHouse => "Lux House",
                HouseType.PenHouse => "PentHouse",
                HouseType.Villa => "Villa",
                HouseType.SuperVilla => "Super Villa"
            };
        }
        protected override void OnYesClick() {
            //do nothing
        }
    }
}