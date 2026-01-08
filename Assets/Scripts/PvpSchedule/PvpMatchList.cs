using System;
using System.Collections.Generic;
using System.Linq;

using PvpSchedule.Models;

using UnityEngine;
using UnityEngine.UI;

namespace PvpSchedule {
    public class PvpMatchList : MonoBehaviour {
        [SerializeField]
        private PvpMatchItem itemPrefab;

        [SerializeField]
        private ScrollRect scrollRect;

        private List<PvpMatchItem> _items;
        private IPvpMatchSchedule[] _models;
        private Func<IPvpMatchSchedule, bool>[] _filters;
        private Action<IPvpMatchSchedule> _joinCallback;
        private Action<IPvpMatchSchedule> _spectateCallback;
        public List<string> MyMatch { get; set; }

        public IPvpMatchSchedule[] Models {
            get => _models;
            set {
                _models = value;
                UpdateModels();
            }
        }
        
        public Func<IPvpMatchSchedule, bool>[] Filters {
            get => _filters;
            set {
                _filters = value;
                UpdateModels();
            }
        }

        public void SetCallback(Action<IPvpMatchSchedule> joinCallback, Action<IPvpMatchSchedule> spectateCallback) {
            _joinCallback = joinCallback;
            _spectateCallback = spectateCallback;
        }
        
        private void Awake() {
            _items = new List<PvpMatchItem>();
        }

        private void UpdateModels() {
            if (_models == null) {
                return;
            }
            
            var filters = _filters ?? Array.Empty<Func<IPvpMatchSchedule, bool>>();
            var models = _models;
            foreach (var filter in filters) {
                models = models.Where(filter).ToArray();
            }

            for (var i = models.Length; i < _items.Count; ++i) {
                _items[i].gameObject.SetActive(false);
            }
            for (var i = _items.Count; i < models.Length; ++i) {
                var item = Instantiate(itemPrefab, scrollRect.content);
                _items.Add(item);
            }
            for (var i = 0; i < models.Length; ++i) {
                var item = _items[i];
                var model = models[i];
                item.gameObject.SetActive(true);
                item.SetInfo(model,
                    () => JoinPvPQueue(model),
                    () => ShowPvPMatch(model),
                    MyMatch);
            }
        }

        private void JoinPvPQueue(IPvpMatchSchedule model) {
            _joinCallback.Invoke(model);
        }

        private void ShowPvPMatch(IPvpMatchSchedule model) {
            _spectateCallback.Invoke(model);
        }
    }
}