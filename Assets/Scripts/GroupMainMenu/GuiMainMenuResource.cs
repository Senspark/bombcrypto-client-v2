using System;
using App;
using Game.Dialog;
using UnityEngine;

namespace GroupMainMenu {
    public enum MenuElement {
        ProfileCard,
        HeaderBar,
        Settings,
        GreenButton,
        BrownButton,
        Hero,
        ChangeMode,
        PlayButton,
        FindMatch,
        HeroSelector,
        DailyGiftButton,
        InventoryButton,
        RankButton,
        StarterPack,
        TournamentMode,
        IpadHeaderBar,
        WarningPointDecay,
        DailyTaskButton,
        DailyTaskNoti
    }

    public enum ButtonType {
        Market,
        Quest,
        Shop,
        DailyGift,
        Rank,
        Altar,
        Inv,
        Hero,
        TreasureHunt,
        DailyTask
    }
    
    [CreateAssetMenu(fileName = "GuiMainMenuRes", menuName = "BomberLand/GuiMainMenuRes")]
    public class GuiMainMenuResource : ScriptableObject {
        [Serializable]
        public class ResourcePicker {
            public GameObject prefab;
        }
        
        [Serializable]
        public class ButtonResourcePicker {
            public Sprite sprite;
            public RuntimeAnimatorController animator;
            public string title;
        }
        
        public SerializableDictionaryEnumKey<MenuElement, ResourcePicker> resource;
        public SerializableDictionaryEnumKey<ButtonType, ButtonResourcePicker> buttonResource;

        public ButtonResourcePicker GetButtonResource(ButtonType buttonType) {
            return buttonResource[buttonType];
        }
        
        public BLProfileCard CreateProfileCared(Transform parent) {
            var prefab = resource[MenuElement.ProfileCard].prefab;
            var profileCard = Instantiate(prefab, parent);
            return profileCard.GetComponent<BLProfileCard>();
        }
        
        public MMWarningPointDecay CreateWarningPointDecay(Transform parent) {
            var prefab = resource[MenuElement.WarningPointDecay].prefab;
            var obj = Instantiate(prefab, parent);
            return obj.GetComponent<MMWarningPointDecay>();
        }

        public MMHeaderBar CreateHeaderBar(Transform parent, Action<BlockRewardType> onTokenClickedCallback) {
            var prefab = resource[MenuElement.HeaderBar].prefab;
            var obj = Instantiate(prefab, parent);
            var headerBar = obj.GetComponent<MMHeaderBar>();
            headerBar.SetOnTokenClickedCallback(onTokenClickedCallback);
            return headerBar;
        }

        public MMHeaderBar CreateIpadHeaderBar(Transform parent, Action<BlockRewardType> onTokenClickedCallback) {
            var prefab = resource[MenuElement.IpadHeaderBar].prefab;
            var obj = Instantiate(prefab, parent);
            var headerBar = obj.GetComponent<MMHeaderBar>();
            headerBar.SetOnTokenClickedCallback(onTokenClickedCallback);
            return headerBar;
        }
        
        
        public MMButton CreateSettingButton(Transform parent, Action onClickedCallback) {
            return CreateButton(MenuElement.Settings, parent, onClickedCallback);
        }

        public MainMenuButton CreateGreenButton(ButtonType buttonType, Transform parent, Action<ButtonType> onClickedCallback) {
            var prefab = buttonType switch {
                ButtonType.DailyGift => resource[MenuElement.DailyGiftButton].prefab,
                ButtonType.Rank => resource[MenuElement.RankButton].prefab,
                ButtonType.DailyTask => resource[MenuElement.DailyTaskButton].prefab,
                _ => resource[MenuElement.GreenButton].prefab,
            };
            var obj = Instantiate(prefab, parent);
            var button = obj.GetComponent<MainMenuButton>();
            button.SetType(buttonType, onClickedCallback);
            return button;
        }
        
        public MainMenuButton CreateBrownButton(ButtonType buttonType, Transform parent, Action<ButtonType> onClickedCallback) {
            var prefab = buttonType switch {
                ButtonType.Inv => resource[MenuElement.InventoryButton].prefab,
                _ => resource[MenuElement.BrownButton].prefab,
            };
            var obj = Instantiate(prefab, parent);
            var button = obj.GetComponent<MainMenuButton>();
            button.SetType(buttonType, onClickedCallback);
            return button;
        }

        public MMHeroChoose CreateHero(Transform parent,
            HeroChooseCallback callback
        ) {
            var prefab = resource[MenuElement.Hero].prefab;
            var obj = Instantiate(prefab, parent);
            var hero = obj.GetComponent<MMHeroChoose>();
            hero.SetHeroChooseCallback(callback);
            return hero;
        }
        
        public MMChangeMode CreateChangeMode(Transform parent) {
            var prefab = resource[MenuElement.ChangeMode].prefab;
            var obj = Instantiate(prefab, parent);
            var button = obj.GetComponent<MMChangeMode>();
            return button;
        }

        public void CreateTournamentMode(Transform parent) {
            var prefab = resource[MenuElement.TournamentMode].prefab;
            Instantiate(prefab, parent);
        }
        
        public BLDialogIapPackIcons CreateStarterPack(Transform parent) {
            var prefab = resource[MenuElement.StarterPack].prefab;
            var obj = Instantiate(prefab, parent);
            var starterPack = obj.GetComponent<BLDialogIapPackIcons>();
            return starterPack;
        }
        
        public MMButton CreateButtonPlay(Transform parent, Action onClickedCallback) {
            return CreateButton(MenuElement.PlayButton, parent, onClickedCallback);
        }

        public MMButton CreateFindMatch(Transform parent, Action onCancelClickedCallback) {
            return CreateButton(MenuElement.FindMatch, parent, onCancelClickedCallback);   
        }

        public MMHeroSelector CreateHeroSelector(Transform parent) {
            var prefab = resource[MenuElement.HeroSelector].prefab;
            var obj = Instantiate(prefab, parent);
            var heroSelector = obj.GetComponent<MMHeroSelector>();
            return heroSelector;
        }
        
        private MMButton CreateButton(MenuElement element, Transform parent, Action onClickedCallback) {
            var prefab = resource[element].prefab;
            var obj = Instantiate(prefab, parent);
            var button = obj.GetComponent<MMButton>();
            button.SetOnClickedCallback(onClickedCallback);
            return button;
        }
        
        public MMDailyTaskNoti CreateDailyTaskNoti(Transform parent, Canvas canvas) {
            var prefab = resource[MenuElement.DailyTaskNoti].prefab;
            var obj = Instantiate(prefab, parent).GetComponent<MMDailyTaskNoti>();
            return obj;
        }
    }
}