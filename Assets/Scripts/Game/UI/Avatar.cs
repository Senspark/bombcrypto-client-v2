using System;
using System.Collections.Generic;
using App;
using Cysharp.Threading.Tasks;
using Senspark;
using Engine.Entities;
using UnityEngine;
using UnityEngine.UI;

public class Avatar : MonoBehaviour {
    [SerializeField]
    private Image img;

    [SerializeField]
    private GameObject heroS, heroL, heroSFake;

    private const string TonPath = "Assets/Scenes/TreasureModeScene/Textures/Pack";
    private const string WebPath = "Assets/Scenes/MainMenuScene/Textures/Pack";

    public void HideImage() {
        img.enabled = false;
        if (heroS != null) {
            heroS.SetActive(false);
        }
        if (heroL != null) {
            heroL.SetActive(false);
        }
        if (heroSFake != null) {
            heroSFake.SetActive(false);
        }
    }

    public async UniTask ChangeImage(PlayerData player) {
        var featureManager =  ServiceLocator.Instance.Resolve<IFeatureManager>();
        await ChangeImage(player.playerType, player.playercolor, featureManager.ShowHeroSIcon, player);
    }

    public async UniTask ChangeImage(PlayerType playerType, PlayerColor playerColor) {
        await ChangeImage(playerType, playerColor, false);
    }

    public void Dim() {
        var color = img.color;
        color.a = 0.8f;
        img.color = color;
    }
    
    private async UniTask ChangeImage(PlayerType playerType, PlayerColor playerColor, bool iShow, PlayerData playerData = null) {
        img.enabled = false;
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
        

        // Không hiển thị S, L trong bản airdrop
        if (AppConfig.IsAirDrop()) {
            iShow = false;
        }
        
        img.sprite = spr;
        img.enabled = true;
        
        var isHeroS = playerData is { IsHeroS: true };
        var isHeroSFake = playerData is { IsHeroS: false, Shield: not null };
        if (heroS != null) {
            heroS.SetActive(isHeroS && !isHeroSFake && iShow);
        }
        if (heroL != null) {
            heroL.SetActive(!isHeroS && !isHeroSFake && iShow);
        }
        if (heroSFake != null) {
            heroSFake.SetActive(!isHeroS && isHeroSFake && iShow);
        }
        
        img.enabled = true;
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
}