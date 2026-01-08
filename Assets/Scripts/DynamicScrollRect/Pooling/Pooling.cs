using System.Collections.Generic;

using DynamicScrollRect;

using Game.Dialog;

using UnityEngine;

namespace pooling {
    public class Pooling<T> : List<T> where T : MonoBehaviour, IPooling {
        public bool CreateMoreIfNeeded = true;

        private Transform _parent;
        private Vector3 _startPos;
        private GameObject _referenceObject;
        private Transform _newContainer;

        public delegate void ObjectCreationCallback(T obj);

        public event ObjectCreationCallback OnObjectCreationCallBack;

        public Pooling<T> Initialize(int amount, GameObject refObject, Transform parent, bool startState = false) {
            return Initialize(amount, refObject, parent, Vector3.zero, startState);
        }

        private Pooling<T> Initialize(int amount, GameObject refObject, Transform parent, Vector3 worldPos,
            bool startState = false) {
            _parent = parent;
            _startPos = worldPos;
            _referenceObject = refObject;

            Clear();

            for (var i = 0; i < amount; i++) {
                var obj = CreateObject();

                if (startState) obj.OnCollect();
                else obj.OnRelease();

                Add(obj);
            }

            return this;
        }

        public Pooling<T> InitializeGrid(int amount, Transform newContainer, Transform parent,
            bool startState = false) {
            return InitializeGrid(amount, newContainer, parent, Vector3.zero, startState);
        }

        private Pooling<T> InitializeGrid(int amount, Transform newContainer, Transform parent, Vector3 worldPos,
            bool startState = false) {
            _parent = parent;
            _startPos = worldPos;
            _newContainer = newContainer;

            Clear();

            for (var i = 0; i < amount; i++) {
                var obj = CreateGridObject();

                if (startState) obj.OnCollect();
                else obj.OnRelease();

                Add(obj);
            }

            return this;
        }

        public T Collect(Transform parent = null, Vector3? position = null, bool localPosition = true) {
            var obj = Find(x => x.IsUsing == false);
            if (obj == null && CreateMoreIfNeeded) {
                obj = _referenceObject ? CreateObject() : CreateGridObject();
                Add(obj);
            }

            if (obj == null) return obj;

            obj.transform.SetParent(parent ? parent : _parent);
            if (localPosition)
                obj.transform.localPosition = position ?? _startPos;
            else
                obj.transform.position = position ?? _startPos;
            obj.OnCollect();

            return obj;
        }

        public void Release(T obj) {
            if (obj != null)
                obj.OnRelease();
        }

        public List<T> GetAllWithState(bool active) {
            return FindAll(x => x.IsUsing == active);
        }

        private T CreateObject(Transform parent = null, Vector3? position = null) {
            var go = Object.Instantiate(_referenceObject, position ?? _startPos, Quaternion.identity,
                parent ? parent : _parent);
            var obj = go.GetComponent<T>() ?? go.AddComponent<T>();
            obj.transform.localPosition = position ?? _startPos;
            obj.name = obj.ObjectName + Count;

            OnObjectCreationCallBack?.Invoke(obj);

            return obj;
        }

        private T CreateGridObject(Transform parent = null, Vector3? position = null) {
            var go = _newContainer.GetComponentsInChildren<DynamicInventoryItem>()[0];
            var obj = go.GetComponent<T>() ?? go.gameObject.AddComponent<T>();
            obj.transform.localPosition = position ?? _startPos;
            obj.name = obj.ObjectName + Count;

            if (obj is DynamicObject dynamicObject) {
                go.transform.SetParent(_parent);
                dynamicObject.SetDynamicItem(go);
            }

            OnObjectCreationCallBack?.Invoke(obj);

            return obj;
        }

        public void ClearPool() {
            Clear();
            foreach (Transform child in _parent) {
                Object.Destroy(child.gameObject);
            }
        }
    }
}