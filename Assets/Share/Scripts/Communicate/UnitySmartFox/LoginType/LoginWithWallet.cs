using System;
using System.Collections;
using System.Collections.Generic;

using App;

using Cysharp.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Senspark;

using Share.Scripts;
using Share.Scripts.Communicate;
using Share.Scripts.Communicate.UnityReact;

using UnityEngine;

/// <summary>
/// Web bsc và sol dùng chung class này
/// </summary>
public class LoginWithWallet : ILoginWith {
    private readonly ILogManager _logger;

    public LoginWithWallet(ILogManager logger) {
        _logger = logger;
    }

    public async UniTask<JwtData> GetJwtData(IUnityReactSupportMethod unityReact,  HandshakeType type) {
        var data = await unityReact.SendToReact(ReactCommand.GET_JWT_WALLET, type.ToString());
        _logger.Log($"Received JWT for wallet: {data}");

        var jsonObj = JsonConvert.DeserializeObject<CmdDataGetJwt>(data);
        if (jsonObj == null) {
            throw new Exception("Invalid data");
        }
        var extraData = new JwtExtraData(true, WalletAddress: jsonObj.WalletAddress, ChainId: jsonObj.ChainId);
        return new JwtData(jsonObj.EncryptedJwt, jsonObj.ServerPublicKey, extraData);
    }

    private record CmdDataGetJwt(
        [JsonProperty("walletAddress")]
        string WalletAddress,
        [JsonProperty("encryptedJwt")]
        string EncryptedJwt,
        [JsonProperty("serverPublicKey")]
        string ServerPublicKey,
        [JsonProperty("chainId")]
        string ChainId = null
    );
}