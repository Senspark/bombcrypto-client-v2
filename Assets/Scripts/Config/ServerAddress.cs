using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace App {
    public static class ServerAddress {
        private static string SvTestV1 => AppConfig.ServerAddresses?.svTestV1;
        private static string SvTestV1Tcp => AppConfig.ServerAddresses?.svTestV1Tcp;
        private static string SvTestV2 => AppConfig.ServerAddresses?.svTestV2;

        private static string SvProd => AppConfig.ServerAddresses?.svProd;
        private static string SvProdTcp => AppConfig.ServerAddresses?.svProdTcp;
        private static string SvTournamentProd => AppConfig.ServerAddresses?.svTournamentProd;
        private static string SvProdTelegram => AppConfig.ServerAddresses?.svProdTelegram;
        private static string SvProdSolana => AppConfig.ServerAddresses?.svProdSolana;
        private static string SvProdWebAirdrop => AppConfig.ServerAddresses?.svProdWebAirdrop;

        private const string SvLocal = "localhost";

        public const int WsPort = 8080;
        public const int WssPort = 8443;
        public const int TcpPort = 9933;

        // webBuild address comes from AppConfig; port is inferred (localhost/127.0.0.1 -> ws, domain -> wss).
        private static readonly List<Info> ServerTestForWebBuild = BuildWebBuildTestList(AppConfig.ServerAddresses?.testServers?.webBuild);

        private static List<Info> BuildWebBuildTestList([CanBeNull] string address) {
            if (string.IsNullOrEmpty(address)) return new List<Info>();
            var isLocal = address is "localhost" or "127.0.0.1";
            return new List<Info> { new Info("Test", address, isLocal ? WsPort : WssPort, false) };
        }

        private static readonly List<Info> ServerTestForWebEditor = new() {
            // new Info("Production", SvProdTcp, TcpPort, true),
            // new Info("Test", SvTestV1, WssPort, false),
            // new Info("Test 2", SvTestV2, WssPort, false),
            // new Info("___ ws", SvLocal, WsPort, false),
            new Info("___ tcp", SvLocal, TcpPort, false),
        };

        private static readonly List<Info> ServerTestForMobileBuild = new() {
            new Info("Production", SvProdTcp, TcpPort, true),
            new Info("Test", SvTestV1, WssPort, false),
            new Info("Test 2", SvTestV2, WssPort, false),
        };

        private static readonly List<Info> ServerTestForMobileEditor = new() {
            new Info("Production", SvProdTcp, TcpPort, true),
            new Info("Test", SvTestV1Tcp, TcpPort, false),
            new Info("___ tcp", SvLocal, TcpPort, false),
        };

        private static readonly List<Info> ServerProdForWebBuild = new() {
            new Info("Production", SvProd, WssPort, true),
        };

        private static readonly List<Info> ServerTournamentProdForWebBuild = new() {
            new Info("Production", SvTournamentProd, WssPort, true),
        };

        private static readonly List<Info> ServerProdForMobileBuild = new() {
            new Info("Production", SvProd, WssPort, true),
        };

        private static readonly List<Info> ServerProdForTelegramBuild = new() {
            new Info("Production", SvProdTelegram, WssPort, true),
        };

        private static readonly List<Info> ServerProdForSolanaBuild = new() {
            new Info("Production", SvProdSolana, WssPort, true),
        };

        private static readonly List<Info> ServerProdForWebAirdropBuild = new() {
            new Info("Production", SvProdWebAirdrop, WssPort, true),
        };

        public static List<Info> TestServerAddresses {
            get {
                if (AppConfig.IsEditor) {
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
            public readonly int Port;

            public bool IsEncrypted => Port == WssPort;

            // For JSON parse
            [JsonConstructor]
            public Info(string name, string address, int port) {
                Name = name;
                Address = address;
                Port = port;
            }

            // For create config
            public Info(string name, string address, int port, bool useProdConfig)
                : this(name, address, port) {
                if (useProdConfig) {
                    ServerAddressConfig.ServerWithProdConfig.Add(address);
                }
            }
        }
    }
}