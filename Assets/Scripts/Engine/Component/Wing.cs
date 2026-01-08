using Senspark;

using UnityEngine;
using UnityEngine.Serialization;

namespace Engine.Components {
    public class Wing : MonoBehaviour {
        [FormerlySerializedAs("_playerRenderer")]
        [SerializeField]
        private SpriteRenderer playerRenderer;

        [SerializeField]
        private SpriteRenderer _renderer;

        [SerializeField]
        private Sprite[] _sprites;

        [SerializeField]
        private Transform[] _transforms;

        private ILogManager _logManager;

        private void Awake() {
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
        }

        public void SyncMove(FaceDirection face) {
            transform.SetParent(_transforms[(int) face], false);
            _renderer.sortingOrder = playerRenderer.sortingOrder + (face == FaceDirection.Down ? -1 : 1);
            _renderer.sprite = _sprites[(int) face];
        }
    }
}