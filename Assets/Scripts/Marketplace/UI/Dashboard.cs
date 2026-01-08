using System;

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Marketplace.UI {
    public class Dashboard : MonoBehaviour {
        [FormerlySerializedAs("_status")]
        [SerializeField]
        private Dropdown status;

        public event Action OnChanged;
        public int Status { get; private set; }
        private (string Label, int Value)[] _filter;

        private void Awake() {
            _filter = new[] {
                ("ALL", -1),
                ("SELLING", 2),
                ("OWNING", 0)
            };
            Status = _filter[0].Value;
            status.options.Clear();
            foreach (var (label, _) in _filter) {
                status.options.Add(new Dropdown.OptionData(label));
            }
            status.onValueChanged.AddListener(index => {
                Status = _filter[index].Value;
                (OnChanged ?? throw new NullReferenceException(nameof(OnChanged)))();
            });
        }
    }
}