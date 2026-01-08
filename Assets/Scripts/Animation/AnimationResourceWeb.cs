using System;
using System.Collections.Generic;
using App;
using Constant;
using Engine.Components;
using Engine.Entities;
using Game.Dialog;
using Services;
using UnityEngine;

namespace Animation {
    
    [CreateAssetMenu(fileName = "BLAnimationResWeb", menuName = "BomberLand/AnimationResWeb")]
    public class AnimationResourceWeb : ScriptableObject {
        
        #region Static Load Sprites
        private static readonly string[] BACKLIGHT_IMAGES_RARE = {
            "Common", "Rare", "SuperRare", "Epic", "Legend", "SuperLegend"
        };

        public static Sprite GetBacklightImageByRarity(int rareIndex, bool forUI) {
            rareIndex = Mathf.Clamp(rareIndex, 0, BACKLIGHT_IMAGES_RARE.Length - 1);
            var path = forUI
                ? $"RarityBacklightsUI/{BACKLIGHT_IMAGES_RARE[rareIndex]}"
                : $"RarityBacklights/{BACKLIGHT_IMAGES_RARE[rareIndex]}";
            var spr = Resources.Load<Sprite>(path);
            return spr;
        }

        public static Sprite[] LoadSpritesHeroTr(PlayerType charName) {
            Debug.Log("Load here 1");
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
            Debug.Log("Load here 2");
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
        #endregion
        
        
        
        public SerializableDictionaryEnumKey<DefaultEntity, AnimationResource.ResourceDefaultPicker> resourceDefaultAnimation;
        
        public SerializableDictionaryEnumKey<GachaChestProductId, AnimationResource.ResourceAnimationPicker> resourceAnimation;
        
        public SerializableDictionaryEnumKey<PlayerType, AnimationResource.ResourceHeroPicker> resourceHeroAnimation;
        
        public SerializableDictionaryEnumKey<PlayerType, AnimationResource.ResourceHeroSkinPicker> resourceHeroSkinAnimation;
        
        public SerializableDictionaryEnumKey<EnemyType, AnimationResource.ResourceAnimationPicker> resourceEnemyAnimation;

        public SerializableDictionaryEnumKey<EnemyBombSkin, AnimationResource.ResourceBombPicker> resourceBombSkin;

        public SerializableDictionaryEnumKey<GameModeType, AnimationResource.ResourceBrickPicker> resourceBrickAnimation;
        
        [SerializeField]
        protected TextAsset textEarlyConfig;

        private Sprite[] GetSpriteByFace(AnimationResource.ResourceFace resource, FaceDirection face) {
            return face switch {
                FaceDirection.Up => resource.Back,
                FaceDirection.Down => resource.Front,
                FaceDirection.Left => resource.Left,
                FaceDirection.Right => resource.Right,
            };
        }

        #region Entities

        public Sprite[] GetDefaultSprite(DefaultEntity entity) {
            return resourceDefaultAnimation[entity].sprites;
        }
        
        public Sprite[] GetSpriteIdle(GachaChestProductId type, FaceDirection face) {
            var resource = resourceAnimation[type].ActionSprites.Idle;
            return GetSpriteByFace(resource, face);
        }

        public Sprite[] GetSpriteExplodeCenter(GachaChestProductId type) {
            return resourceAnimation[type].ActionSprites.ExplodeCenter;
        }

        public Sprite[] GetSpriteExplodeMiddle(GachaChestProductId type) {
            return resourceAnimation[type].ActionSprites.ExplodeMiddle;
        }

        public Sprite[] GetSpriteExplodeEnd(GachaChestProductId type) {
            return resourceAnimation[type].ActionSprites.ExplodeEnd;
        }

        public Sprite GetSpriteBombSkin(EnemyBombSkin bombSkin) {
            return resourceBombSkin[bombSkin].sprite;
        }

        public Sprite[] GetSpriteBrickTile(GameModeType type, int tileIndex) {
            return resourceBrickAnimation[type].TilesetSprites[tileIndex].Sprites;
        }

        public Sprite[] GetSpriteBrickBlock(GameModeType type, BrokenType blockType) {
            return resourceBrickAnimation[type].BlockTypeSprites[blockType].Sprites;
        }
        
        #endregion
        
        #region Hero
        public Sprite[] GetSpriteIdle(PlayerType type, PlayerColor color, FaceDirection face) {
            var resource = resourceHeroAnimation[type].heroPicker[color].ActionSprites.Idle;
            var result = GetSpriteByFace(resource, face);
            if (result.Length > 0) {
                return result;
            }
            resource = resourceHeroAnimation[type].heroPicker[PlayerColor.White].ActionSprites.Idle;
            result = GetSpriteByFace(resource, face);
            if (result.Length > 0) {
                return result;
            }
            resource = resourceHeroAnimation[type].heroPicker[PlayerColor.HeroTr].ActionSprites.Idle;
            return GetSpriteByFace(resource, face);
        }
        
        public Sprite[] GetSpriteMoving(PlayerType type, PlayerColor color, FaceDirection face) {
            var resource = resourceHeroAnimation[type].heroPicker[color].ActionSprites.Moving;
            var result = GetSpriteByFace(resource, face);
            if (result.Length > 0) {
                return result;
            }
            resource = resourceHeroAnimation[type].heroPicker[PlayerColor.White].ActionSprites.Moving;
            result = GetSpriteByFace(resource, face);
            if (result.Length > 0) {
                return result;
            }
            resource = resourceHeroAnimation[type].heroPicker[PlayerColor.HeroTr].ActionSprites.Moving;
            return GetSpriteByFace(resource, face);
        }

        public Sprite[] GetSpriteTakeDamage(PlayerType type, PlayerColor color) {
            var result = resourceHeroAnimation[type].heroPicker[color].ActionSprites.TakeDamage;
            if (result.Length > 0) {
                return result;
            }
            result = resourceHeroAnimation[type].heroPicker[PlayerColor.White].ActionSprites.TakeDamage;
            if (result.Length > 0) {
                return result;
            }
            return resourceHeroAnimation[type].heroPicker[PlayerColor.HeroTr].ActionSprites.TakeDamage;
        }

        public Sprite[] GetSpriteDie(PlayerType type, PlayerColor color) {
            var result = resourceHeroAnimation[type].heroPicker[color].ActionSprites.Die;
            if (result.Length > 0) {
                return result;
            }
            result = resourceHeroAnimation[type].heroPicker[PlayerColor.White].ActionSprites.Die;
            if (result.Length > 0) {
                return result;
            }
            return resourceHeroAnimation[type].heroPicker[PlayerColor.HeroTr].ActionSprites.Die;
        }
        
        #endregion
        
        #region Enemy
        public Sprite[] GetSpriteIdle(EnemyType type, FaceDirection face) {
            var resource = resourceEnemyAnimation[type].ActionSprites.Idle;
            return GetSpriteByFace(resource, face);
        }
        
        public Sprite[] GetSpriteMoving(EnemyType type, FaceDirection face) {
            var resource = resourceEnemyAnimation[type].ActionSprites.Moving;
            return GetSpriteByFace(resource, face);
        }

        public Sprite[] GetSpriteShoot(EnemyType type, FaceDirection face) {
            var resource = resourceEnemyAnimation[type].ActionSprites.Shoot;
            return GetSpriteByFace(resource, face);
        }
        
        public Sprite[] GetSpriteSpawn(EnemyType type) {
            return resourceEnemyAnimation[type].ActionSprites.Spawn;
        }

        public Sprite[] GetSpriteTakeDamage(EnemyType type) {
            return resourceEnemyAnimation[type].ActionSprites.TakeDamage;
        }

        public Sprite[] GetSpriteDie(EnemyType type) {
            return resourceEnemyAnimation[type].ActionSprites.Die;
        }
        
        public Sprite[] GetSpriteSleep(EnemyType type) {
            return resourceEnemyAnimation[type].ActionSprites.Sleep;
        }

        public Sprite[] GetSpritePrepare(EnemyType type) {
            return resourceEnemyAnimation[type].ActionSprites.Prepare;
        }

        public Sprite[] GetSpriteDistanceShoot(EnemyType type) {
            return resourceEnemyAnimation[type].ActionSprites.DistanceShot;
        }

        public Sprite[] GetSpriteTurnIntoBomb(EnemyType type) {
            return resourceEnemyAnimation[type].ActionSprites.TurnIntoBomb;
        }

        #endregion

        [Button]
        public void UpdateProductType() {
            var earlyConfigManager = new EarlyConfigManager(null);
            earlyConfigManager.Initialize(textEarlyConfig.text);

            var productList = new Dictionary<GachaChestProductId, InventoryItemType>();
            foreach (var item in earlyConfigManager.Items) {
                productList[(GachaChestProductId) item.ItemId] = (InventoryItemType) item.ItemType;
            }

            foreach (var it in resourceAnimation) {
                if (productList.ContainsKey(it.Key)) {
                    it.Value.Type = productList[it.Key];
                } else {
                    it.Value.Type = 0;
                }
            }
            Debug.Log($"Update Type Done with {earlyConfigManager.Items.Length} items");
        }

        [Button]
        public void UpdateAnimationSprites() {
            foreach (var it in resourceAnimation) {
                switch (it.Value.Type) {
                    case InventoryItemType.Hero:
                        var charName = UIHeroData.ConvertFromHeroId((int) it.Key);
                        it.Value.ActionSprites.Idle.Front = LoadSpritesHeroTr(charName, "Front");
                        it.Value.ActionSprites.Idle.Back = LoadSpritesHeroTr(charName, "Back");
                        it.Value.ActionSprites.Idle.Right = LoadSpritesHeroTr(charName, "Right");
                        it.Value.ActionSprites.Idle.Left = LoadSpritesHeroTr(charName, "Right");

                        it.Value.ActionSprites.Moving.Front = LoadSpritesHeroTr(charName, "Front");
                        it.Value.ActionSprites.Moving.Back = LoadSpritesHeroTr(charName, "Back");
                        it.Value.ActionSprites.Moving.Right = LoadSpritesHeroTr(charName, "Right");
                        it.Value.ActionSprites.Moving.Left = LoadSpritesHeroTr(charName, "Right");

                        it.Value.ActionSprites.Die = LoadSpritesHeroTr(charName, "Die");
                        it.Value.ActionSprites.TakeDamage = LoadSpritesHeroTr(charName, "Die");
                        break;
                    case InventoryItemType.Avatar:
                        it.Value.ActionSprites.Idle.Front = LoadSpritesAvatar((int) it.Key, "Front");
                        it.Value.ActionSprites.Idle.Back = LoadSpritesAvatar((int) it.Key, "Front");
                        it.Value.ActionSprites.Idle.Right = LoadSpritesAvatar((int) it.Key, "Right");
                        it.Value.ActionSprites.Idle.Left = LoadSpritesAvatar((int) it.Key, "Right");
                        break;
                    case InventoryItemType.BombSkin:
                        it.Value.ActionSprites.Idle.Front = LoadSpriteBomb((int) it.Key);
                        break;
                    case InventoryItemType.Fire:
                        it.Value.ActionSprites.ExplodeCenter = LoadSpriteExplode((int) it.Key, "fire1");
                        it.Value.ActionSprites.ExplodeMiddle = LoadSpriteExplode((int) it.Key, "fire2");
                        it.Value.ActionSprites.ExplodeEnd = LoadSpriteExplode((int) it.Key, "fire3");
                        break;
                    case InventoryItemType.Emoji:
                        it.Value.ActionSprites.Idle.Front = LoadSpriteEmoji((int) it.Key);
                        break;
                }
            }
        }

        [Button]
        private void UpdateHeroAnimationSprites() {
            var colorKeys = Enum.GetValues(typeof(PlayerColor));
            foreach (var (charName, value) in resourceHeroAnimation) {
                value.Type = charName;
                value.heroPicker = new SerializableDictionary<PlayerColor, AnimationResource.ResourceColorPicker>();
                var i = 0;
                foreach (var c in colorKeys) {
                    var heroColor = (PlayerColor) c;
                    var actionSprites = new AnimationResource.ResourceAction {
                        Idle = new AnimationResource.ResourceFace {
                            Front = LoadSpritesHero(charName, heroColor, "Front"), 
                            Back = LoadSpritesHero(charName, heroColor, "Back"),
                            Right = LoadSpritesHero(charName, heroColor, "Right"),
                            Left = LoadSpritesHero(charName, heroColor, "Right"),
                        },
                        Moving = new AnimationResource.ResourceFace {
                            Front = LoadSpritesHero(charName, heroColor, "Front"),
                            Back = LoadSpritesHero(charName, heroColor, "Back"),
                            Right = LoadSpritesHero(charName, heroColor, "Right"),
                            Left = LoadSpritesHero(charName, heroColor, "Right"),
                        },
                        
                        Die = CreateHeroDieFrames(LoadSpritesHero(charName, heroColor, "Die")),
                        TakeDamage = LoadSpritesHero(charName, heroColor, "Die"),
                    };

                    var colorPicker = new AnimationResource.ResourceColorPicker {
                        heroColor = heroColor, 
                        ActionSprites = actionSprites,
                    };

                    value.heroPicker[heroColor] = colorPicker;
                }
            }
        }

        private Sprite[] CreateHeroDieFrames(Sprite[] die) {
            var list = new List<Sprite>();
            list.AddRange(die);
            list.AddRange(die);
            return list.ToArray();
        }

        [Button]
        public void UpdateEnemyAnimationSprites() {
            foreach (var it in resourceEnemyAnimation) {
                var enemyType = it.Key;
                it.Value.ActionSprites.Idle.Front = LoadSpriteEnemy(enemyType, "Front", "front", 3);
                it.Value.ActionSprites.Idle.Back = LoadSpriteEnemy(enemyType, "Back", "back", 3);
                it.Value.ActionSprites.Idle.Right = LoadSpriteEnemy(enemyType, "Right", "right", 3);
                it.Value.ActionSprites.Idle.Left = LoadSpriteEnemy(enemyType, "Right", "right", 3);

                it.Value.ActionSprites.Moving.Front = LoadSpriteEnemy(enemyType, "Front", "front", 3);
                it.Value.ActionSprites.Moving.Back = LoadSpriteEnemy(enemyType, "Back", "back", 3);
                it.Value.ActionSprites.Moving.Right = LoadSpriteEnemy(enemyType, "Right", "right", 3);
                it.Value.ActionSprites.Moving.Left = LoadSpriteEnemy(enemyType, "Right", "right", 3);
                
                it.Value.ActionSprites.Shoot.Front = LoadSpriteEnemy(enemyType, "Front", "front_shoot", 4);
                it.Value.ActionSprites.Shoot.Back = LoadSpriteEnemy(enemyType, "Back", "back_shoot", 4);
                it.Value.ActionSprites.Shoot.Right = LoadSpriteEnemy(enemyType, "Right", "right_shoot", 4);
                it.Value.ActionSprites.Shoot.Left = LoadSpriteEnemy(enemyType, "Right", "right_shoot", 4);

                it.Value.ActionSprites.Prepare = LoadSpriteEnemy(enemyType, "Prepare");
                it.Value.ActionSprites.Spawn = LoadSpriteEnemy(enemyType, "Spawn");
                it.Value.ActionSprites.Sleep = LoadSpriteEnemy(enemyType, "Sleeping");
                it.Value.ActionSprites.TakeDamage = LoadSpriteEnemy(enemyType, "TakeDamage");
                it.Value.ActionSprites.Die = CreateEnemyDieFrames(LoadSpriteEnemy(enemyType, "Die"));
            }
        }

        [Button]
        public void UpdateBrickAnimationSprites() {
            var brokenKeys = Enum.GetValues(typeof(BrokenType));
            foreach (var it in resourceBrickAnimation) {
                it.Value.Type = it.Key;
                it.Value.BlockTypeSprites = new SerializableDictionary<BrokenType, AnimationResource.BlockTypeResource>();
                it.Value.TilesetSprites = new SerializableDictionary<int, AnimationResource.TilesetResource>();
                foreach (var b in brokenKeys) {
                    var brokenType = (BrokenType) b;
                    it.Value.BlockTypeSprites[brokenType] =
                        new AnimationResource.BlockTypeResource {Type = brokenType, Sprites = LoadSpriteBlockType(it.Key, brokenType)};
                }
                for (var i = 0; i < 9; i++) {
                    it.Value.TilesetSprites[i] =
                        new AnimationResource.TilesetResource {TileIndex = i, Sprites = LoadSpriteTileIndex(it.Key, i)};
                }
            }
        }

        private Sprite[] CreateEnemyDieFrames(Sprite[] die) {
            var list = new List<Sprite>();
            for (var i = 0; i < 7; i++) {
                list.AddRange(die);
            }
            return list.ToArray();
        }
    }
}