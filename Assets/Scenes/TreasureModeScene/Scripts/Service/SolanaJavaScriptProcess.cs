using System;
using System.Threading.Tasks;
using Game.Dialog;
using Newtonsoft.Json.Linq;
using Senspark;
using Share.Scripts.Communicate;
using Share.Scripts.Communicate.UnityReact;
using UnityEngine;

namespace Scenes.TreasureModeScene.Scripts.Service {
    public interface ISolanaReactProcess {
        void LogoutUserSolana();
        Task<bool> DepositSol(string invoice, double amount, DepositType depositType);
    }
    
    public class SolanaReactProcess: ISolanaReactProcess {
        private readonly ILogManager _logManager;
        private readonly IMasterUnityCommunication _unityCommunication;     
        public SolanaReactProcess(IMasterUnityCommunication unityCommunication, ILogManager logManager) {
            _unityCommunication = unityCommunication;
            _logManager = logManager;
        }

        public void LogoutUserSolana() {
            try {
                _logManager.Log();
                App.Utils.Logout();
            } catch (Exception ex) {
                _logManager.Log($"[CLIENT] Error when logout: {ex.Message}");
                App.Utils.KickToConnectScene();
            }
        }

        public async Task<bool> DepositSol(string invoice, double amount, DepositType depositType) {
            try {
                _logManager.Log();
                
                if (Application.isEditor) {
                    return true;
                }
                var depositCmd = new JObject {
                    ["amount"] = amount,
                    ["invoiceCode"] = invoice
                };
                var cmd = depositType == DepositType.SOL_DEPOSIT
                    ? ReactCommand.DEPOSIT
                    : ReactCommand.DEPOSIT_BCOIN_SOL;
                var response = await _unityCommunication.UnityToReact.SendToReact(cmd, depositCmd.ToString());
                var result = bool.Parse(response);
                _logManager.Log($"Deposit response: {result}");
                return result;
            } catch (Exception ex) {
                _logManager.Log($"[CLIENT] Error when deposit: {ex.Message}");
                return false;
            }
        }
        
    }
}