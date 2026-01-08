using Engine.Utils;

using UnityEngine;

namespace Engine.Components {
    public class TestAnimationPet : MonoBehaviour {
        public HasAnimation hasAnimation;
        public SpriteRenderer body;
        public SerializableDictionary<FaceDirection, Sprite> dicSpriteChar;
        public SerializableDictionary<FaceDirection, ImageAnimation> dicAniPet;

        protected void Awake() {
        }

        protected void LateUpdate() {
            body.sprite = dicSpriteChar[hasAnimation.CurrentFace];
            var aniPet = dicAniPet[hasAnimation.CurrentFace];
            foreach (var it in dicAniPet) {
                it.Value.gameObject.SetActive(aniPet == it.Value);
            }
            switch (hasAnimation.CurrentFace) {
                case FaceDirection.Down:
                    break;
                case FaceDirection.Up:
                    break;
                case FaceDirection.Left:
                    body.transform.localRotation = Quaternion.Euler(0, 180, 0);
                    aniPet.transform.localRotation = Quaternion.Euler(0, 180, 0);
                    break;
                case FaceDirection.Right:
                    body.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    aniPet.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    break;
            }
        }
    }
}