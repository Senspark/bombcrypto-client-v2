namespace Data {
    public struct CostumeData {
        public struct PriceData {
            public string Package;
            public string RewardType;
            public int Price;
            public long Duration;
        }

        public int ItemId;
        public PriceData[] Prices;
    }
}