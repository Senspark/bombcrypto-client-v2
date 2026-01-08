using System;
using System.Collections.Generic;

using App;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog.BomberLand.BLWallet {
    
    [Serializable]
    public class BLWalletSegment <T> where T: MonoBehaviour{
        [SerializeField]
        public GameObject segment;
        
        [SerializeField]
        public GameObject item;
        
        private readonly List<T> _items = new List<T>();
        
        public void HideContent() {
            segment.SetActive(false);
        }

        public void ShowContent() {
            segment.SetActive(true);   
        }
        
        public List<T> Create(int numItem){
            segment.SetActive(true);
            if (_items.Count < numItem) {
                var numSpawn = numItem - _items.Count;
                for (var idx = 0; idx < numSpawn; idx++) {
                    var obj = UnityEngine.Object.Instantiate(item, segment.transform, false);
                    _items.Add(obj.GetComponent<T>());
                }
            }
            var r = new List<T>();
            for (var idx = 0; idx < _items.Count; idx++) {
                var i = _items[idx];
                if (idx >= numItem) {
                    i.gameObject.SetActive(false);    
                    continue;
                }
                i.gameObject.SetActive(true);
                r.Add(i);
            }
            return r;
        }
    }
    public class BLWalletSegmentContent : MonoBehaviour
    {   
        [SerializeField]
        private BLWalletSegment<BLWalletSegmentItem> segmentToken;
        
        [SerializeField]
        private BLWalletSegment<BLWalletSegmentItem> segmentNft;
        
        [SerializeField]
        public Transform contentSizeFitter;

        [SerializeField]
        private GameObject[] items;
        
        private readonly List<BLWalletSegmentItem> _itemsToken = new List<BLWalletSegmentItem>();
        
        private bool _requestAutoLayout = false;
            
        protected void Awake() {
            
        }

        public void HideUiSegmentToken() {
            segmentToken.HideContent();
        }
        
        public void HideUiSegmentNft() {
            segmentNft.HideContent();
        }

        public List<BLWalletSegmentItem> InitUiTokens(int numItem) {
            if (items is {Length: 2}) {
                var isPad = ScreenUtils.IsIPadScreen();
                if (isPad) {
                    segmentToken.item = items[1];
                }
            }
            segmentToken.ShowContent();
            return segmentToken.Create(numItem);
        }
        
        public List<BLWalletSegmentItem> InitUiNFt(int numItem) {
            if (items is {Length: 2}) {
                var isPad = ScreenUtils.IsIPadScreen();
                if (isPad) {
                    segmentNft.item = items[1];
                }
            }
            segmentNft.ShowContent();
            return segmentNft.Create(numItem);
        }

        public void AutoLayout() {
            _requestAutoLayout = true;
        }

        protected void LateUpdate() {
            if(!_requestAutoLayout) {
                return;
            }
            var tRoot = contentSizeFitter;
            var rtRoot = contentSizeFitter.GetComponent<RectTransform>();
            if (tRoot.childCount <= 0) {
                _requestAutoLayout = false;
                return;
            }
            var hFix = 0.0f;
            var offset = 14.0f;
            if (AppConfig.IsTon()) {
                offset = 0f;
            }
            for (var idx = 0; idx < tRoot.childCount; idx++) {
                var o = tRoot.GetChild(idx);
                if(!o.gameObject.activeSelf) {
                    continue;
                }
                var rt = o.GetComponent<RectTransform>();
                if(rt.sizeDelta.y == 0) {
                    return;
                }
                var s = rt.sizeDelta;
                s = new Vector2(s.x, s.y);
                rt.sizeDelta = s;
                rt.localPosition = new Vector3(rt.localPosition.x, -hFix);
                hFix += s.y;
                hFix += offset;
            }
            rtRoot.sizeDelta = new Vector2(rtRoot.sizeDelta.x, hFix);
            _requestAutoLayout = false;
        }
    }
}
