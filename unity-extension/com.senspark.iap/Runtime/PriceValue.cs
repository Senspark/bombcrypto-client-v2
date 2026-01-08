namespace Senspark.Iap {
    public readonly struct PriceValue {
        public readonly float Price;
        public readonly string Currency;
        public PriceValue(float price, string currency) {
            Price = price;
            Currency = currency;
        }

        public static PriceValue Empty = new(0, string.Empty);
    }
}