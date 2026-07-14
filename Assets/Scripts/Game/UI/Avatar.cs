using System;
using System.Collections.Generic;
using Animation;
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

    private const string CharPath = "Assets/Data/CharactersSprites";

    public void HideImage() {
        img.enabled = false;
        if (heroS) {
            heroS.SetActive(false);
        }
        if (heroL) {
            heroL.SetActive(false);
        }
        if (heroSFake) {
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
        if (HeroSpriteCatalog.Has(playerType)) {
            // Mọi hero trong catalog: portrait qua IHeroSpriteLoader (path-load, icon nếu có).
            var loader = ServiceLocator.Instance.Resolve<IHeroSpriteLoader>();
            var rarity = playerData != null ? (HeroRarity)playerData.rare : HeroRarity.Common;
            spr = await loader.LoadPortrait(playerType, playerColor, rarity);
        }
        else if (AppConfig.IsAirDrop()) {
            if (playerColor == PlayerColor.Skin) {
                if (BHeroSkinPalette.IsSkinSupported(playerType) && playerData != null) {
                    imgPath = $"{playerType}/{playerColor}/{(HeroRarity)playerData.rare}/Front/player_front_01";
                } else {
                    playerColor = BHeroSkinPalette.Resolve(playerType, PlayerColor.White);
                    imgPath = $"{playerType}/{playerColor}/Front/player_front_01";
                }
                spr = await LoadWithAddressable(imgPath);
            } else {
                playerColor = BHeroSkinPalette.Resolve(playerType, playerColor);
                imgPath = playerType == PlayerType.DogeTr
                    ? $"{playerType}/{playerColor}/icon"
                    : $"{playerType}/{playerColor}/Front/player_front_01";
                spr = await LoadWithAddressable(imgPath);
            }
        }
        else {
            playerColor = BHeroSkinPalette.Resolve(playerType, playerColor);
            imgPath = playerType == PlayerType.DogeTr
                ? $"{playerType}/{playerColor}/icon"
                : $"{playerType}/{playerColor}/Front/player_front_01";
            spr = await LoadWithAddressable(imgPath);
            if (!spr) {
                playerColor = PlayerColor.White;
                imgPath = $"{playerType}/{playerColor}/Front/player_front_01";
                spr = await LoadWithAddressable(imgPath);
            }
            if (!spr) {
                playerColor = PlayerColor.HeroTr;
                imgPath = $"{playerType}/{playerColor}/Front/player_front_01";
                spr = await LoadWithAddressable(imgPath);
            }
        }


        // Item có thể đã bị destroy khi đang await load (vd lật trang inventory) → bỏ ghi để khỏi crash.
        if (!this || !img) {
            return;
        }

        // Không hiển thị S, L trong bản airdrop
        if (AppConfig.IsAirDrop()) {
            iShow = false;
        }

        img.sprite = spr;
        img.enabled = true;
        
        var isHeroS = playerData is { IsHeroS: true };
        var isHeroSFake = playerData is { IsHeroS: false, Shield: not null };
        if (heroS) {
            heroS.SetActive(isHeroS && !isHeroSFake && iShow);
        }
        if (heroL) {
            heroL.SetActive(!isHeroS && !isHeroSFake && iShow);
        }
        if (heroSFake) {
            heroSFake.SetActive(!isHeroS && isHeroSFake && iShow);
        }
        
        img.enabled = true;
    }

    private async UniTask<Sprite> LoadWithAddressable(string path) {
        try {
            var actualPath = $"{CharPath}/{path}.png";
            var spr = await AddressableLoader.LoadAsset<Sprite>(actualPath);
            spr.texture.filterMode = FilterMode.Point;
            return spr;
        } catch (Exception) {
            return null;
        }
    }
}