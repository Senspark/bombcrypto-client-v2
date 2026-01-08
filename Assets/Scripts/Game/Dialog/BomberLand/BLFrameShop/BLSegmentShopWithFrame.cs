using UnityEngine;

namespace Game.Dialog.BomberLand.BLFrameShop {
    public class BLSegmentShopWithFrame : BLSegmentShop {
        
        [SerializeField]
        public GameObject prefabFrameContent;
        
        [SerializeField]
        protected GameObject frame = null;
        public GameObject Frame => frame;
        
        protected override void Awake() {
            base.Awake();
            if (!frame && prefabFrameContent) {
                frame = Instantiate(prefabFrameContent, frameContent.transform);    
            }
        }
        
    }
}