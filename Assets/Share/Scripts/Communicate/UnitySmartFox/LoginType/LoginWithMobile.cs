using System;

using App;

using Cysharp.Threading.Tasks;

using Newtonsoft.Json;

using Senspark;

using Share.Scripts.Communicate.UnityReact;

using System.IdentityModel.Tokens.Jwt;
using System.Linq;

using Castle.Core.Internal;

using Communicate;
using Communicate.Encrypt;

using Share.Scripts.CustomException;

using UnityEngine;

namespace Share.Scripts.Communicate.UnitySmartFox.LoginType {
    public class LoginWithMobile : ILoginWith {
        private readonly ILogManager _logger;
        private readonly IMobileRequest _mobileRequest;
        private readonly IPublicJwtSession _jwtSession;
        private readonly IObfuscate _obfuscate;
        
        private const string InvalidJwt = "Data login is invalid"; 

        public LoginWithMobile(IMobileRequest mobileRequest, IPublicJwtSession jwtSession,
            ILogManager logger) {
            _mobileRequest = mobileRequest;
            _jwtSession = jwtSession;
            _logger = logger;
            _obfuscate = new AppendBytesObfuscate(Secret.ApiAppendBytes);
        }

        public async UniTask<JwtData> GetJwtData(IUnityReactSupportMethod react, HandshakeType type) {
            JwtDataMobile result;
            var userAccount = _jwtSession.Account;
            // Login lần đầu, phải check proof
            if (_jwtSession.RawJwt.IsNullOrEmpty()) {
                if (userAccount.loginType == App.LoginType.UsernamePassword) {
                    result = await _mobileRequest.CheckProof(userAccount.userName, userAccount.password);
                } else {
                    result = await _mobileRequest.CheckProofGuest(userAccount.userName);
                }
            } else {
                var jwtValidator = new JwtValidator();
                var r = jwtValidator.ValidateJwt(_jwtSession.RawJwt, userAccount?.userName);
                if (r == JwtValidation.Valid) {
                    if (_jwtSession.RawJwt.IsNullOrEmpty() || _jwtSession.PublicKey.IsNullOrEmpty()) {
                        _logger.Log("Jwt is valid but jwt or public key is null or empty");
                        throw new InvalidJwtException(InvalidJwt);
                    }
                    return new JwtData(_jwtSession.RawJwt, _jwtSession.PublicKey, _jwtSession.ExtraData);
                }
                if (r == JwtValidation.Expired) {
                    _logger.Log("Jwt is expired, refresh jwt");
                    if (userAccount?.loginType is App.LoginType.Guest) {
                        result = await _mobileRequest.RefreshJwtGuest(userAccount.userName);
                    } else if (userAccount?.loginType is App.LoginType.UsernamePassword &&
                               !userAccount.password.IsNullOrEmpty()) {
                        result = await _mobileRequest.RefreshJwtAccount(userAccount.userName, userAccount.password);
                    } else {
                        result = null;
                    }

                    if (result == null) {
                        _logger.Log("Jwt refresh failed, get new jwt");
                        throw new InvalidJwtException(InvalidJwt);
                    }
                } else {
                    _logger.Log("Jwt is invalid, get new jwt");
                    throw new InvalidJwtException(InvalidJwt);
                }
            }
            _logger.Log($"Received JWT for account mobile: {result}");

            var extraData = JsonConvert.DeserializeObject<JwtExtraData>(result.ExtraData);
            var key = _obfuscate.DeObfuscate(result.Key);
            return new JwtData(result.Jwt, key, extraData);
        }
    }
}

// Use for mobile only
public class JwtValidator {
    public JwtValidation ValidateJwt(string jwt, string savedWalletAddress) {
        if (string.IsNullOrEmpty(jwt)) {
            return JwtValidation.Error;
        }
        if (string.IsNullOrEmpty(savedWalletAddress)) {
            return JwtValidation.Error;
        }
        try {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            var exp = token.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
            if (exp == null) {
                return JwtValidation.Error;
            }
            var expTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp)).UtcDateTime;
            var now = DateTime.UtcNow;
            if ((expTime - now).TotalSeconds < 300) {
                return JwtValidation.Expired;
            }
            var jwtWalletAddress = token.Claims.FirstOrDefault(c => c.Type == "userName")?.Value;
            if (jwtWalletAddress == null) {
                return JwtValidation.NoAddress;
            }
            if (!jwtWalletAddress.Equals(savedWalletAddress, StringComparison.OrdinalIgnoreCase)) {
                return JwtValidation.NoAddress;
            }
            return JwtValidation.Valid;
        } catch (Exception e) {
            return JwtValidation.Error;
        }
    }
}

public enum JwtValidation {
    Error,
    Expired,
    NoAddress,
    Valid
}