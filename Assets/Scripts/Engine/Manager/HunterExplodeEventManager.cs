namespace Engine.Manager {
    // Treasure mode: the server decides every explosion (TREASURE_EVENTS "EXPLODE"), so a local
    // explode is visual only. This class exists only to name the mode.
    public class HunterExplodeEventManager : DefaultExplodeEventManager {
        public HunterExplodeEventManager(IEntityManager entityManager) : base(entityManager) {
        }
    }
}
