using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Utils {
    [RequireComponent(typeof(UnityEngine.UI.ScrollRect))]
    public class UnityScrollRect : MonoBehaviour {
        [FormerlySerializedAs("_onXChanged")]
        [SerializeField]
        private UnityEvent<float> onXChanged;

        [FormerlySerializedAs("_onYChanged")]
        [SerializeField]
        private UnityEvent<float> onYChanged;

        private float _x;
        private float _y;

        private void Awake() {
            GetComponent<UnityEngine.UI.ScrollRect>().onValueChanged.AddListener(value => {
                if (!Mathf.Approximately(_x, value.x)) {
                    _x = value.x;
                    onXChanged.Invoke(_x);
                }
                if (!Mathf.Approximately(_y, value.y)) {
                    _y = value.y;
                    onYChanged.Invoke(_y);
                }
            });
        }
    }
}