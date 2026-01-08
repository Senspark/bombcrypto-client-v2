using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Senspark;
using Server.Models;
using Services.Rewards;
using Sfs2X.Entities.Data;

namespace App {
    public class DefaultChestRewardManager : ObserverManager<ChestRewardManagerObserver>, IChestRewardManager {
        /*
         * FLOW
         * Hiện tại có 3 loại data_type (Network) để lưu trữ: TR, BSC, POLYGON
         * Nếu server trả về TR thì lưu vào TR
         * Nếu server trả về BSC | POLYGON thì cũng lưu vào TR với điều kiện trùng với current network
         *
         * Thứ tự lưu trong mảng là [TR, BSC, POLYGON]
         *
         * Khi user lấy ra kết quả, nếu ko specify network thì sẽ trả về TR
         */
        private readonly Dictionary<string, float[]> _chestReward = new();
        private readonly Dictionary<string, float[]> _pendingChestReward = new();
        private readonly NetworkType _currentNetwork;
        private const int DATA_TYPE_AMOUNT = 8;
        private const string DATA_TYPE_TR = "TR";
        private const string DATA_TYPE_BSC = "BSC";
        private const string DATA_TYPE_POLYGON = "POLYGON";
        private const string DATA_TYPE_TON = "TON";
        private const string DATA_TYPE_SOLANA = "SOL";
        private const string DATA_TYPE_RON = "RON";
        private const string DATA_TYPE_BAS = "BAS";
        private const string DATA_TYPE_VIC = "VIC";


