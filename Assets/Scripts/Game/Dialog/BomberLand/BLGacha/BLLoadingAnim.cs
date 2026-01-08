using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Dialog.BomberLand.BLGacha {
    [CreateAssetMenu(fileName = "BLLoadingAnim", menuName = "BomberLand/LoadingAnim")]
    public class BLLoadingAnim : ScriptableObject {
        [Serializable]
        public class LoadingAnim {
            public int animIndex;
            public Sprite[] sprites;
        }

        public LoadingAnim errorAnim;
        public List<LoadingAnim> animations;

        public LoadingAnim GetRandomAnim() {
            if (animations.Count == 0) {
                return errorAnim;
            }
            
            var randomIndex = Random.Range(0, animations.Count);
            var randomAnim = animations[randomIndex];
            return randomAnim;
        }
    }
}