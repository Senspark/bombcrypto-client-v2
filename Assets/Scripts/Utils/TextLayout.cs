using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Utils {
    public class TextLayout : MonoBehaviour {
        [FormerlySerializedAs("_childAlignment")]
        [SerializeField]
        private TextAnchor childAlignment;

        [FormerlySerializedAs("_childControlHeight")]
        [SerializeField]
        private bool childControlHeight;
        
        [FormerlySerializedAs("_childControlWidth")]
        [SerializeField]
        private bool childControlWidth;

        [FormerlySerializedAs("_childForceExpandHeight")]
        [SerializeField]
        private bool childForceExpandHeight;
        
        [FormerlySerializedAs("_childForceExpandWidth")]
        [SerializeField]
        private bool childForceExpandWidth;

        [FormerlySerializedAs("_spacing")]
        [SerializeField]
        private float spacing;

        [FormerlySerializedAs("_text")]
        [SerializeField]
        private RectTransform text;

        private float _size;
        private HorizontalLayoutGroup _layout;

        private void Update() {
            if (Mathf.Approximately(_size, text.rect.width)) {
                return;
            }
            if (_layout) {
                Destroy(_layout);
            }
            _layout = gameObject.AddComponent<HorizontalLayoutGroup>();
            _size = text.rect.width;
            _layout.childAlignment = childAlignment;
            _layout.childControlHeight = childControlHeight;
            _layout.childControlWidth = childControlWidth;
            _layout.childForceExpandHeight = childForceExpandHeight;
            _layout.childForceExpandWidth = childForceExpandWidth;
            _layout.spacing = spacing;
        }
    }
}