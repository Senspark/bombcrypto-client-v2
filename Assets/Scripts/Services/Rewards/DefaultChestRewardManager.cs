using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.UI;
using Senspark;

namespace App {
    public class DefaultChestRewardManager : ObserverManager<ChestRewardManagerObserver>, IChestRewardManager {
        private sealed class Balance {
            public float Value;
            public float Pending;
        }

        // token wire-name -> scope (DataType) -> balance
        private readonly Dictionary<string, Dictionary<DataType, Balance>> _store = new();

        private DataType _currentDataType = DataType.TR;
        private HashSet<DataType> _visibleScopes = VisibleScopes(DataType.TR);

        public DefaultChestRewardManager() {
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public void SetCurrentNetwork(DataType dataType) {
            _currentDataType = dataType;
            _visibleScopes = VisibleScopes(dataType);
        }

        // Scope nào được chấp nhận/hiển thị với network hiện tại (game owner confirmed).
        // BSC/POLYGON thấy cả hai (cross-EVM) + TR; các network đơn chỉ thấy chính nó; VIC bị loại.
        private static HashSet<DataType> VisibleScopes(DataType current) {
            return current switch {
                DataType.BSC or DataType.POLYGON => new HashSet<DataType> { DataType.BSC, DataType.POLYGON, DataType.TR, DataType.BP },
                DataType.TR => new HashSet<DataType> { DataType.TR },
                DataType.TON => new HashSet<DataType> { DataType.TON },
                DataType.SOL => new HashSet<DataType> { DataType.SOL },
                DataType.RON => new HashSet<DataType> { DataType.RON },
                DataType.BAS => new HashSet<DataType> { DataType.BAS },
                _ => new HashSet<DataType>(),
            };
        }

        private static bool TryParseScope(string dataType, out DataType scope) {
            scope = DataType.TR;
            return !string.IsNullOrEmpty(dataType) && Enum.TryParse(dataType, out scope);
        }

        // Số dư "hiện tại" (không chỉ định scope): scope của network hiện tại, fallback TR
        // (cho off-chain COIN/GEM/GOLD/ROCK vốn chỉ tồn tại ở TR).
        private Balance GetCurrent(string token) {
            if (!_store.TryGetValue(token, out var scopes)) {
                return null;
            }
            if (scopes.TryGetValue(_currentDataType, out var cur)) {
                return cur;
            }
            return scopes.TryGetValue(DataType.TR, out var tr) ? tr : null;
        }

        private float ScopeValue(string token, DataType scope) {
            return _store.TryGetValue(token, out var scopes) && scopes.TryGetValue(scope, out var b) ? b.Value : 0;
        }

        private float ScopePending(string token, DataType scope) {
            return _store.TryGetValue(token, out var scopes) && scopes.TryGetValue(scope, out var b) ? b.Pending : 0;
        }

        private Balance Ensure(string token, DataType scope) {
            if (!_store.TryGetValue(token, out var scopes)) {
                scopes = new Dictionary<DataType, Balance>();
                _store[token] = scopes;
            }
            if (!scopes.TryGetValue(scope, out var b)) {
                b = new Balance();
                scopes[scope] = b;
            }
            return b;
        }

        #region INIT

        public void InitNewChestReward(IChestReward rewards) {
            _store.Clear();

            foreach (var e in rewards.Rewards) {
                // Filter-at-receive: scope lạ hoặc không visible (gồm VIC) -> bỏ, không throw.
                if (!TryParseScope(e.Network, out var scope)) {
                    continue;
                }
                if (!_visibleScopes.Contains(scope)) {
                    continue;
                }

                var balance = Ensure(e.Type.Name, scope);
                balance.Value = e.Value;
                balance.Pending = e.ClaimPending;

                DispatchEvent(d => d.OnRewardBalanceChanged?.Invoke(e.Type.Type, scope, e.Value));
            }
        }

        #endregion

        #region GET REWARD

        public float GetChestRewardByNetwork(IRewardType type, DataType network) {
            return ScopeValue(type.Name, network);
        }

        private float GetChestReward(string type) {
            return GetCurrent(type)?.Value ?? 0;
        }

        public float GetChestReward(BlockRewardType type) {
            if (type == BlockRewardType.Other) {
                return 0;
            }
            var t = RewardUtils.ConvertToBlockRewardType(type);
            return GetChestReward(t);
        }

        public float GetChestReward(BlockRewardType type, DataType dataType) {
            if (type == BlockRewardType.Other) {
                return 0;
            }
            var t = RewardUtils.ConvertToBlockRewardType(type);
            return ScopeValue(t, dataType);
        }

        public float GetChestReward(IRewardType type) {
            var t = RewardUtils.ConvertToBlockRewardType(type.Name);
            return GetChestReward(t);
        }

        public float GetBcoinRewardAndDeposit() {
            return GetChestReward(BlockRewardType.BCoin) + GetChestReward(BlockRewardType.BCoinDeposited);
        }

        public float GetSenRewardAndDeposit() {
            return GetChestReward(BlockRewardType.Senspark) + GetChestReward(BlockRewardType.SensparkDeposited);
        }

        public float GetBcoinRewardAndDeposit(DataType network) {
            return GetChestReward(BlockRewardType.BCoin, network) + GetChestReward(BlockRewardType.BCoinDeposited, network);
        }

        public float GetSenRewardAndDeposit(DataType network) {
            return GetChestReward(BlockRewardType.Senspark, network) + GetChestReward(BlockRewardType.SensparkDeposited, network);
        }

        public float GetRock() {
            return GetChestReward(BlockRewardType.Rock);
        }

        // Bridge balance sống ở scope BP (chain-agnostic). Hook cho spending phase sau — Wallet UI đọc thẳng
        // rewards array, không qua getter này. Generic GetChestReward(Bcoin/SenBridge) sẽ trả 0 (không có scope current/TR).
        public float GetBcoinBridge() {
            return ScopeValue(RewardUtils.ConvertToBlockRewardType(BlockRewardType.BcoinBridge), DataType.BP);
        }

        public float GetSenBridge() {
            return ScopeValue(RewardUtils.ConvertToBlockRewardType(BlockRewardType.SenBridge), DataType.BP);
        }

        #endregion

        #region SET REWARD

        private void SetChestReward(string type, float value) {
            Ensure(type, _currentDataType).Value = value;
        }

        public void SetChestReward(BlockRewardType type, float value) {
            if (type == BlockRewardType.Other) {
                return;
            }
            var n = RewardUtils.ConvertToBlockRewardType(type);
            SetChestReward(n, value);
            DispatchEvent(e => e.OnRewardBalanceChanged?.Invoke(type, _currentDataType, value));
        }

        #endregion

        #region ADJUST REWARD

        private float AdjustChestReward(string type, float addValue) {
            var b = Ensure(type, _currentDataType);
            b.Value += addValue;
            return b.Value;
        }

        public float AdjustChestReward(IRewardType type, float addValue) {
            var result = AdjustChestReward(type.Name, addValue);
            DispatchEvent(e => e.OnRewardBalanceChanged?.Invoke(type.Type, _currentDataType, result));
            return result;
        }

        public float AdjustChestReward(BlockRewardType type, float addValue) {
            if (type == BlockRewardType.Other) {
                return 0;
            }
            var name = RewardUtils.ConvertToBlockRewardType(type);
            var result = AdjustChestReward(name, addValue);
            DispatchEvent(e => e.OnRewardBalanceChanged?.Invoke(type, _currentDataType, result));
            return result;
        }

        #endregion

        #region GET CLAIM PENDING

        public float GetClaimPendingRewardByNetwork(IRewardType type, DataType network) {
            return ScopePending(type.Name, network);
        }

        #endregion
    }
}
