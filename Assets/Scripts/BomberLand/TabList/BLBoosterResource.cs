using System;

using Cysharp.Threading.Tasks;

using Game.Dialog.BomberLand.BLGacha;

using PvpMode.Manager;

using UnityEngine;

namespace Game.UI {
    public class BLBoosterResource : MonoBehaviour {
        
        [Serializable]
        public class SkinPicker {
            public Sprite sprite;
        }
        
        [SerializeField]
        private SerializableDictionaryEnumKey<BoosterType, SkinPicker> resourceSkin;

        [SerializeField]
        private BLGachaRes gachaRes;
        
        public async UniTask<Sprite> GetSprite(int itemId) {
            var boosterType = ConvertIdToBoosterEnum(itemId);
            if (gachaRes && boosterType == BoosterType.Unknown) {
                return  await gachaRes.GetSpriteByItemId(itemId);
            }
            return GetSprite(boosterType);
        }

        public Sprite GetSprite(BoosterType boosterType) {
            return resourceSkin[boosterType].sprite;
        }

        private BoosterType ConvertIdToBoosterEnum(int itemId) {
            return itemId switch {
                18 => BoosterType.Key,
                19 => BoosterType.Shield,
                20 => BoosterType.RankGuardian,
                21 => BoosterType.FullRankGuardian,
                22 => BoosterType.CupBonus,
                23 => BoosterType.FullCupBonus,
                26 => BoosterType.BombAddOne,
                27 => BoosterType.SpeedAddOne,
                28 => BoosterType.RangeAddOne,
                _ => BoosterType.Unknown
            };
        }
    }
}