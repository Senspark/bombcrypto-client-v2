using System.Collections.Generic;

using Engine.Entities;
using UnityEngine;

namespace Engine.Components {
    public class Avatar : MonoBehaviour {
        [SerializeField]
        private SpriteRenderer playerRenderer;

        [SerializeField]
        private SpriteRenderer sprRenderer;

        [SerializeField]
        private Transform[] transforms;

        private int _avatarId;

        private readonly List<PlayerType> _floatPlayer = new List<PlayerType>() {
            PlayerType.Cupid
        };

        private readonly Dictionary<FaceDirection, string> _states = new() {
            [FaceDirection.Down] = "Front",
            [FaceDirection.Up] = "Front",
            [FaceDirection.Left] = "Right",
            [FaceDirection.Right] = "Right"
        };

        public void Initialize(int avatarId, PlayerType type) {
            _avatarId = avatarId;
            gameObject.SetActive(avatarId != 0);

            //Fix me: nâng đáy cánh ngang với đáy chân.
            if (!_floatPlayer.Contains(type)) {
                return;
            }
            var parent = transforms[0].parent; 
            var position = parent.localPosition;
            position.y += 0.25f;
            parent.localPosition = position;
        }

        public void SyncMove(FaceDirection face) {
            if (!gameObject.activeSelf) {
                return;
            }
            transform.SetParent(transforms[(int) face], false);
            sprRenderer.sortingOrder = playerRenderer.sortingOrder + (face == FaceDirection.Down ? -1 : 1);
        }
    }
}