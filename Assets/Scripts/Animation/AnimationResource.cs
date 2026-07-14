using System;
using System.Collections.Generic;
using App;
using Constant;
using Cysharp.Threading.Tasks;
using Engine.Components;
using Engine.Entities;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Animation {
    public enum BrokenType {
        map0 = 0,
        map1 = 1,
        map2 = 2,
        jailHouse = 7,
        woodenChest = 8,
        silverChest = 9,
        goldenChest = 10,
        diamondChest = 11,
        legendChest = 12,
        keyChest = 13,
        bossChest = 19,
    }

    public enum EnemyBombSkin {
        AutoBots = 21,
        BigTank = 22,
        CandyKing = 23,
        BigRockyLord = 24,
        BeetlesKing = 25,
        DeceptionsHeadQuater = 26,
        LordPirates = 27,
        DumplingsMaster = 28,
        PumpkinLord = 29,
        JesterKing = 30
    }

    public enum DefaultEntity {
        Unknown,
        DefaultBomb,
    }

    public enum AnimationAction {
        Moving,
        Standing,
        Idle,
        Shoot,
        Spawn,

        TakeDamage,
        Sleeping,
        Preparing,
        DistanceShoot,

        Explosion,
    }

    [CreateAssetMenu(fileName = "BLAnimationRes", menuName = "BomberLand/AnimationRes")]
    public class AnimationResource : ScriptableObject {

        public async UniTask LoadDataFirstForTon() {
            await GetAnimationResourceTonAsync();
        }
        public async UniTask LoadDataFirstForWeb() {
            await GetAnimationResourceWebAsync();
        }
        
        #region Static Load Sprites
        
        private const string TonPath = "Assets/Scenes/TreasureModeScene/Textures/Pack";

        private static readonly string[] BACKLIGHT_IMAGES_RARE = {
            "Common", "Rare", "SuperRare", "Epic", "Legend", "SuperLegend", "Mega", "SuperMega", "Mystic", "SuperMystic"
        };

        public static async UniTask<Sprite> GetBacklightImageByRarity(int rareIndex, bool forUI) {
            rareIndex = Mathf.Clamp(rareIndex, 0, BACKLIGHT_IMAGES_RARE.Length - 1);
            var path = forUI
                ? $"RarityBacklightsUI/{BACKLIGHT_IMAGES_RARE[rareIndex]}"
                : $"RarityBacklights/{BACKLIGHT_IMAGES_RARE[rareIndex]}";
            var spr = await LoadWithAddressable(path);
            spr.texture.filterMode = FilterMode.Point;
            return spr;
        }

        public static Sprite[] LoadSpritesHeroTr(PlayerType charName) {
            var spriteSheetName = $"Characters/{charName}/{PlayerColor.HeroTr}";
            return Resources.LoadAll<Sprite>(spriteSheetName);
        }

        public static Sprite[] LoadSpritesAvatar(int avatarId) {
            return Resources.LoadAll<Sprite>($"Avatar/{avatarId}/Front");
        }

        private static Sprite[] LoadSpritesHeroTr(PlayerType charName, string subFolder) {
            return LoadSpritesHero(charName, PlayerColor.HeroTr, subFolder);
        }

        private static Sprite[] LoadSpritesHero(PlayerType charName, PlayerColor colorName, string subFolder) {
            var spriteSheetName = $"Characters/{charName}/{colorName}/{subFolder}";
            return Resources.LoadAll<Sprite>(spriteSheetName);
        }

        private static Sprite[] LoadSpritesAvatar(int avatarId, string subFolder) {
            return Resources.LoadAll<Sprite>($"Avatar/{avatarId}/{subFolder}");
        }

        public static Sprite[] LoadSpriteExplode(int explosionSkin) {
            var spriteSheetName = explosionSkin != 0 ? $"{explosionSkin}" : "Fire";
            return Resources.LoadAll<Sprite>($"Explosion/{spriteSheetName}");
        }

        private static Sprite[] LoadSpriteExplode(int explosionSkin, string fileName) {
            var spriteSheetName = explosionSkin != 0 ? $"{explosionSkin}" : "Fire";

            var sprites = new List<Sprite>();
            for (var i = 0; i < 4; i++) {
                sprites.Add(Resources.Load<Sprite>($"Explosion/{spriteSheetName}/{fileName}_0{i}"));
            }
            return sprites.ToArray();
        }

        private static Sprite[] LoadSpriteEnemy(EnemyType enemyType, string subFolder) {
            return Resources.LoadAll<Sprite>($"Enemies/{enemyType}/{subFolder}");
        }

        private static Sprite[] LoadSpriteEnemy(EnemyType enemyType, string subFolder, string fileName, int num) {
            var sprites = new List<Sprite>();
            for (var i = 0; i < num; i++) {
                sprites.Add(Resources.Load<Sprite>($"Enemies/{enemyType}/{subFolder}/{fileName}_0{i}"));
            }
            return sprites.ToArray();
        }

        private static Sprite[] LoadSpriteTileIndex(GameModeType type, int tileIndex) {
            var t = type switch {
                GameModeType.TreasureHuntV2 => "Amazon/",
                _ => string.Empty,
            };
            return Resources.LoadAll<Sprite>($"Bricks/{t}Normal/StoryBoard/Stage{tileIndex}");
        }

        private static Sprite[] LoadSpriteBlockType(GameModeType type, BrokenType brokenType) {
            var t = type switch {
                GameModeType.TreasureHuntV2 => "Amazon/",
                _ => string.Empty,
            };
            var n = brokenType < BrokenType.jailHouse ? "Normal/" : string.Empty;
            return Resources.LoadAll<Sprite>($"Bricks/{t}{n}{brokenType.ToString()}");
        }

        public static Sprite[] LoadSpriteBomb(int bombSkin) {
            var spriteSheetName = $"{bombSkin}";
            var subSprites = new Sprite[4];
            subSprites[0] = Resources.Load<Sprite>($"BombIdle/{spriteSheetName}/1");
            subSprites[1] = Resources.Load<Sprite>($"BombIdle/{spriteSheetName}/2");
            subSprites[2] = Resources.Load<Sprite>($"BombIdle/{spriteSheetName}/3");
            subSprites[3] = Resources.Load<Sprite>($"BombIdle/{spriteSheetName}/4");
            return subSprites;
        }

        public static Sprite[] LoadSpriteEmoji(int itemId) {
            return Resources.LoadAll<Sprite>($"Emoji/{itemId}");
        }
        
        public static UniTask<Sprite> LoadWithAddressable(string path) {
            var result = AddressableLoader.LoadAsset<Sprite>($"{TonPath}/{path}.png");
            
            return result;
        }

        #endregion

        [Serializable]
        public class ResourceFace {
            public Sprite[] Front;
            public Sprite[] Back;
            public Sprite[] Left;
            public Sprite[] Right;
        }

        [Serializable]
        public class ResourceAction {
            public ResourceFace Idle;

            public ResourceFace Moving;
            public ResourceFace Standing;
            public ResourceFace Shoot;

            public Sprite[] Spawn;
            public Sprite[] TakeDamage;
            public Sprite[] Sleep;
            public Sprite[] Prepare;
            public Sprite[] DistanceShot;
            public Sprite[] TurnIntoBomb;
            public Sprite[] Die;

            public Sprite[] ExplodeCenter;
            public Sprite[] ExplodeMiddle;
            public Sprite[] ExplodeEnd;
        }

        [Serializable]
        public class ResourceAnimationPicker {
            public InventoryItemType Type;
            public ResourceAction ActionSprites;
        }

        [Serializable]
        public class ResourceEnemyPicker {
            public EnemyType Type;
            public ResourceAction ActionSprites;
        }

        [Serializable]
        public class ResourceBombPicker {
            public EnemyBombSkin bombSkin;
            public Sprite sprite;
        }

        [Serializable]
        public class TilesetResource {
            public int TileIndex;
            public Sprite[] Sprites;
        }

        [Serializable]
        public class BlockTypeResource {
            public BrokenType Type;
            public Sprite[] Sprites;
        }

        [Serializable]
        public class ResourceBrickPicker {
            public GameModeType Type;
            public SerializableDictionary<int, TilesetResource> TilesetSprites;
            public SerializableDictionary<BrokenType, BlockTypeResource> BlockTypeSprites;
        }

        [Serializable]
        public class ResourceDefaultPicker {
            public Sprite[] sprites;
        }
        
        public AssetReference webReference;
        public AssetReference tonReference;
        
        private static AnimationResourceWeb _animationResourceWeb;
        private static AnimationResourceTon _animationResourceTon;

        private Sprite[] GetSpriteByFace(ResourceFace resource, FaceDirection face) {
            return face switch {
                FaceDirection.Up => resource.Back,
                FaceDirection.Down => resource.Front,
                FaceDirection.Left => resource.Left,
                FaceDirection.Right => resource.Right,
            };
        }

        #region Entities

        public Sprite[] GetDefaultSprite(DefaultEntity entity) {
            if (!AppConfig.IsTon()) {
                return _animationResourceWeb.resourceDefaultAnimation[entity].sprites;
            }
            return _animationResourceTon.resourceDefaultAnimation[entity].sprites;
        }

        public Sprite[] GetSpriteIdle(GachaChestProductId type, FaceDirection face) {
            if (!AppConfig.IsTon()) {
                var res = _animationResourceWeb.resourceAnimation[type].ActionSprites.Idle;
                return GetSpriteByFace(res, face);
            }
            var resource = _animationResourceTon.resourceAnimation[type].ActionSprites.Idle;
            return GetSpriteByFace(resource, face);
        }

        public Sprite[] GetSpriteExplodeCenter(GachaChestProductId type) {
            if (!AppConfig.IsTon()) {
                return _animationResourceWeb.resourceAnimation[type].ActionSprites.ExplodeCenter;
            }
            return _animationResourceTon.resourceAnimation[type].ActionSprites.ExplodeCenter;
        }

        public Sprite[] GetSpriteExplodeMiddle(GachaChestProductId type) {
            if (!AppConfig.IsTon()) {
                return _animationResourceWeb.resourceAnimation[type].ActionSprites.ExplodeMiddle;
            }
            return _animationResourceTon.resourceAnimation[type].ActionSprites.ExplodeMiddle;
        }

        public Sprite[] GetSpriteExplodeEnd(GachaChestProductId type) {
            if (!AppConfig.IsTon()) {
                return _animationResourceWeb.resourceAnimation[type].ActionSprites.ExplodeEnd;
            }
            return _animationResourceTon.resourceAnimation[type].ActionSprites.ExplodeEnd;
        }

        public Sprite GetSpriteBombSkin(EnemyBombSkin bombSkin) {
            if (!AppConfig.IsTon()) {
                return _animationResourceWeb.resourceBombSkin[bombSkin].sprite;
            }
            return _animationResourceTon.resourceBombSkin[bombSkin].sprite;
        }

        public Sprite[] GetSpriteBrickTile(GameModeType type, int tileIndex) {
            if (!AppConfig.IsTon()) {
                return _animationResourceWeb.resourceBrickAnimation[type].TilesetSprites[tileIndex].Sprites;
            }
            return _animationResourceTon.resourceBrickAnimation[type].TilesetSprites[tileIndex].Sprites;
        }

        public Sprite[] GetSpriteBrickBlock(GameModeType type, BrokenType blockType) {
            if (!AppConfig.IsTon()) {
                return _animationResourceWeb.resourceBrickAnimation[type].BlockTypeSprites[blockType].Sprites;
            }
            return _animationResourceTon.resourceBrickAnimation[type].BlockTypeSprites[blockType].Sprites;
        }

        #endregion

        #region Enemy

        public Sprite[] GetSpriteIdle(EnemyType type, FaceDirection face) {
            if (!AppConfig.IsTon()) {
                var resource = _animationResourceWeb.resourceEnemyAnimation[type].ActionSprites.Idle;
                return GetSpriteByFace(resource, face);
            }
            return _animationResourceTon.resourceEnemyAnimation[type].ActionSprites.Idle.Front;

        }

        public Sprite[] GetSpriteMoving(EnemyType type, FaceDirection face) {
            if (!AppConfig.IsTon()) {
                var resource = _animationResourceWeb.resourceEnemyAnimation[type].ActionSprites.Moving;
                return GetSpriteByFace(resource, face);
            }
            return _animationResourceTon.resourceEnemyAnimation[type].ActionSprites.Moving.Front;
        }

        public Sprite[] GetSpriteShoot(EnemyType type, FaceDirection face) {
            if (!AppConfig.IsTon()) {
                var resource = _animationResourceWeb.resourceEnemyAnimation[type].ActionSprites.Shoot;
                return GetSpriteByFace(resource, face);
            }
            return _animationResourceTon.resourceEnemyAnimation[type].ActionSprites.Shoot.Front;
        }

        public Sprite[] GetSpriteSpawn(EnemyType type) {
            if (!AppConfig.IsTon()) {
                return _animationResourceWeb.resourceEnemyAnimation[type].ActionSprites.Spawn;
            }
            return _animationResourceTon.resourceEnemyAnimation[type].ActionSprites.Spawn;
        }

        public Sprite[] GetSpriteTakeDamage(EnemyType type) {
            if (!AppConfig.IsTon()) {
                return _animationResourceWeb.resourceEnemyAnimation[type].ActionSprites.TakeDamage;
            }
            return _animationResourceTon.resourceEnemyAnimation[type].ActionSprites.TakeDamage;
        }

        public Sprite[] GetSpriteDie(EnemyType type) {
            if (!AppConfig.IsTon()) {
                return _animationResourceWeb.resourceEnemyAnimation[type].ActionSprites.Die;
            }
            return _animationResourceTon.resourceEnemyAnimation[type].ActionSprites.Die;
        }

        public Sprite[] GetSpriteSleep(EnemyType type) {
            if (!AppConfig.IsTon()) {
                return _animationResourceWeb.resourceEnemyAnimation[type].ActionSprites.Sleep;
            }
            return _animationResourceTon.resourceEnemyAnimation[type].ActionSprites.Sleep;
        }

        public Sprite[] GetSpritePrepare(EnemyType type) {
            if (!AppConfig.IsTon()) {
                return _animationResourceWeb.resourceEnemyAnimation[type].ActionSprites.Prepare;
            }
            return _animationResourceTon.resourceEnemyAnimation[type].ActionSprites.Prepare;
        }

        public Sprite[] GetSpriteDistanceShoot(EnemyType type) {
            if (!AppConfig.IsTon()) {
                return _animationResourceWeb.resourceEnemyAnimation[type].ActionSprites.DistanceShot;
            }
            return _animationResourceTon.resourceEnemyAnimation[type].ActionSprites.DistanceShot;
        }

        public Sprite[] GetSpriteTurnIntoBomb(EnemyType type) {
            if (!AppConfig.IsTon()) {
                return _animationResourceWeb.resourceEnemyAnimation[type].ActionSprites.TurnIntoBomb;
            }
            return _animationResourceTon.resourceEnemyAnimation[type].ActionSprites.TurnIntoBomb;
        }

        #endregion

        private async UniTask<AnimationResourceTon> GetAnimationResourceTonAsync()
        {
            if (_animationResourceTon == null)
            {
                _animationResourceTon = await AddressableLoader.LoadAsset<AnimationResourceTon>(tonReference);
            }
            return _animationResourceTon;
        }
        private async UniTask<AnimationResourceWeb> GetAnimationResourceWebAsync()
        {
            if (_animationResourceWeb == null)
            {
                _animationResourceWeb = await AddressableLoader.LoadAsset<AnimationResourceWeb>(webReference);
            }
            return _animationResourceWeb;
        }
    }
    
}

public static class SpriteArrayExtension {
    public static Sprite[] ToFillerPoint(this Sprite[] sprites) {
        if(sprites.Length > 0 && sprites[0].texture.filterMode == FilterMode.Point) {
            return sprites;
        }
        foreach (var spr in sprites) {
            spr.texture.filterMode = FilterMode.Point;
        }
        return sprites;
    }
}
