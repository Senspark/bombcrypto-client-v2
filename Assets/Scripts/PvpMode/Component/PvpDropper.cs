using Actions;

using Engine.Entities;

using PvpMode.Entities;

using UnityEngine;

namespace Engine.Components {
    public class PvpDropper : Dropper {

        [SerializeField]
        private PvpGhostDie pvpGhost;

        protected override void CreateDieAnimationGhost() {
            var ghost = Instantiate(pvpGhost, transform.parent);
            ghost.InitForDie(GetComponent<PlayerPvp>().Faction == FactionType.Ally);

            var ghostTransform = ghost.transform;
            ghostTransform.localPosition = transform.localPosition;

            var entity = GetComponent<Entity>();
            entity.EntityManager.AddEntity(ghost);
        }
    }
}