using System;
using App;
using UnityEngine;

namespace Services.Rewards {
    [Serializable]
    public class TokenData {
        public int code;
        public int sortOrder;
        public string tokenName;
        public string displayName;
        public string iconName;
        public bool displayOnLaunchPad;
        public bool alwaysDisplay;
        public bool enableFarm;
        public bool enableClaim;
        public bool enableDeposit;
        public bool useTax;
        public TokenCondition claimFee;
        public float minValueToClaim;
        public Sprite icon;
        public string networkSymbol;
        public string networkSymbolDisplayName;
        public NetworkSymbol NetworkSymbol => new(networkSymbol);
    }
    
    [Serializable]
    public readonly struct NetworkSymbol {
        public readonly string Name;
        //DevHoang: Add new airdrop
        public static readonly NetworkSymbol TR = new("TR"); 
        public static readonly NetworkSymbol Bsc = new("BSC"); 
        public static readonly NetworkSymbol Polygon = new("POLYGON"); 
        public static readonly NetworkSymbol Ton = new("TON"); 
        public static readonly NetworkSymbol Sol = new("SOL"); 
        public static readonly NetworkSymbol Ron = new("RON"); 
        public static readonly NetworkSymbol Bas = new("BAS"); 
        public static readonly NetworkSymbol Vic = new("VIC"); 

        public NetworkSymbol(string symbol) {
            Name = symbol;
        }

        public static NetworkSymbol Convert(NetworkType type) {
            return type switch {
                //DevHoang: Add new airdrop
                NetworkType.Binance => Bsc,
                NetworkType.Polygon => Polygon,
                NetworkType.Ton => Ton,
                NetworkType.Solana => Sol,
                NetworkType.Ronin => Ron,
                NetworkType.Base => Bas,
                NetworkType.Viction => Vic,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public static bool operator ==(NetworkSymbol c1, NetworkSymbol c2) {
            return c1.Name.Equals(c2.Name);
        }
        
        public static bool operator !=(NetworkSymbol c1, NetworkSymbol c2) {
            return !c1.Name.Equals(c2.Name);
        }
        
        public static bool operator ==(NetworkSymbol c1, NetworkType c2) {
            return c1 == Convert(c2);
        }
        
        public static bool operator !=(NetworkSymbol c1, NetworkType c2) {
            return c1 != Convert(c2);
        }
        
        public static bool operator ==(NetworkSymbol c1, string c2) {
            return c1 == new NetworkSymbol(c2);
        }
        
        public static bool operator !=(NetworkSymbol c1, string c2) {
            return c1 != new NetworkSymbol(c2);
        }
        
        public bool Equals(NetworkSymbol other) {
            return Name == other.Name;
        }

        public override bool Equals(object obj) {
            return obj is NetworkSymbol other && Equals(other);
        }

        public override int GetHashCode() {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }

    [Serializable]
    public class TokenCondition {
        public string tokenName;
        public string comparer;
        public float value;

        public bool IsTrue(float compareValue) {
            return comparer switch {
                "==" => IsEqual(compareValue, value),
                ">" => IsGreaterThan(compareValue, value),
                ">=" => IsGreaterThanOrEqual(compareValue, value),
                "<" => IsLessThan(compareValue, value),
                "<=" => IsLessThanOrEqual(compareValue, value),
                _ => false
            };
        }

        public static bool IsEqual(float a, float b) => Math.Abs(a - b) < 0.0001f;
        public static bool IsLessThan(float a, float b) => a < b;
        public static bool IsLessThanOrEqual(float a, float b) => a <= b;
        public static bool IsGreaterThan(float a, float b) => a > b;
        public static bool IsGreaterThanOrEqual(float a, float b) => a >= b;
    }
}