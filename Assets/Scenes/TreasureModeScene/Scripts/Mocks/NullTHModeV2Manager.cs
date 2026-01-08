using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Sfs2X.Entities;
using Sfs2X.Entities.Data;

using UnityEngine;

namespace Scenes.TreasureModeScene.Scripts.Mocks {
    public class NullTHModeV2Manager : ITHModeV2Manager {
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public void OnConnection() {
            throw new NotImplementedException();
        }

        public void OnConnectionError(string message) {
            throw new NotImplementedException();
        }

        public void OnConnectionRetry() {
            throw new NotImplementedException();
        }

        public void OnConnectionResume() {
            throw new NotImplementedException();
        }

        public void OnConnectionLost(string reason) {
            throw new NotImplementedException();
        }

        public void OnLogin() {
            throw new NotImplementedException();
        }

        public void OnLoginError(int code, string message) {
            throw new NotImplementedException();
        }

        public void OnUdpInit(bool success) {
            throw new NotImplementedException();
        }

        public void OnPingPong(int lagValue) {
            throw new NotImplementedException();
        }

        public void OnRoomVariableUpdate(SFSRoom room) {
            throw new NotImplementedException();
        }

        public void OnJoinRoom(SFSRoom room) {
            throw new NotImplementedException();
        }

        public void OnExtensionResponse(string cmd, ISFSObject value) {
            throw new NotImplementedException();
        }

        

        public void OnExtensionResponse(string cmd, int requestId, byte[] data) {
        }

        public void OnExtensionError(string cmd, int requestId, int errorCode, string errorMessage) {
        }

        public int AddObserver(THModeObserver observer) {
            return 0;
        }

        public bool RemoveObserver(int id) {
            return true;
        }

        public void DispatchEvent(Action<THModeObserver> dispatcher) {
            throw new NotImplementedException();
        }

        public int CurrentTime { get; set; }
        public float TimeLeft { get; set; }

        public THModeV2RewardData GetRewardData() {
            throw new NotImplementedException();
        }

        public THModeV2PoolData GetPoolData() {
            throw new NotImplementedException();
        }

        public List<PoolData> GetCurrentPoolData(PoolType type) {
            return new List<PoolData>();
        }

        public float GetMaxPool(PoolType type, int rarity) {
            return 0;
        }

        public RectTransform GetPositionPool(int rarity) {
            throw new NotImplementedException();
        }

        public void SetPositionPool(List<RectTransform> listPostion) {
        }
    }
}