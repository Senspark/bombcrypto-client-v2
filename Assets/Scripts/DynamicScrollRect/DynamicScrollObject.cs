using System;

using pooling;

using UnityEngine;

namespace Game.Dialog {
    public abstract class DynamicScrollObject<T> : PoolingObject, IScrollItem, IComparable {
        private Action _refreshListAction;
        private RectTransform _rectTransform;

        public virtual float CurrentHeight {
            get => RectTransform.sizeDelta.y;
            set => RectTransform.sizeDelta = new Vector2(RectTransform.sizeDelta.x, value);
        }

        public virtual float CurrentWidth {
            get => RectTransform.sizeDelta.x;
            set => RectTransform.sizeDelta = new Vector2(value, RectTransform.sizeDelta.y);
        }

        public virtual int CurrentIndex { get; set; }
        //public bool IsCentralized { get; private set; }
        //public Vector2 PositionInViewport { get; private set; }
        //public Vector2 DistanceFromCenter { get; private set; }

        public RectTransform RectTransform {
            get {
                if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }

        public virtual void Reset() {
        }

        public virtual void UpdateScrollObject(T item, int index, int selectId = -1) {
            CurrentIndex = index;
            //OnObjectIsNotCentralized();
        }

        // public virtual void SetRefreshListAction(Action action) {
        //     _refreshListAction = action;
        // }

        // public virtual void SetPositionInViewport(Vector2 position, Vector2 distanceFromCenter) {
        //     PositionInViewport = position;
        //     DistanceFromCenter = distanceFromCenter;
        // }
        //
        // public virtual void OnObjectIsCentralized() {
        //     IsCentralized = true;
        // }
        //
        // public virtual void OnObjectIsNotCentralized() {
        //     IsCentralized = false;
        // }

        public int CompareTo(object obj) {
            if (obj is DynamicScrollObject<T> scrollObject)
                return CurrentIndex.CompareTo(scrollObject.CurrentIndex);

            return -1;
        }
    }
}