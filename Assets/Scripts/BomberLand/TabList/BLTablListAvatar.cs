using System;
using System.Collections.Generic;
using System.IO;

using App;

using BomberLand.Shop;

using Constant;

using Cysharp.Threading.Tasks;

using Data;

using Engine.Entities;
using Engine.Utils;

using Game.Dialog;
using Game.Dialog.BomberLand.BLFrameShop;
using Game.Dialog.BomberLand.BLGacha;

using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class BLTablListAvatar : MonoBehaviour {
        [SerializeField]
        private Image icon;

        [SerializeField]
        private ImageAnimation imageAnimation;

        [SerializeField]
        private ExplodeAnimation explodeAnimation;

        [SerializeField]
        private BLGachaRes resource;
        
        [SerializeField]
        private BLBoosterResource boosterResource;
        
        [SerializeField]
        private BLShopResource shopResource;
        
        private const string TonPath = "Assets/Scenes/TreasureModeScene/Textures/Pack";
        private const string WebPath = "Assets/Scenes/MainMenuScene/Textures/Pack";
        
        public void ChangeAvatar<T>(T data) {
            switch (data) {
                case UIHeroData hero :
                    if (resource == null) {
                        ChangeImage(hero.HeroType, hero.HeroColor);
                    } else {
                        ChangeAvatarByItemId(hero.HeroId);
                    }
                    break;
                case InventoryChestData chest:
                    ChangeChestImage(chest);
                    break;
                case ItemData item: {
                    if (resource == null) {
                        ChangeBoosterImage(item.ItemId);
                    } else {
                        ChangeAvatarByItemId(item.ItemId);
                    }
                    break;
                }
            }
        }

        public void ChangeAvatarByItemId(int itemId) {
            ChangeItemImage(itemId);
        }

        private async void ChangeImage(PlayerType playerType, PlayerColor playerColor, PlayerData playerData = null) {
            icon.enabled = false;
            string imgPath;
            Sprite spr = null;
            if (AppConfig.IsAirDrop()) {
                if (playerColor == PlayerColor.Skin) {
                    var skinPlayers = new List<PlayerType> { 
                        PlayerType.Ninja , PlayerType.Witch, PlayerType.Knight,
                        PlayerType.Man, PlayerType.Vampire, PlayerType.Frog,
                        PlayerType.King, PlayerType.Pepe, PlayerType.Doge,
                        PlayerType.BomberMan
                    };
                    
                    if (skinPlayers.Contains(playerType) && playerData != null) {
                        imgPath = $"Characters/{playerType}/{playerColor}/{(HeroRarity)playerData.rare}/Front/player_front_01";
                    } else {
                        playerColor = PlayerColor.White;
                        imgPath = $"Characters/{playerType}/{playerColor}/Front/player_front_01";
                    }
                    spr = await LoadWithAddressable(imgPath);
                } else {
                    // Player nhiều màu thì lấy theo màu, tránh sai đường dẫn bị in log lỗi
                    var colorfulPlayers = new List<PlayerType>
                        { PlayerType.Witch, PlayerType.BomberMan, PlayerType.Knight, PlayerType.Man, PlayerType.Vampire };
                    if (!colorfulPlayers.Contains(playerType)) {
                        playerColor = (playerColor != PlayerColor.HeroTr) ? PlayerColor.White : PlayerColor.HeroTr;
                    }
                    if (playerType == PlayerType.DogeTr) {
                        imgPath = $"Characters/{playerType}/{playerColor}/icon";
                    } else {
                        imgPath = $"Characters/{playerType}/{playerColor}/Front/player_front_01";
                    }
                    spr = await LoadWithAddressable(imgPath);
                }
            } 
            else {
                if (playerType == PlayerType.DogeTr) {
                    imgPath = $"Characters/{playerType}/{playerColor}/icon";
                } else {
                    imgPath = $"Characters/{playerType}/{playerColor}/Front/player_front_01";
                }
                spr = await LoadWithAddressable(imgPath);
                if (spr == null) {
                    playerColor = PlayerColor.White;
                    imgPath = $"Characters/{playerType}/{playerColor}/Front/player_front_01";
                    spr = await LoadWithAddressable(imgPath);
                }
                if (spr == null) {
                    playerColor = PlayerColor.HeroTr;
                    imgPath = $"Characters/{playerType}/{playerColor}/Front/player_front_01";
                    spr = await LoadWithAddressable(imgPath);
                }
            }
            
            icon.sprite = spr;
            icon.enabled = true;
        }

        private async UniTask<Sprite> LoadWithAddressable(string path) {
            var resourcePath = AppConfig.IsAirDrop() ? TonPath : WebPath;
            try {
                var actualPath = $"{resourcePath}/{path}.png";
                var spr = await AddressableLoader.LoadAsset<Sprite>(actualPath);
                spr.texture.filterMode = FilterMode.Point;
                return spr;
            } catch (Exception) {
                return null;
            }
        }
        
        public void ChangeImage(Sprite spr) {
            if (!spr) {
                return;
            }
            icon.sprite = spr;
            icon.enabled = true;   
        }

        private async void ChangeItemImage(int itemId) {
            if (imageAnimation == null || explodeAnimation == null) {
                icon.sprite = await resource.GetSpriteByItemId(itemId);
                return;
            }
            var resourcePicker =  await resource.GetAnimationByItemId(itemId);
            var type = resourcePicker.Type;
            if (type > 0 && resourcePicker.AnimationIdle.Length > 0) {
                icon.gameObject.SetActive(false);
                if (type == InventoryItemType.Fire) {
                    explodeAnimation.StartLoop(resourcePicker.AnimationIdle);
                    explodeAnimation.gameObject.SetActive(true);
                    imageAnimation.gameObject.SetActive(false);
                } else {
                    imageAnimation.StartLoop(resourcePicker.AnimationIdle);
                    imageAnimation.gameObject.SetActive(true);
                    explodeAnimation.gameObject.SetActive(false);
                }
            } else {
                icon.sprite = await resource.GetSpriteByItemId(itemId);
                icon.gameObject.SetActive(true);
                imageAnimation.gameObject.SetActive(false);
                explodeAnimation.gameObject.SetActive(false);
            }
        }

        private async void ChangeBoosterImage(int itemId) {
            icon.sprite = await boosterResource.GetSprite(itemId);
        }

        private void ChangeChestImage(InventoryChestData data) {
            icon.sprite = shopResource.GetImageChestShop((ChestShopType)data.ChestType);
        }
    }
}