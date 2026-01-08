using Constant;

using Engine.Components;

using UnityEngine;

namespace Engine.ScriptableObject {
    [CreateAssetMenu(fileName = "BLTrailRes", menuName = "BomberLand/TrailRes")]
    public class BLTrailRes : UnityEngine.ScriptableObject {
        public SerializableDictionary<GachaChestProductId, GameObject> resourceTrail;

        public void AttachTrailEffect(Transform tfTrail, int trail, BlinkEffect blinkEffect) {
            var productId = (GachaChestProductId) trail;
            if (!resourceTrail.ContainsKey(productId)) {
                tfTrail.gameObject.SetActive(false);
                return;
            }
            tfTrail.gameObject.SetActive(true);
            var prefab = resourceTrail[productId];
            var obj = Instantiate(prefab, tfTrail, false);
            switch (productId) {
                case GachaChestProductId.TrailMultiShadow:
                case GachaChestProductId.TrailHappyColor:
                case GachaChestProductId.TrailNitro: {
                    var spriteTrail = obj.GetComponent<SpriteTrail.SpriteTrail>();
                    var trailContainer = new GameObject("TrailContainer").transform;
                    var objSpriteBody = tfTrail.parent;
                    spriteTrail.Init(trailContainer, objSpriteBody.gameObject);
                    if (blinkEffect) {
                        blinkEffect.SetTrailRenderer(spriteTrail.LocalTrailContainer.gameObject);
                    }
                    break;
                }
            }
        }
    }
}