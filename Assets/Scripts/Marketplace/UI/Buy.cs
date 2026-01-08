using System;

using Senspark;

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Marketplace.UI {
    public class Buy : MonoBehaviour {
        [FormerlySerializedAs("_sort")]
        [SerializeField]
        private Dropdown sort;

        public event Action OnChanged;
        public event Action OnCleared;
        public int Length { get; private set; }
        public int ProductId { get; private set; } = -1;
        public int Sort { get; private set; }

        private ILogManager _logManager;
        private (string Label, int Value)[] _sort;

        private void Awake() {
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            _sort = new[] {
                ("LOWEST TO HIGHEST", 0),
                ("HIGHEST TO LOWEST", 1)
            };
            sort.options.Clear();
            foreach (var (label, _) in _sort) {
                sort.options.Add(new Dropdown.OptionData(label));
            }
            sort.onValueChanged.AddListener(index => {
                Sort = _sort[index].Value;
                Change();
            });
        }

        private void Change() => (OnChanged ?? throw new NullReferenceException(nameof(OnChanged)))();

        public void IncreaseLength(int value) {
            Length += value;
        }

        public void OnSearch(string value) {
            try {
                ProductId = string.IsNullOrEmpty(value) ? -1 : int.Parse(value);
                Change();
            } catch (Exception e) {
                _logManager.Log(e.Message);
                (OnCleared ?? throw new NullReferenceException(nameof(OnCleared)))();
            }
        }

        public void ResetLength(int length) {
            Length = length;
        }
    }
}