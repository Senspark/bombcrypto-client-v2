namespace Senspark.Security {
    public class SignKey {
        public readonly string CypherText;
        public readonly string Key;
        public readonly string Vector;

        public SignKey(string cypherText, string key, string vector) {
            CypherText = cypherText;
            Key = key;
            Vector = vector;
        }
    }
}