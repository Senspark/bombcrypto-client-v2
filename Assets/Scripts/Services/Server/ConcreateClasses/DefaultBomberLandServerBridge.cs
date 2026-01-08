using System.Collections.Generic;
using System.Threading.Tasks;
using BLPvpMode.Manager.Api;
using BLPvpMode.Manager.Api.Handlers;
using CustomSmartFox.SolCommands;
using JetBrains.Annotations;
using Sfs2X.Entities.Data;
using UnityEngine;

namespace App.BomberLand {
    public partial class DefaultBomberLandServerBridge : IBomberLandServerBridge {
        private readonly IDictionary<EntryPoint, string> _slogans;
        private readonly ISmartFoxApi _api;
        private readonly bool _enableLog;
        
        [NotNull]
        private readonly IExtensionRequestBuilder _requestBuilder;
        
        [NotNull]
        private readonly ITaskDelay _taskDelay;

        [NotNull]
        private readonly ICacheRequestManager _cacheRequestManager;
        
        [NotNull]
        private readonly IServerDispatcher _serverDispatcher;
        
        public DefaultBomberLandServerBridge(
            bool enableLog,
            ISmartFoxApi api,
            IExtensionRequestBuilder requestBuilder,
            ITaskDelay taskDelay,
            ICacheRequestManager cacheRequestManager,
            IServerDispatcher serverDispatcher
        ) {
            _enableLog = enableLog;
            _api = api;
            _requestBuilder = requestBuilder;
            _taskDelay = taskDelay;
            _cacheRequestManager = cacheRequestManager;
            _serverDispatcher = serverDispatcher;
        }
        
        public async Task DeleteUser(string accessToken) {
            var data = new SFSObject().Apply(it => {
                accessToken ??= string.Empty;
                it.PutUtfString("access_token", accessToken);
            });
            
            var result = await _serverDispatcher.SendCmd(new CmdDeleteUser(data));
            OnDeleteUser(result);
        }
        
        private bool OnDeleteUser(ISFSObject data) {
            return true;
        }

        public async Task<bool> EnterPasscode(string passCode) {
            var data = new SFSObject().Apply(it => {
                it.PutUtfString("passcode", passCode);
            });
            
            var result = await _serverDispatcher.SendCmd(new CmdEnterPasscode(data));
            return OnEnterPassCode(result);
        }
        
        private bool OnEnterPassCode(ISFSObject data) {
            var result = data.GetBool("success");
            return result;
        }

        public async Task<ValidateIapResult> ValidateBuyIapGem(ValidateIapRequest requestData) {
            var data = new SFSObject().Apply(it => {
                var storeId = Application.platform switch {
                    RuntimePlatform.Android => 0,
                    RuntimePlatform.IPhonePlayer => 1,
                    _ => -1,
                };
                it.PutUtfString("transaction_id", requestData.TransactionId);
                it.PutUtfString("product_id", requestData.ProductId);
                it.PutUtfString("bill_token", requestData.PurchaseToken);
                it.PutUtfString("package_name", requestData.PackageName);
                it.PutInt("store_id", storeId);
            });
            
            var result = await _serverDispatcher.SendCmd(new CmdBuyGem(data));
            return OnValidateBuyIap(result);
        }

        public async Task<ValidateIapResult> ValidateBuyIapOffer(ValidateIapRequest requestData) {
            var data = new SFSObject().Apply(it => {
                var storeId = Application.platform switch {
                    RuntimePlatform.Android => 0,
                    RuntimePlatform.IPhonePlayer => 1,
                    _ => -1,
                };
                it.PutUtfString("transaction_id", requestData.TransactionId);
                it.PutUtfString("product_id", requestData.ProductId);
                it.PutUtfString("bill_token", requestData.PurchaseToken);
                it.PutUtfString("package_name", requestData.PackageName);
                it.PutInt("store_id", storeId);
            });
            
            var result = await _serverDispatcher.SendCmd(new CmdBuyPack(data));
            return OnValidateBuyIap(result);
        }
        
        private ValidateIapResult OnValidateBuyIap(ISFSObject data) {
            Debug.Log("OnValidateBuyIap");
            var errCode = ServerUtils.GetErrorCode(data);
            switch (errCode) {
                case 0:
                    var result = data.GetBool("success");
                    return result ? ValidateIapResult.Success : ValidateIapResult.Failed;
                case ErrorCode.IAP_SHOP_BILL_ALREADY_USED:
                    return ValidateIapResult.Existed;
                default:
                    throw ServerUtils.ParseErrorMessage(data);
            }
        }

        public async Task<IOfferPacksResult> GetOfferPacks() {
            var data = new SFSObject();

            var serverManager = (IServerManager)_serverDispatcher;
            var response = await serverManager.SendExtensionRequestAsync(new CmdGetPackShop(data));
            return OnGetPackShop(response);
        }
        
        private IOfferPacksResult OnGetPackShop(ISFSObject data) {
            var result = new OfferPacksResult(data);
            return result;
        }

        public async Task<bool> GetRemoveInterstitialAds() {
            var data = new SFSObject();

            var response = await _serverDispatcher.SendCmd(new CmdCheckNoAds(data));
            return OnGetRemoveAds(response);
        }
        
        private bool OnGetRemoveAds(ISFSObject data) {
            var resp = data.GetBool("no_ads");
            return resp;
        }
    }
}