using Data;

namespace Game.UI {
    public interface IUiCrystalData {
        int ItemId { get; }
        int Quantity { get; }
        int TargetItemID { get; }
        int GoldFee { get; }
        int GemFee { get; }
    }

    public class UiCrystalData : IUiCrystalData {
        public int ItemId { get; }
        public int Quantity { get; }
        public int TargetItemID { get; }
        public int GoldFee { get; }
        public int GemFee { get; }

        public UiCrystalData(CrystalData data, ConfigUpgradeCrystalData config) {
            ItemId = data.ItemId;
            Quantity = data.Quantity;
            TargetItemID = config.TargetItemID;
            GoldFee = config.GoldFee;
            GemFee = config.GemFee;
        }
    }
}