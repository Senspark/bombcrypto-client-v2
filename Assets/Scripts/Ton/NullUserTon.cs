using System;
using System.Threading.Tasks;

using App;

using Game.Dialog;

using Sfs2X.Entities;
using Sfs2X.Entities.Data;

public class NullUserTon : IUserTonManager {
    public Task<bool> Initialize() {
        return Task.FromResult(true);
    }

    public void Destroy() {
    }

    public void OnConnection() {
    }

    public void OnConnectionError(string message) {
    }

    public void OnConnectionRetry() {
    }

    public void OnConnectionResume() {
    }

    public void OnConnectionLost(string reason) {
    }

    public void OnLogin() {
    }

    public void OnLoginError(int code, string message) {
    }

    public void OnUdpInit(bool success) {
    }

    public void OnPingPong(int lagValue) {
    }

    public void OnRoomVariableUpdate(SFSRoom room) {
    }

    public void OnJoinRoom(SFSRoom room) {
    }

    public void OnExtensionResponse(string cmd, ISFSObject value) {
    }

    

    public void OnExtensionResponse(string cmd, int requestId, byte[] data) {
    }

    public void OnExtensionError(string cmd, int requestId, int errorCode, string errorMessage) {
    }

    public int AddObserver(UserTonObserver observer) {
        return 0;
    }

    public bool RemoveObserver(int id) {
        return true;
    }

    public void DispatchEvent(Action<UserTonObserver> dispatcher) {
    }

    public void LogoutUserTon() {
    }

    public Task<string> GetInvoice(double amount, DepositType depositType) {
        return Task.FromResult(string.Empty);

    }

    public Task<bool> GetTaskTonDataConfig() {
        return Task.FromResult(true);
    }

    public Task<bool> CompleteTask(int taskId) {
        return Task.FromResult(true);
    }

    public Task<bool> ClaimTask(int taskId) {
        return Task.FromResult(true);
    }

    public Task<IReferralData> GetReferralData() {
        return Task.FromResult<IReferralData>(null);
    }

    public Task<bool> ClaimReferralReward() {
        return Task.FromResult(true);
    }

    public Task<bool> ReactiveHouse(int houseId) {
        return Task.FromResult(true);
    }
}