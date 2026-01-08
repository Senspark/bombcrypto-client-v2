namespace Data {
    public class InventoryChestItemData {
        public string ItemDescription { get; }
        public int ItemId { get; }
        public string ItemName { get; }

        public InventoryChestItemData(
            string description,
            int itemId,
            string name
        ) {
            ItemDescription = description;
            ItemId = itemId;
            ItemName = name;
        }
    }
}