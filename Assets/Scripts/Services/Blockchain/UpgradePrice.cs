using Newtonsoft.Json;

namespace App {
    /// <summary>
    /// Custo de um upgrade no BHeroUpgradeV2, o contrato de teste dos níveis 6 a 10.
    ///
    /// Três moedas em vez de uma: BCOIN e SEN são ERC20 e vêm zerados nos níveis legados (2 a 5),
    /// onde só o nativo é cobrado. Todos os valores são wei em string — nunca converter para
    /// double antes de assinar, porque o contrato exige <c>msg.value</c> exato e não devolve troco.
    /// </summary>
    public class UpgradePrice {
        [JsonProperty("bcoin")]
        public string Bcoin = "0";

        [JsonProperty("sen")]
        public string Sen = "0";

        [JsonProperty("native")]
        public string Native = "0";

        public static UpgradePrice Zero => new UpgradePrice();

        public bool HasTokenCost => !IsZero(Bcoin) || !IsZero(Sen);

        public static bool IsZero(string wei) {
            if (string.IsNullOrEmpty(wei)) {
                return true;
            }
            foreach (var c in wei) {
                if (c != '0') {
                    return false;
                }
            }
            return true;
        }
    }
}
