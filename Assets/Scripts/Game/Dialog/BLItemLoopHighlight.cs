using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Game.Dialog {
    public class BLItemLoopHighlight : MonoBehaviour {
        [FormerlySerializedAs("_description")]
        [SerializeField]
        private Text description;

        public void UpdateTarget(Transform target, string desc) {
            description.text = desc;
            transform.position = target.position;
        }
    }
}