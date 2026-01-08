using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace App {
    public static class ServerAddress {
        private static string SvTestV1 => AppConfig.SvTestV1;
        private static string SvTestV1Tcp => AppConfig.SvTestV1Tcp;
        private static string SvTestV2 => AppConfig.SvTestV2;

        private static string SvProd => AppConfig.SvProd;
        private static string SvProdTcp => AppConfig.SvProdTcp;
        private static string SvTournamentProd => AppConfig.SvTournamentProd;
        private static string SvProdTelegram => AppConfig.SvProdTelegram;
        private static string SvProdSolana => AppConfig.SvProdSolana;
        private static string SvProdWebAirdrop => AppConfig.SvProdWebAirdrop;

        private const string PingMain = "127.0.0.1";
        private const string SvLocal = "127.0.0.1";

        public const int WsPort = 8080;
        public const int WssPort = 8443;
        public const int TcpPort = 9933;

        private static readonly List<Info> ServerTestForWebBuild = new() {
            new Info("Production", SvProd, WssPort, true, PingMain),
            new Info("Test", SvTestV1, WssPort, false, PingMain),
            new Info("Test 2", SvTestV2, WssPort, false, PingMain),
            new Info("___ ws", SvLocal, WsPort, false, PingMain),
            new Info("___ tcp", SvLocal, TcpPort, false, PingMain),
        };

        private static readonly List<Info> ServerTestForWebEditor = new() {
            new Info("Production", SvProdTcp, TcpPort, true, PingMain),
            new Info("Test", SvTestV1, WssPort, false, PingMain),
            new Info("Test 2", SvTestV2, WssPort, false, PingMain),
            new Info("___ ws", SvLocal, WsPort, false, PingMain),
            new Info("___ tcp", SvLocal, TcpPort, false, PingMain),
        };

        private static readonly List<Info> ServerTestForMobileBuild = new() {
            new Info("Production", SvProdTcp, TcpPort, true, PingMain),
            new Info("Test", SvTestV1, WssPort, false, PingMain),
            new Info("Test 2", SvTestV2, WssPort, false, PingMain),
        };

        private static readonly List<Info> ServerTestForMobileEditor = new() {
            new Info("Production", SvProdTcp, TcpPort, true, PingMain),
            new Info("Test", SvTestV1Tcp, TcpPort, false, PingMain),
            new Info("___ tcp", SvLocal, TcpPort, false, PingMain),
        };

        private static readonly List<Info> ServerProdForWebBuild = new() {
            new Info("Production", SvProd, WssPort, true, PingMain),
        };

        private static readonly List<Info> ServerTournamentProdForWebBuild = new() {
            new Info("Production", SvTournamentProd, WssPort, true, PingMain),
        };

        private static readonly List<Info> ServerProdForMobileBuild = new() {
            new Info("Production", SvProd, WssPort, true, PingMain),
        };

        private static readonly List<Info> ServerProdForTelegramBuild = new() {
            new Info("Production", SvProdTelegram, WssPort, true, PingMain),
        };
        
        private static readonly List<Info> ServerProdForSolanaBuild = new() {
            new Info("Production", SvProdSolana, WssPort, true, PingMain),
        };
        
        private static readonly List<Info> ServerProdForWebAirdropBuild = new() {
            new Info("Production", SvProdWebAirdrop, WssPort, true, PingMain),
        };

        public static List<Info> TestServerAddresses {
            get {
                if (Application.isEditor) {
                    return Application.isMobilePlatform ? ServerTestForMobileEditor : ServerTestForWebEditor;
                }
                return Application.isMobilePlatform ? ServerTestForMobileBuild : ServerTestForWebBuild;
            }
        }

        public static List<Info> ProdServerAddresses =>
            Application.isMobilePlatform ? ServerProdForMobileBuild : ServerProdForWebBuild;

        public static List<Info> TournamentProServerAddress =>
            Application.isMobilePlatform ? ServerProdForMobileBuild : ServerTournamentProdForWebBuild;

        public static List<Info> TelegramProdServerAddress => ServerProdForTelegramBuild;
        public static List<Info> SolanaProdServerAddress => ServerProdForSolanaBuild;
        public static List<Info> WebAirdropProdServerAddress => ServerProdForWebAirdropBuild;

        /**
         * Main Test & Prod server
         */
        public static bool IsMainServerAddress(string serverAddress) {
            return ServerAddressConfig.ServerWithProdConfig.Contains(serverAddress);
        }

        [CanBeNull]
        public static Info GetServerInfo(string serverAddress) {
            var isMain = IsMainServerAddress(serverAddress);
            if (isMain) {
                List<Info> prodServerAddress;
                if (AppConfig.IsTournament()) {
                    prodServerAddress = TournamentProServerAddress;
                } else if (AppConfig.IsTon()) {
                    prodServerAddress = TelegramProdServerAddress;
                } else if(AppConfig.IsSolana()) {
                    prodServerAddress = SolanaProdServerAddress;
                    
                }else {
                    prodServerAddress = ProdServerAddresses;
                }

                return prodServerAddress.FirstOrDefault(e => e.Address == serverAddress) ??
                       TestServerAddresses.FirstOrDefault(e => e.Address == serverAddress);
            }
            return TestServerAddresses.FirstOrDefault(e => e.Address == serverAddress);
        }

        private static class ServerAddressConfig {
            public static readonly HashSet<string> ServerWithProdConfig = new();
        }

        [Serializable]
        public class Info {
            public readonly string Name;
            public readonly string Address;
            public readonly string PingServerAddress;

            public readonly int Port;

            public bool IsEncrypted => Port == WssPort;

            // For JSON parse
            [JsonConstructor]
            public Info(string name, string address, int port) {
                Name = name;
                PingServerAddress = Address = address;
                Port = port;
            }

            // For create config
            public Info(string name, string address, int port, bool useProdConfig, string pingServerAddress)
                : this(name, address, port) {
                PingServerAddress = pingServerAddress;
                if (useProdConfig) {
                    ServerAddressConfig.ServerWithProdConfig.Add(address);
                }
            }
        }
    }
}