        public DefaultChestRewardManager(NetworkType currentNetwork) {  
            _currentNetwork = currentNetwork;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        #region INIT

        public void InitNewChestReward(IChestReward rewards) {
            _chestReward.Clear();
            _pendingChestReward.Clear();

            var list = rewards.Rewards;
            foreach (var e in list) {
                var t = e.Type.Name;
                var n = ConvertDataTypeToDataIndex(e.Network);
                var sameNetwork = IsSameNetwork(e.Network);
                
                // Set Claim Pending
                InitPendingChestRewardIfNotExisted(t);
                _pendingChestReward[t][n] = e.ClaimPending;

                // Set Chest Reward
                InitChestRewardIfNotExisted(t);
                _chestReward[t][n] = e.Value;
                
                // Set luôn vào TR nếu trùng network, chỉ set nếu ko phải starcore vì starcore giờ sẽ có 2 đồng TR và network
                if (sameNetwork && e.Type.Type != BlockRewardType.BLCoin) {
                    var tr = ConvertDataTypeToDataIndex(DATA_TYPE_TR);
                    _pendingChestReward[t][tr] = e.ClaimPending;
                    _chestReward[t][tr] = e.Value;
                }

                DispatchEvent(d => d.OnRewardChanged?.Invoke(e.Type.Type, e.Value));
                if (sameNetwork && e.Type.Type != BlockRewardType.BLCoin) {
                    DispatchEvent(d => d.OnSameNetworkRewardChanged?.Invoke(e.Type.Type, e.Value, e.Network));
                }
            }
        }

        private static int ConvertNetworkTypeToDataIndex(NetworkType networkType) {
            return networkType switch {
                //DevHoang: Add new airdrop
                NetworkType.Binance => 1,
                NetworkType.Polygon => 2,
                NetworkType.Ton => 3,
                NetworkType.Solana => 4,
                NetworkType.Ronin => 5,
                NetworkType.Base => 6,
                NetworkType.Viction => 7,
                _ => throw new ArgumentOutOfRangeException(nameof(networkType), networkType, null)
            };
        }

        private static int ConvertDataTypeToDataIndex(string network) {
            return network switch {
                //DevHoang: Add new airdrop
                DATA_TYPE_TR => 0,
                DATA_TYPE_BSC => 1,
                DATA_TYPE_POLYGON => 2,
                DATA_TYPE_TON => 3,
                DATA_TYPE_SOLANA => 4,
                DATA_TYPE_RON => 5,
                DATA_TYPE_BAS => 6,
                DATA_TYPE_VIC => 7,
                
                _ => throw new ArgumentOutOfRangeException(nameof(network), network, null)
            };
        }

        private bool IsSameNetwork(string network) {
            return network switch {
                //DevHoang: Add new airdrop
                DATA_TYPE_BSC => _currentNetwork == NetworkType.Binance,
                DATA_TYPE_POLYGON => _currentNetwork == NetworkType.Polygon,
                DATA_TYPE_TON => _currentNetwork == NetworkType.Ton,
                DATA_TYPE_SOLANA => _currentNetwork == NetworkType.Solana,
                DATA_TYPE_RON => _currentNetwork == NetworkType.Ronin,
                DATA_TYPE_BAS => _currentNetwork == NetworkType.Base,
                DATA_TYPE_VIC => _currentNetwork == NetworkType.Viction,
                DATA_TYPE_TR => true,
                _ => false
            };
        }

        private void InitChestRewardIfNotExisted(string type) {
            if (!_chestReward.ContainsKey(type)) {
                _chestReward.Add(type, new float[DATA_TYPE_AMOUNT]);
            }
        }

        private void InitPendingChestRewardIfNotExisted(string type) {
            if (!_pendingChestReward.ContainsKey(type)) {
                _pendingChestReward.Add(type, new float[DATA_TYPE_AMOUNT]);
            }
        }

        #endregion

        #region GET REWARD OF SPECIFY NETWORK
        
        private float GetChestRewardByNetwork(string type, NetworkType network) {
            var n = ConvertNetworkTypeToDataIndex(network);
            return GetChestRewardByNetwork(type, n);
        }

        private float GetChestRewardByNetwork(string type, int dataIndex) {
            if (_chestReward.ContainsKey(type)) {
                return _chestReward[type][dataIndex];
            }
            return 0;
        }

        public float GetChestRewardByNetwork(BlockRewardType type, NetworkType network) {
            if (type == BlockRewardType.Other) {
                return 0;
            }
            var t = RewardUtils.ConvertToBlockRewardType(type);
            return GetChestRewardByNetwork(t, network);
        }

        public float GetChestRewardByNetwork(IRewardType type, NetworkSymbol network) {
            var dataIndex = ConvertDataTypeToDataIndex(network.Name);
            return GetChestRewardByNetwork(type.Name, dataIndex);
        }

        #endregion

        #region GET REWARD OF CURRENT NETWORK

        public float GetChestReward(string type) {
            if (_chestReward.ContainsKey(type)) {
                var n = ConvertDataTypeToDataIndex(DATA_TYPE_TR);
                return _chestReward[type][n];
            }
            return 0;
        }
        
        public float GetChestReward(BlockRewardType type) {
            if (type == BlockRewardType.Other) {
                return 0;
            }
            var t = RewardUtils.ConvertToBlockRewardType(type);
            return GetChestReward(t);
        }
        public float GetChestReward(BlockRewardType type, string dataType) {
            if (type == BlockRewardType.Other) {
                return 0;
            }
            var t = RewardUtils.ConvertToBlockRewardType(type);
            
            if (_chestReward.ContainsKey(t)) {
                var n = ConvertDataTypeToDataIndex(dataType);
                return _chestReward[t][n];
            }
            return 0;
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
        
        public float GetBcoinRewardAndDeposit(string network) {
            return GetChestReward(BlockRewardType.BCoin, network) + GetChestReward(BlockRewardType.BCoinDeposited, network);
        }
        
        public float GetSenRewardAndDeposit(string network) {
            return GetChestReward(BlockRewardType.Senspark, network) + GetChestReward(BlockRewardType.SensparkDeposited, network);
        }
        
        public float GetRock() {
            var t = RewardUtils.ConvertToBlockRewardType(BlockRewardType.Rock);
            return GetChestReward(t);
        }
        
        #endregion

        #region SET REWARD OF SPECIFY NETWORK

        private void SetChestRewardByNetwork(string type, float value, NetworkType network) {
            // Set cho network nhưng cũng phải set cho TR
            InitChestRewardIfNotExisted(type);
            
            var n = ConvertNetworkTypeToDataIndex(network);
            _chestReward[type][n] = value;

            n = ConvertDataTypeToDataIndex(DATA_TYPE_TR);
            _chestReward[type][n] = value;
        }

        #endregion

        #region SET REWARD OF CURRENT NETWORK
        
        public void SetChestReward(string type, float value) {
            // Set cho TR nhưng cũng phải set cho current network
            InitChestRewardIfNotExisted(type);
            
            var n = ConvertNetworkTypeToDataIndex(_currentNetwork);
            _chestReward[type][n] = value;

            // Riêng với BLCoin thì không cần set vào TR, vì BLCoin sẽ có 2 đồng là TR và network
            if (type == nameof(BlockRewardType.BLCoin))
                return;
            n = ConvertDataTypeToDataIndex(DATA_TYPE_TR);
            _chestReward[type][n] = value;


        }

        public void SetChestReward(IRewardType type, float value) {
            SetChestReward(type.Name, value);
            DispatchEvent(e => e.OnRewardChanged?.Invoke(type.Type, value));
        }

        public void SetChestReward(BlockRewardType type, float value) {
            if (type == BlockRewardType.Other) {
                return;
            }
            var n = RewardUtils.ConvertToBlockRewardType(type);
            SetChestReward(n, value);
            DispatchEvent(e => e.OnRewardChanged?.Invoke(type, value));
        }

        #endregion

        #region ADJUST REWARD BY SPECIFY NETWORK

        private float AdjustChestRewardByNetwork(string type, float addValue, NetworkType network) {
            // Set cho network nhưng cũng phải set cho TR
            InitChestRewardIfNotExisted(type);
            
            var n = ConvertNetworkTypeToDataIndex(network);
            var val = _chestReward[type][n] + addValue;
            _chestReward[type][n] = val;

            n = ConvertDataTypeToDataIndex(DATA_TYPE_TR);
            _chestReward[type][n] = val;
                
            return val;
        }

        #endregion

        #region ADJUST REWARD BY CURRENT NETWORK
        
        public float AdjustChestReward(string type, float addValue) {
            // Set cho TR nhưng cũng phải set cho current network
            InitChestRewardIfNotExisted(type);

            var n = ConvertNetworkTypeToDataIndex(_currentNetwork);
            var val = _chestReward[type][n] + addValue;
            _chestReward[type][n] = val;

            // Riêng với BLCoin thì không cần set vào TR, vì BLCoin sẽ có 2 đồng là TR và network
            if (type != nameof(BlockRewardType.BCoin)) {
                n = ConvertDataTypeToDataIndex(DATA_TYPE_TR);
                _chestReward[type][n] = val;
            }
            
     

            return val;
        }

        public float AdjustChestReward(IRewardType type, float addValue) {
            var result = AdjustChestReward(type.Name, addValue);
            DispatchEvent(e => e.OnRewardChanged?.Invoke(type.Type, result));
            return result;
        }

        public float AdjustChestReward(BlockRewardType type, float addValue) {
            if (type == BlockRewardType.Other) {
                return 0;
            }
            var name = RewardUtils.ConvertToBlockRewardType(type);
            var result = AdjustChestReward(name, addValue);
            DispatchEvent(e => e.OnRewardChanged?.Invoke(type, result));
            return result;
        }

        #endregion

        #region GET CLAIM PENDING OF SPECIFY NETWORK
        
        private float GetClaimPendingRewardByNetwork(string type, NetworkType networkType) {
            var n = ConvertNetworkTypeToDataIndex(networkType);
            return GetClaimPendingRewardByNetwork(type, n);
        }
        
        private float GetClaimPendingRewardByNetwork(string type, int dataIndex) {
            if (_pendingChestReward.ContainsKey(type)) {
                return _pendingChestReward[type][dataIndex];
            }
            return 0;
        }

        public float GetClaimPendingRewardByNetwork(BlockRewardType type, NetworkType networkType) {
            if (type == BlockRewardType.Other) {
                return 0;
            }
            var name = RewardUtils.ConvertToBlockRewardType(type);
            return GetClaimPendingRewardByNetwork(name, networkType);
        }

        public float GetClaimPendingRewardByNetwork(IRewardType type, NetworkSymbol network) {
            var dataIndex = ConvertDataTypeToDataIndex(network.Name);
            return GetClaimPendingRewardByNetwork(type.Name, dataIndex);
        }

        #endregion

        #region GET CLAIM PENDING OF CURRENT NETWORK
        
        public float GetClaimPendingReward(string type) {
            if (_pendingChestReward.ContainsKey(type)) {
                var n = ConvertDataTypeToDataIndex(DATA_TYPE_TR);
                return _pendingChestReward[type][n];
            }
            return 0;
        }

        public IChestReward ParseChestReward(ISFSObject data) {
            var rewards = new List<ITokenReward>();
            var array = data.GetSFSArray("rewards");
            for (var i = 0; i < array.Size(); ++i) {
                var item = new TokenReward(array.GetSFSObject(i));
                rewards.Add(item);
            }
            var result = new ChestReward(rewards);
            InitNewChestReward(result);
            return result;
        }

        public float GetClaimPendingReward(IRewardType type) {
            return GetClaimPendingReward(type.Name);
        }

        public float GetClaimPendingReward(BlockRewardType type) {
            return GetClaimPendingRewardByNetwork(type, _currentNetwork);
        }

        #endregion
    }
}