namespace Data {
    public class BoosterData {
        public int BoosterId { get; }
        public string Description { get; }
        public int Quantity { get; }
        public float TimeEffect { get; }
        public float CoolDown { get; }

        public BoosterData(
            int boosterId,
            float cooldown,
            string description,
            float duration,
            int quantity
        ) {
            BoosterId = boosterId;
            Description = description;
            Quantity = quantity;
            TimeEffect = duration;
            CoolDown = cooldown;
        }
    }
}