using UnityEngine;

namespace Constant {
    public static class GameConstant {
        public const int MaxPriceInput = 9999;
        public const float RatioPriceShop = 3;
        public const int MinFuseQuantity = 4;
        public const bool EnableAdsPvp = true;
        public const bool AdventureRequestServer = false;
        public const bool EnableLuckyWheelPve = true;
        public const bool EnableLuckyWheelPvp = true;
        public static readonly bool MobilePlatform = Application.isMobilePlatform; // || Application.isEditor;

        public static readonly bool FiPlatform =
            Application.platform == RuntimePlatform.WebGLPlayer || Application.isEditor;

        public const int MatchPvpRequiredForNewUser = 0;
    }
}