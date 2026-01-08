using Engine.Entities;

using StoryMode.Entities;

using UnityEngine;

namespace Engine.Components {
    public class AdventureDropper : Dropper {
        [SerializeField]
        private AdventureGhostDie adventureGhost;

        private PlayerType _playerType;
        private PlayerColor _playerColor;
        
        public override void SetHeroSprite(PlayerType charName, PlayerColor colorName) {
            _playerType = charName;
            _playerColor = colorName;
        }

        protected override void CreateDieAnimationGhost() {
            var ghost = Instantiate(adventureGhost, transform.parent);
            ghost.InitForDie(_playerType, _playerColor);

            var ghostTransform = ghost.transform;
            ghostTransform.localPosition = transform.localPosition;
            
            var entity = GetComponent<Entity>();
            entity.EntityManager.AddEntity(ghost);
        }
    }
}