using System;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace App {
    public enum GamePlatform {
        UNKNOWN,
        WEBGL,
        TOURNAMENT,
        TON,
        SOL,
        MOBILE
    }
    public static class AppConfig {
        public static bool IsProduction { get; private set; }
        public static GamePlatform GamePlatform { get; private set; }
        public static IBuildConfig BuildConfig { get; private set; }
        public static string TestUserName { get; private set; }
        public static string TestPassword { get; private set; }
        public static string NewTestWallet { get; private set; }
        public static bool Simulated => true;
        public static bool EnableLog => !IsProduction;
        public static string WalletTon { get; private set; }
        public static string WalletTonHex {get; private set; }
        public static string AppsFlyerWebDevKey { get; private set; }
        public static string AppsFlyerAndroidDevKey { get; private set; }
        public static string AppsFlyerIosAppId { get; private set; }
        public static byte[] ReactPermutationOrder32 { get; private set; }
        public static int SmartFoxAppendBytes { get; private set; }
        public static int ApiAppendBytes { get; private set; }
        public static string ProductionSalt { get; private set; }
        public static string TestSalt { get; private set; }
        public static string AppleEditorAccessToken { get; private set; }
        public static string AdmobAppIdAndroid { get; private set; }
        public static string AdmobAppIdIos { get; private set; }
        public static string AdmobRewardedAdIdAndroid { get; private set; }
        public static string AdmobRewardedAdIdIos { get; private set; }
        public static string AdmobRewardedAdIdTest { get; private set; }
        public static string AdmobInterstitialAdIdAndroid { get; private set; }
        public static string AdmobInterstitialAdIdIos { get; private set; }
        public static string AdmobInterstitialAdIdTest { get; private set; }
        // Server Addresses
        public static string SvTestV1 { get; private set; }
        public static string SvTestV1Tcp { get; private set; }
        public static string SvTestV2 { get; private set; }
        public static string SvProd { get; private set; }
        public static string SvProdTcp { get; private set; }
        public static string SvTournamentProd { get; private set; }
        public static string SvProdTelegram { get; private set; }
        public static string SvProdSolana { get; private set; }
        public static string SvProdWebAirdrop { get; private set; }
        
        public static string AuthApiHostProduction { get; private set; }
        public static string AuthApiHostTest { get; private set; }
        
        public static string BaseApiHost { get; private set; }
        public static string TournamentBaseApiHost { get; private set; }
        public static string BaseApiTestHost { get; private set; }
        
        // Firebase Config
        public static string FirebaseApiKey { get; private set; }
        public static string FirebaseAuthDomain { get; private set; }
        public static string FirebaseProjectId { get; private set; }
        public static string FirebaseStorageBucket { get; private set; }
        public static string FirebaseMessagingSenderId { get; private set; }
        public static string FirebaseAppId { get; private set; }
        public static string FirebaseMeasurementId { get; private set; }

        private static Data _buildData;

        static AppConfig() {
            Initialize();
        }

        public static void Initialize() {
            const string path = "configs/AppConfig";
            var d = Resources.Load<TextAsset>(path);
            var data = JsonConvert.DeserializeObject<Data>(d.text);
            _buildData = data ?? throw new NullReferenceException(path);
            IsProduction = data.isProduction;
            GamePlatform = data.gamePlatform;
            TestUserName = data.testUserName;
            TestPassword = data.testPassword;
            NewTestWallet = data.newTestWallet;
            WalletTon = data.walletTon;
            WalletTonHex = data.walletTonHex;
            AppsFlyerWebDevKey = data.appsFlyer?.webDevKey;
            AppsFlyerAndroidDevKey = data.appsFlyer?.androidDevKey;
            AppsFlyerIosAppId = data.appsFlyer?.iosAppId;
            ReactPermutationOrder32 = data.encryption?.reactPermutationOrder32;
            SmartFoxAppendBytes = data.encryption?.smartFoxAppendBytes ?? 32;
            ApiAppendBytes = data.encryption?.apiAppendBytes ?? 4;
            ProductionSalt = data.encryption?.productionSalt;
            ProductionSalt = data.encryption?.productionSalt;
            ProductionSalt = data.encryption?.productionSalt;
            TestSalt = data.encryption?.testSalt;
            AppleEditorAccessToken = data.apple?.editorAccessToken;
            AdmobAppIdAndroid = data.ads?.admobAppIdAndroid;
            AdmobAppIdIos = data.ads?.admobAppIdIos;
            AdmobRewardedAdIdAndroid = data.ads?.admobRewardedAdIdAndroid;
            AdmobRewardedAdIdIos = data.ads?.admobRewardedAdIdIos;
            AdmobRewardedAdIdTest = data.ads?.admobRewardedAdIdTest;
            AdmobInterstitialAdIdAndroid = data.ads?.admobInterstitialAdIdAndroid;
            AdmobInterstitialAdIdIos = data.ads?.admobInterstitialAdIdIos;
            AdmobInterstitialAdIdTest = data.ads?.admobInterstitialAdIdTest;

            // Server Addresses
            SvTestV1 = data.serverAddresses?.svTestV1;
            SvTestV1Tcp = data.serverAddresses?.svTestV1Tcp;
            SvTestV2 = data.serverAddresses?.svTestV2;
            SvProd = data.serverAddresses?.svProd;
            SvProdTcp = data.serverAddresses?.svProdTcp;
            SvTournamentProd = data.serverAddresses?.svTournamentProd;
            SvProdTelegram = data.serverAddresses?.svProdTelegram;
            SvProdSolana = data.serverAddresses?.svProdSolana;
            SvProdWebAirdrop = data.serverAddresses?.svProdWebAirdrop;
            
            AuthApiHostProduction = data.serverAddresses?.authApiHostProduction;
            AuthApiHostTest = data.serverAddresses?.authApiHostTest;
            
            BaseApiHost = data.serverAddresses?.baseApiHost;
            TournamentBaseApiHost = data.serverAddresses?.tournamentBaseApiHost;
            BaseApiTestHost = data.serverAddresses?.baseApiTestHost;
            
            FirebaseApiKey = data.firebase?.apiKey;
            FirebaseAuthDomain = data.firebase?.authDomain;
            FirebaseProjectId = data.firebase?.projectId;
            FirebaseStorageBucket = data.firebase?.storageBucket;
            FirebaseMessagingSenderId = data.firebase?.messagingSenderId;
            FirebaseAppId = data.firebase?.appId;
            FirebaseMeasurementId = data.firebase?.measurementId;
            
            BuildConfig = new DefaultBuildConfig(IsProduction);
        }

        public static int GetVersion(bool isProduction) {
            var d = _buildData.buildInfo;
            return isProduction ? d.productionVersion : d.testVersion;
        }

        public static string GetSaltKey(bool isProduction) {
            var salt = isProduction ? ProductionSalt : TestSalt;
            var data = Convert.FromBase64String(salt);
            var decodedString = Encoding.UTF8.GetString(data);
            return decodedString;
        }

        public static string GetPackageName() {
            return Application.identifier;
        }

        public static TestWalletInfo GetTestWalletInfo() {
            return _buildData.testWallet;
        }
        
        public static bool IsWebGL() {
            return GamePlatform == GamePlatform.WEBGL || GamePlatform == GamePlatform.TOURNAMENT;
        }
        public static bool IsTon() {
            return GamePlatform == GamePlatform.TON;
        }
        public static bool IsSolana() {
            return GamePlatform == GamePlatform.SOL;
        }
        public static bool IsMobile() {
            return GamePlatform == GamePlatform.MOBILE;
        }
        
        public static bool IsRonin() {
            return _buildData.isWebGLAirdrop.webGLRonin;
        }
        
        public static bool IsBase() {
            return _buildData.isWebGLAirdrop.webGLBase;
        }
        
        public static bool IsViction() {
            return _buildData.isWebGLAirdrop.webGLViction;
        }

        public static void SetWebGLNetwork(NetworkType network) {
            _buildData.isWebGLAirdrop.webGLRonin = network == NetworkType.Ronin;
            _buildData.isWebGLAirdrop.webGLBase = network == NetworkType.Base;
            _buildData.isWebGLAirdrop.webGLViction = network == NetworkType.Viction;
        }
        
        // bản webGL hoặc mobile của bsc / polygon
        //FIXME: Để tạm cho tournament, xong sẽ xoá
        public static bool IsBscOrPolygon() {
            return (GamePlatform == GamePlatform.WEBGL || GamePlatform == GamePlatform.MOBILE || GamePlatform == GamePlatform.TOURNAMENT)
                && !IsWebAirdrop();
        }
        public static bool IsTournament() {
            return GamePlatform == GamePlatform.TOURNAMENT;
        }

        public static bool IsAirDrop() {
            return GamePlatform == GamePlatform.TON ||
                   GamePlatform == GamePlatform.SOL ||
                   IsWebAirdrop();
        }
        
        public static bool IsWebAirdrop() {
            return _buildData.isWebGLAirdrop.webGLRonin ||
                   _buildData.isWebGLAirdrop.webGLBase ||
                   _buildData.isWebGLAirdrop.webGLViction;
        }
        
        public static void Reset() {
            _buildData.isWebGLAirdrop.webGLRonin = false;
            _buildData.isWebGLAirdrop.webGLBase = false;
            _buildData.isWebGLAirdrop.webGLViction = false;
        }

        [Serializable]
        private class Data {
            public bool isProduction;
            public bool isTournament;
            public bool isTelegram;
            public GamePlatform gamePlatform;
            public WebGLAirdropInfo isWebGLAirdrop;
            public BuildInfo buildInfo;
            public string testUserName;
            public string testPassword;
            public TestWalletInfo testWallet;
            public string newTestWallet;
            public string walletTon;
            public string walletTonHex;
            public string userTonData;
            public string userTonDataRaw;
            public AppsFlyerInfo appsFlyer;
            public AppleInfo apple;
            public AdsInfo ads;
            public EncryptionInfo encryption;
            public ServerAddressInfo serverAddresses;
            public FirebaseInfo firebase;
        }
        
        [Serializable]
        private class WebGLAirdropInfo {
            public bool webGLRonin;
            public bool webGLBase;
            public bool webGLViction;
        }

        [Serializable]
        private class BuildInfo {
            public int testVersion;
            public int productionVersion;
        }

        [Serializable]
        public class TestWalletInfo {
            public string address;
            public string jwt;
        }

        [Serializable]
        private class AppsFlyerInfo {
            public string webDevKey;
            public string androidDevKey;
            public string iosAppId;
        }

        [Serializable]
        public class FirebaseInfo {
            public string apiKey;
            public string authDomain;
            public string projectId;
            public string storageBucket;
            public string messagingSenderId;
            public string appId;
            public string measurementId;
        }

        [Serializable]
        private class EncryptionInfo {
            public byte[] reactPermutationOrder32;
            public int smartFoxAppendBytes;
            public int apiAppendBytes;
            public string productionSalt;
            public string testSalt;
        }

        [Serializable]
        private class ServerAddressInfo {
            public string svTestV1;
            public string svTestV1Tcp;
            public string svTestV2;
            public string svProd;
            public string svProdTcp;
            public string svTournamentProd;
            public string svProdTelegram;
            public string svProdSolana;
            public string svProdWebAirdrop;
            public string authApiHostProduction;
            public string authApiHostTest;
            public string baseApiHost;
            public string tournamentBaseApiHost;
            public string baseApiTestHost;
        }

        [Serializable]
        private class AppleInfo {
            public string editorAccessToken;
        }

        [Serializable]
        private class AdsInfo {
            public string admobAppIdAndroid;
            public string admobAppIdIos;
            public string admobRewardedAdIdAndroid;
            public string admobRewardedAdIdIos;
            public string admobRewardedAdIdTest;
            public string admobInterstitialAdIdAndroid;
            public string admobInterstitialAdIdIos;
            public string admobInterstitialAdIdTest;
        }
    }
}