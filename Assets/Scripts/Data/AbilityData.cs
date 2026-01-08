namespace Data {
    public class AbilityData {
        // public AbilityData(AbilityId abilityId, int value) : this((int) abilityId, value) {
        // }
        //
        // public AbilityData(int abilityId, int value) {
        //     AbilityId = abilityId;
        //     Value = value;
        // }
        
        public AbilityData(int abilityId) {
            AbilityId = abilityId;
        }

        public int AbilityId { get; }
        // public int Value { get; }
    }
}