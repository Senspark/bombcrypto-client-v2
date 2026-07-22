using System;
using System.Text;

using JetBrains.Annotations;

using Newtonsoft.Json;

using UnityEngine;

namespace App {
    public enum GamePlatform {
        UNKNOWN,
        WEBGL, // Web build for ETH Network
        TOURNAMENT, // Web build for PvP Tournament only
        TON, // Web build for Telegram mini App
        SOL, // Web build for Solana
        MOBILE // Android/iOS Build
    }

    public enum EthNetwork {
        None,
        Bsc,
        Polygon,
        Ronin,
        Base
    }

    public static class AppConfig {
        public static bool IsProduction { get; private set; }
        public static GamePlatform GamePlatform { get; private set; }
        public static EthNetwork EthNetwork { get; private set; }
        public static IBuildConfig BuildConfig { get; private set; }
        public static string TestUserName { get; private set; }
        public static string TestPassword { get; private set; }
        public static string TestWalletSolana { get; private set; }
        public static bool Simulated => false;
        public static bool EnableLog => !IsProduction;
        public static string WalletTon { get; private set; }
        public static string TestWalletTonHex { get; private set; }
        public static string TestWalletEth { get; private set; }
        public static string EditorAccount { get; private set; }
        
        [CanBeNull] public static EncryptionInfo EncryptionData { get; private set; }
        [CanBeNull] public static ServerAddressInfo ServerAddresses { get; private set; }
        [CanBeNull] public static FirebaseInfo FirebaseWebGLData { get; private set; }
        [CanBeNull] public static AppsFlyerInfo AppsFlyerData { get; private set; }
        [CanBeNull] public static AppleInfo AppleData { get; private set; }
        [CanBeNull] public static AdsInfo AdsData { get; private set; }
        
        private static int ProductionVersion;
        private static int TestVersion;

        static AppConfig() {
            Initialize();
        }

        public static void Initialize() {
            const string path = "configs/AppConfig";
            var d = Resources.Load<TextAsset>(path);
            var data = JsonConvert.DeserializeObject<Data>(d.text);
            IsProduction = data.isProduction;
            GamePlatform = data.gamePlatform;
            EthNetwork = ParseEthNetworkFromString(data.forEthNetwork);
            TestUserName = data.testUserName;
            TestPassword = data.testPassword;
            TestWalletSolana = data.testWalletSolana;
            TestWalletTonHex = data.testWalletTonHex;
            TestWalletEth = data.testWalletEth;
            EditorAccount = data.editorAccount;
            AppsFlyerData = data.appsFlyer;
            EncryptionData = data.encryption;

            ServerAddresses = data.serverAddresses;

            FirebaseWebGLData = data.firebaseWebGL;
            AppleData = data.apple;
            AdsData = data.ads;
            
            BuildConfig = new DefaultBuildConfig(IsProduction);
            ProductionVersion = data.buildInfo.productionVersion;
            TestVersion = data.buildInfo.testVersion;
        }

        public static int GetVersion(bool isProduction) {
            return isProduction ? ProductionVersion : TestVersion;
        }

        public static string GetSaltKey(bool isProduction) {
            var salt = isProduction ? EncryptionData.productionSalt : string.Empty;
            var data = Convert.FromBase64String(salt);
            var decodedString = Encoding.UTF8.GetString(data);
            return decodedString;
        }

        public static string GetPackageName() {
            return Application.identifier;
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
        
        public static bool IsRonin() {
            return GamePlatform == GamePlatform.WEBGL && EthNetwork == EthNetwork.Ronin;
        }
        
        public static bool IsBase() {
            return GamePlatform == GamePlatform.WEBGL && EthNetwork == EthNetwork.Base;
        }

        public static bool IsViction() {
            return false;
        }

        public static bool IsMobile() {
            return GamePlatform == GamePlatform.MOBILE;
        }

        public static bool IsEditor => Application.isEditor;

        public static void SetWebGLNetwork(NetworkType network) {
            switch (network) {
                case NetworkType.Binance:
                    EthNetwork = EthNetwork.Bsc;
                    break;
                case NetworkType.Polygon:
                    EthNetwork = EthNetwork.Polygon;
                    break;
                case NetworkType.Ronin:
                    EthNetwork = EthNetwork.Ronin;
                    break;
                case NetworkType.Base:
                    EthNetwork = EthNetwork.Base;
                    break;
                default:
                    // ignore
                    break;
            }
        }

        // bản webGL hoặc mobile của bsc / polygon
        //FIXME: Để tạm cho tournament, xong sẽ xoá
        public static bool IsBscOrPolygon() {
            return (GamePlatform == GamePlatform.WEBGL || GamePlatform == GamePlatform.MOBILE ||
                    GamePlatform == GamePlatform.TOURNAMENT)
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
            return GamePlatform == GamePlatform.WEBGL && EthNetwork > EthNetwork.Polygon;
        }

        public static void Reset() {
            EthNetwork = EthNetwork.None;
        }

        [Serializable]
        private class Data {
            public bool isProduction;
            public bool isTournament;
            public bool isTelegram;
            public GamePlatform gamePlatform;
            public string forEthNetwork;
            public BuildInfo buildInfo;
            public string testUserName;
            public string testPassword;
            public string testWalletSolana;
            public string testWalletTonHex;
            public string testWalletEth;
            public string editorAccount;
            [CanBeNull] public AppsFlyerInfo appsFlyer;
            [CanBeNull] public AppleInfo apple;
            [CanBeNull] public AdsInfo ads;
            [CanBeNull] public FirebaseInfo firebaseWebGL;
            [CanBeNull] public ServerAddressInfo serverAddresses;
            [CanBeNull] public EncryptionInfo encryption;
        }

        [Serializable]
        private class BuildInfo {
            public int testVersion;
            public int productionVersion;
        }

        [Serializable]
        public class AppsFlyerInfo {
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
        public class EncryptionInfo {
            public byte[] reactPermutationOrder32;
            public int smartFoxAppendBytes;
            public int apiAppendBytes;
            public string productionSalt;
            public char rsaDelimiter;
        }

        [Serializable]
        public class ServerAddressInfo {
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
            [CanBeNull] public TestServerConfig testServers;
        }

        [Serializable]
        public class TestServerConfig {
            public string webBuild;
        }

        [Serializable]
        public class AppleInfo {
            public string editorAccessToken;
        }

        [Serializable]
        public class AdsInfo {
            public string admobAppIdAndroid;
            public string admobAppIdIos;
            public string admobRewardedAdIdAndroid;
            public string admobRewardedAdIdIos;
            public string admobRewardedAdIdTest;
            public string admobInterstitialAdIdAndroid;
            public string admobInterstitialAdIdIos;
            public string admobInterstitialAdIdTest;
        }

        private static EthNetwork ParseEthNetworkFromString(string value) {
            return value switch {
                "BSC" => EthNetwork.Bsc,
                "POL" => EthNetwork.Polygon,
                "RON" => EthNetwork.Ronin,
                "BAS" => EthNetwork.Base,
                _ => EthNetwork.None
            };
        }
    }
}