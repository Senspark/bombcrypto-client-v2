using System;

using App;

using Constant;

using Cysharp.Threading.Tasks;

using Engine.Entities;

using Services;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Dialog.BomberLand.BLGacha {
    [CreateAssetMenu(fileName = "BLGachaRes", menuName = "BomberLand/GachaRes")]
    public class BLGachaRes : ScriptableObject {
        [Serializable]
        public class ResourcePicker {
            public Sprite sprite;
        }

        [Serializable]
        public class ResourceAnimationPicker {
            public InventoryItemType Type;
            public Sprite[] AnimationIdle;
        }

        [Serializable]
        public class ResourceDesc {
            public string desc;
        }
        

        public AssetReference resourceChestRef;
        public AssetReference resourceItemRef;
        public AssetReference resourcePvpRankRef;
        public AssetReference resourceEmptySkinChestRef;
        public AssetReference resourceAvatarRef;
        public AssetReference resourceAnimationRef;
        public AssetReference resourceDescriptionRef;

        private ResourceChestSo _resourceChestSo;
        private ResourceItemSo _resourceItemSo;
        private ResourcePvpRankSo _resourcePvpRankSo;
        private ResourceEmptySkinChestSo _resourceEmptySkinChestSo;
        private ResourceAvatarSo _resourceAvatarSo;
        private ResourceAnimationSo _resourceAnimationSo;
        private ResourceDescriptionSo _resourceDescriptionSo;
        
        // Những biến này dùng để support nhiều class gọi lấy data trong cùng 1 frame ko bị gọi load nhiều lần bởi việc load async
        private readonly UniTaskCompletionSource _chestTcs = new();
        private readonly UniTaskCompletionSource _itemTcs = new();
        private readonly UniTaskCompletionSource _pvpRankTcs = new();
        private readonly UniTaskCompletionSource _emptySkinChestTcs = new();
        private readonly UniTaskCompletionSource _avatarTcs = new();
        private readonly UniTaskCompletionSource _animationTcs = new();
        private readonly UniTaskCompletionSource _descriptionTcs = new();
        
        private bool _chestLoaded;
        private bool _itemLoaded;
        private bool _pvpRankLoaded;
        private bool _emptySkinChestLoaded;
        private bool _avatarLoaded;
        private bool _animationLoaded;
        private bool _descriptionLoaded;

        public async UniTask<Sprite> GetSpriteByItemId(int itemId) {
            return await GetSpriteByItemId((GachaChestProductId)itemId);
        }

        public async UniTask<Sprite> GetSpriteByItemId(GachaChestProductId type) {
            if (_resourceChestSo != null) {
                return _resourceChestSo.GetSpriteByItemId(type); 
            }
            if (_chestLoaded) {
                await _chestTcs.Task;   
                return _resourceChestSo.GetSpriteByItemId(type);
            }
            _chestLoaded = true;
            _resourceChestSo = await AddressableLoader.LoadAsset<ResourceChestSo>(resourceChestRef);
            _chestTcs.TrySetResult();
            return _resourceChestSo.GetSpriteByItemId(type);
        }

        public async UniTask<Sprite> GetSpriteByItemType(ItemType type) {
            if (_resourceItemSo != null) {
                return _resourceItemSo.GetSpriteByItemType(type);
            }
            if (_itemLoaded) {
                await _itemTcs.Task;
                return _resourceItemSo.GetSpriteByItemType(type);
            }
            _itemLoaded = true;
            _resourceItemSo = await AddressableLoader.LoadAsset<ResourceItemSo>(resourceItemRef);
            _itemTcs.TrySetResult();
            return _resourceItemSo.GetSpriteByItemType(type);
        }

        public async UniTask<Sprite> GetSpriteByRewardType(string rewardType) {
            var type = RewardUtils.ConvertToBlockRewardType(rewardType);
            return await GetSpriteByRewardType(type);
        }

        public async UniTask<Sprite> GetSpriteByRewardType(BlockRewardType rewardType) {
            if (_resourceChestSo != null) {
                return _resourceChestSo.GetSpriteByRewardType(rewardType);
            }
            if(_chestLoaded) {
                await _chestTcs.Task;
                return _resourceChestSo.GetSpriteByRewardType(rewardType);
            }
            _chestLoaded = true;
            _resourceChestSo = await AddressableLoader.LoadAsset<ResourceChestSo>(resourceChestRef);
            _chestTcs.TrySetResult();
            return _resourceChestSo.GetSpriteByRewardType(rewardType);
        }

        public async UniTask<Sprite> GetSpriteByPvpRank(PvpRankType pvpRankType) {
            if (_resourcePvpRankSo != null) {
                return  _resourcePvpRankSo.GetSpriteByPvpRank(pvpRankType);
            }
            if (_pvpRankLoaded) {
                await _pvpRankTcs.Task;
                return _resourcePvpRankSo.GetSpriteByPvpRank(pvpRankType);
            }
            _pvpRankLoaded = true;
            _resourcePvpRankSo = await AddressableLoader.LoadAsset<ResourcePvpRankSo>(resourcePvpRankRef);
            _pvpRankTcs.TrySetResult();
            return _resourcePvpRankSo.GetSpriteByPvpRank(pvpRankType);
        }

        public async UniTask<Sprite> GetSpriteByEmptySkinChest(SkinChestType type) {
            if (_resourceEmptySkinChestSo != null) {
                return _resourceEmptySkinChestSo.GetSpriteByEmptySkinChest(type);
            }
            if (_emptySkinChestLoaded) {
                await _emptySkinChestTcs.Task;
                return _resourceEmptySkinChestSo.GetSpriteByEmptySkinChest(type);
            }
            _emptySkinChestLoaded = true;
            _resourceEmptySkinChestSo =
                await AddressableLoader.LoadAsset<ResourceEmptySkinChestSo>(resourceEmptySkinChestRef);
            _emptySkinChestTcs.TrySetResult();
            return _resourceEmptySkinChestSo.GetSpriteByEmptySkinChest(type);
        }

        public async UniTask<ResourceAnimationPicker> GetAnimationByItemId(int itemId) {
            return await GetAnimation((GachaChestProductId)itemId);
        }

        public async UniTask<ResourceAnimationPicker> GetAnimation(GachaChestProductId type) {
            if (_resourceAnimationSo != null) {
                return _resourceAnimationSo.GetAnimation(type);
            }
            if (_animationLoaded) {
                await _animationTcs.Task;
                return _resourceAnimationSo.GetAnimation(type);
            }
            _animationLoaded = true;
            _resourceAnimationSo =
                await AddressableLoader.LoadAsset<ResourceAnimationSo>(resourceAnimationRef);
            _animationTcs.TrySetResult();
            return _resourceAnimationSo.GetAnimation(type);
        }

        public async UniTask<ResourceAnimationPicker> GetAnimation(GachaRankChange type) {
            if (_resourceAnimationSo != null) {
                return _resourceAnimationSo.GetAnimation(type);
            }
            if (_animationLoaded) {
                await _animationTcs.Task;
                return _resourceAnimationSo.GetAnimation(type);
            }
            _animationLoaded = true;
            _resourceAnimationSo =
                await AddressableLoader.LoadAsset<ResourceAnimationSo>(resourceAnimationRef);
            _animationTcs.TrySetResult();
            return _resourceAnimationSo.GetAnimation(type);
        }

        public async UniTask<string> GetDescription(int itemId) {
            if (_resourceDescriptionSo != null) {
                return _resourceDescriptionSo.GetDescription(itemId);
            }
            if (_descriptionLoaded) {
                await _descriptionTcs.Task;
                return _resourceDescriptionSo.GetDescription(itemId);
            }
            _descriptionLoaded = true;
            _resourceDescriptionSo =
                await AddressableLoader.LoadAsset<ResourceDescriptionSo>(resourceDescriptionRef);
            _descriptionTcs.TrySetResult();
            return _resourceDescriptionSo.GetDescription(itemId);
        }

        public async UniTask<Sprite[]> GetAvatar(int avatarId) {
            if (_resourceAvatarSo != null) {
                return _resourceAvatarSo.GetAvatar(avatarId);    
            }
            if (_avatarLoaded) {
                await _avatarTcs.Task;
                return _resourceAvatarSo.GetAvatar(avatarId);
            }
            _avatarLoaded = true;
            _resourceAvatarSo =
                await AddressableLoader.LoadAsset<ResourceAvatarSo>(resourceAvatarRef);
            _avatarTcs.TrySetResult();
            return _resourceAvatarSo.GetAvatar(avatarId);
        }
    }
}