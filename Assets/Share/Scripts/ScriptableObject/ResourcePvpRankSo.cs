using System.Collections;
using System.Collections.Generic;

using Game.Dialog.BomberLand.BLGacha;

using Services;

using UnityEngine;
[CreateAssetMenu(fileName = "Resource Pvp Rank", menuName = "BomberLand/GachaRes/Resource Pvp Rank")]
public class ResourcePvpRankSo : ScriptableObject
{
    public SerializableDictionaryEnumKey<PvpRankType, BLGachaRes.ResourcePicker> resourcePvpRank;

    public Sprite GetSpriteByPvpRank(PvpRankType pvpRankType) {
        if (resourcePvpRank.TryGetValue(pvpRankType, out var value)) {
            var s = value.sprite;
            if (s != null) {
                return s;
            }
        }
        return resourcePvpRank[PvpRankType.Unknown].sprite;
    }
}
