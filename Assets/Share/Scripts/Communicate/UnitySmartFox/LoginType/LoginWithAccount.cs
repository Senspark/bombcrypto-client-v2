using System;
using System.Collections;
using System.Collections.Generic;

using App;

using Communicate;

using Cysharp.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Senspark;

using Share.Scripts;
using Share.Scripts.Communicate;
using Share.Scripts.Communicate.UnityReact;

using UnityEngine;

public class LoginWithAccount : ILoginWith
{
    private readonly ILogManager _logger;
    private readonly IPublicJwtSession _jwtSession;

    public LoginWithAccount(IPublicJwtSession jwtSession, ILogManager logger) {
        _jwtSession = jwtSession;
        _logger = logger;
    }
    public async UniTask<JwtData> GetJwtData(IUnityReactSupportMethod unityReact, HandshakeType type) {
        try {
            if(_jwtSession.Account == null) {
                throw new Exception("User account not set");
            }
            var userAccount = _jwtSession.Account;
        
            var userName = userAccount.userName;
            var password = userAccount.password;
        
            var sendData = new JObject {
                {"userName", userName},
                {"password", password},
            };
            var data = await unityReact.SendToReact(ReactCommand.GET_JWT_ACCOUNT, sendData);
            _logger.Log($"Received JWT for account: {data}");

            var jsonObj = JsonConvert.DeserializeObject<CmdDataGetJwt>(data);
            if (jsonObj == null) {
                throw new Exception("Invalid data");
            }
            var extraData = new JwtExtraData(jsonObj.IsUserFi, jsonObj.Address, Username: userName, Password: password);
            return new JwtData(jsonObj.EncryptedJwt, jsonObj.ServerPublicKey, extraData);
        } catch (Exception e) {
            throw new IncorrectPassword("Username or password is incorrect");
        }
        
    }
    
    private record CmdDataGetJwt(
        [JsonProperty("encryptedJwt")]
        string EncryptedJwt,
        [JsonProperty("serverPublicKey")]
        string ServerPublicKey,
        [JsonProperty("isUserFi")]  
        bool IsUserFi,
        [JsonProperty("address")]
        string Address = null
    );
}
