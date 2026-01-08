using Constant;

using Game.Dialog.BomberLand.BLGacha;

using UnityEngine;
[CreateAssetMenu(fileName = "Resource Avatar", menuName = "BomberLand/GachaRes/Resource Avatar")]
public class ResourceAvatarSo : ScriptableObject
{
    public SerializableDictionaryEnumKey<GachaChestProductId, BLGachaRes.ResourceAnimationPicker> resourceAvatarAnimation;

    public Sprite[] GetAvatar(int avatarId) {
        var productId = (GachaChestProductId)avatarId;
        return resourceAvatarAnimation.TryGetValue(productId, out var value)
            ? value.AnimationIdle
            : resourceAvatarAnimation[GachaChestProductId.Unknown].AnimationIdle;
    }
}
