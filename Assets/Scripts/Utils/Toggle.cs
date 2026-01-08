using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Utils {
    [RequireComponent(typeof(UnityEngine.UI.Toggle))]
    public class Toggle : MonoBehaviour {
        [FormerlySerializedAs("_event")]
        [SerializeField]
        private UnityEvent @event;

        private void Awake() {
            var toggle = GetComponent<UnityEngine.UI.Toggle>();
            toggle.onValueChanged.AddListener(isOn => {
                if (isOn) {
                    @event.Invoke();
                }
            });
        }
    }
}