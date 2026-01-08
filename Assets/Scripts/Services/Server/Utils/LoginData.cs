using App;
using Data;
using UnityEngine;
using DeviceType = App.DeviceType;

namespace Services.Server {
    public enum LoginDataType {
        Bombcrypto,
        Bomberland,
        Telegram,
        Solana,
    }

    public interface ILoginData {
        LoginDataType LoginDataType { get; }
        float TimeOut { get; }
    }

    public class LoginDataBombcrypto : ILoginData {
        public readonly string UserName;
        public readonly string Password;
        public readonly string ActivationCode;
        public readonly string Signature;
        public readonly LoginType LoginType;
        public readonly string Slogan;
        public LoginDataType LoginDataType => LoginDataType.Bombcrypto;
        public float TimeOut { get; }

        public LoginDataBombcrypto(
            string userName,
            string password,
            string activationCode,
            string signature,
            LoginType loginType,
            float timeOutSec
        ) {
            UserName = userName;
            Password = password;
            ActivationCode = activationCode;
            Signature = signature;
            LoginType = loginType;
            Slogan = SFSDefine.GetSlogans(EntryPoint.Login);
            TimeOut = timeOutSec;
        }
    }

    public class LoginDataBomberland : ILoginData {
        public readonly int LoginType;
        public readonly string Network;
        public readonly string JwtToken;
        public readonly string UserName;
        public readonly string Slogan;
        public readonly DeviceType DeviceType;
        public LoginDataType LoginDataType => LoginDataType.Bomberland;
        public float TimeOut { get; }

        public LoginDataBomberland(int loginType, string network, string jwtToken, string userName, float timeOutSec) {
            LoginType = loginType;
            Network = network;
            JwtToken = jwtToken;
            UserName = userName;
            Slogan = SFSDefine.GetSlogans(EntryPoint.Login);
            DeviceType = Application.isMobilePlatform
                ? DeviceType.Mobile
                : DeviceType.Web;
            TimeOut = timeOutSec;
        }
    }

    public class LoginDataUserTr : LoginDataBomberland {
        public LoginDataUserTr(string userName, string jwtToken) :
            base(1, null, jwtToken, userName, 30) { }
    }

    public class LoginDataUserFi : LoginDataBomberland {
        public LoginDataUserFi(string userName, string jwtToken, NetworkType network) :
            base(1, network switch
            {
                //DevHoang: Add new airdrop
                NetworkType.Binance => "BSC",
                NetworkType.Polygon => "POLYGON",
                NetworkType.Ronin => "RON",
                NetworkType.Base => "BAS",
                NetworkType.Viction => "VIC",
            }, jwtToken, userName, 30) { }
    }

    public class LoginDataUserGuest : LoginDataBomberland {
        public LoginDataUserGuest(string userName, string jwt) :
            base(3, null, jwt, userName, 30) { }
    }
    
    public class LoginDataTelegram : ILoginData {
        public readonly string Token;
        public readonly string UserName;
        public readonly string UserData;
        public readonly Platform Platform;
        public readonly int LoginType;
        public readonly DeviceType DeviceType;
        public readonly string ReferralCode;
        public LoginDataTelegram(string token, string userName, string userData, string referralCode, Platform platform) {
            Token = token;
            UserName = userName;
            UserData = userData;
            Platform = platform;
            LoginType = (int)App.LoginType.Telegram;
            DeviceType = Application.isMobilePlatform
                ? DeviceType.Mobile
                : DeviceType.Web;
            ReferralCode = referralCode;
            TimeOut = 30;
        }
        public LoginDataType LoginDataType => LoginDataType.Telegram;
        public float TimeOut { get; }
        
    }
    
    public class LoginDataSolana : ILoginData {
        public readonly string Token;
        public readonly string UserName;
        public readonly string UserData;
        public readonly Platform Platform;
        public readonly int LoginType;
        public readonly DeviceType DeviceType;

        public LoginDataSolana(string token, string userName, string userData, Platform platform) {
            Token = token;
            UserName = userName;
            UserData = userData;
            Platform = platform;
            LoginType = (int)App.LoginType.Solana;;
            DeviceType = Application.isMobilePlatform
                ? DeviceType.Mobile
                : DeviceType.Web;
            TimeOut = 60;
        }
        public LoginDataType LoginDataType => LoginDataType.Solana;
        public float TimeOut { get; }
        
    }
}