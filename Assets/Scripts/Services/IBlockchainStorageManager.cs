using System;
using System.Threading.Tasks;

using Senspark;

namespace App {
    [Service(nameof(IBlockchainStorageManager))]
    public interface IBlockchainStorageManager : IService, IObserverManager<BlockchainStorageManagerObserver> {
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SetBalance(ObserverCurrencyType category, double value);

        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SetBalance(RpcTokenCategory category, double value);

        public double GetBalance(RpcTokenCategory category);

        /// <summary>
        /// Trả về balance theo current Network
        /// </summary>
        public double GetBalance(BlockRewardType category);

        /// <summary>
        /// Trả về balance theo current Network
        /// </summary>
        public double GetBalance(ObserverCurrencyType category);
    }
    
    public class BlockchainStorageManagerObserver {
        public Action<ObserverCurrencyType, double> OnCurrencyChanged;
    }

    public class DefaultBlockchainStorageManager : 
        ObserverManager<BlockchainStorageManagerObserver>,
        IBlockchainStorageManager {
        private readonly NetworkType _networkType;
        private readonly double[] _rpcTokens = new double[Enum.GetNames(typeof(RpcTokenCategory)).Length];
        private double _rock;

        public DefaultBlockchainStorageManager(NetworkType networkType) {
            _networkType = networkType;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SetBalance(ObserverCurrencyType category, double value) {
            if (category == ObserverCurrencyType.Rock) {
                _rock = value;
                DispatchEvent(e => e.OnCurrencyChanged?.Invoke(ObserverCurrencyType.Rock, value));
                return;
            }
            var t = category switch {
                ObserverCurrencyType.WalletBCoin => RpcTokenCategory.Bcoin,
                ObserverCurrencyType.WalletBomb => RpcTokenCategory.Bomb,
                ObserverCurrencyType.WalletSenBsc => RpcTokenCategory.SenBsc,
                ObserverCurrencyType.WalletSenPolygon => RpcTokenCategory.SenPolygon,
                _ => throw new ArgumentOutOfRangeException(nameof(category))
            };
            SetBalance(t, value);
        }

        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SetBalance(RpcTokenCategory category, double value) {
            _rpcTokens[(int) category] = value;
            var t = ConvertTo(category);
            DispatchEvent(e => e.OnCurrencyChanged?.Invoke(t, value));
        }

        public double GetBalance(RpcTokenCategory category) {
            return _rpcTokens[(int) category];
        }

        public double GetBalance(BlockRewardType category) {
            var t = (category, _networkType) switch {
                (BlockRewardType.BCoin, NetworkType.Binance) => RpcTokenCategory.Bcoin,
                (BlockRewardType.BCoin, NetworkType.Polygon) => RpcTokenCategory.Bomb,
                (BlockRewardType.Senspark, NetworkType.Binance) => RpcTokenCategory.SenBsc,
                (BlockRewardType.Senspark, NetworkType.Polygon) => RpcTokenCategory.SenPolygon,
                _ => throw new ArgumentOutOfRangeException(nameof(category))
            };
            return GetBalance(t);
        }

        public double GetBalance(ObserverCurrencyType category) {
            if (category == ObserverCurrencyType.Rock) {
                return _rock;
            }
            var t = category switch {
                ObserverCurrencyType.WalletBCoin => RpcTokenCategory.Bcoin,
                ObserverCurrencyType.WalletBomb => RpcTokenCategory.Bomb,
                ObserverCurrencyType.WalletSenBsc => RpcTokenCategory.SenBsc,
                ObserverCurrencyType.WalletSenPolygon => RpcTokenCategory.SenPolygon,
                _ => throw new ArgumentOutOfRangeException(nameof(category))
            };
            return GetBalance(t);
        }

        private static ObserverCurrencyType ConvertTo(RpcTokenCategory category) {
            return category switch {
                RpcTokenCategory.Bcoin => ObserverCurrencyType.WalletBCoin,
                RpcTokenCategory.Bomb => ObserverCurrencyType.WalletBomb,
                RpcTokenCategory.SenBsc => ObserverCurrencyType.WalletSenBsc,
                RpcTokenCategory.SenPolygon => ObserverCurrencyType.WalletSenPolygon,
                _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
            };
        }
    }
}