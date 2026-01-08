using System.Collections.Generic;

using Engine.Entities;

using UnityEngine;

namespace BomberLand.Component {
    public class EnemyAvatarControl : MonoBehaviour {
        [SerializeField]
        private Transform container;

        [SerializeField]
        private EnemyAvatar enemyPrefab;

        private readonly Dictionary<EnemyType, EnemyAvatar> _enemyAvatars = new();

        public void AddEnemy(EnemyType enemyType) {
            if (_enemyAvatars.ContainsKey(enemyType)) {
                _enemyAvatars[enemyType].IncreaseQuantity();
            } else {
                var avatar = Instantiate(enemyPrefab, container, false);
                avatar.ChangeImage(enemyType);
                avatar.UpdateQuantity(1);
                _enemyAvatars[enemyType] = avatar;
            }
        }

        public void RemoveEnemy(EnemyType enemyType) {
            if (_enemyAvatars.ContainsKey(enemyType)) {
                _enemyAvatars[enemyType].DecreaseQuantity();
            }
        }
    }
}