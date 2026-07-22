using App;

namespace Game.ConnectControl {
    public readonly struct ModeResult {
        public readonly bool CanTreasure;
        public readonly bool CanAdventure;
        public readonly LandingMode Landing;

        public ModeResult(bool canTreasure, bool canAdventure, LandingMode landing) {
            CanTreasure = canTreasure;
            CanAdventure = canAdventure;
            Landing = landing;
        }
    }

    // Access matrix (docs/login-flow-account-type.md):
    public static class ModeAccess {
        public static ModeResult Resolve(bool isUserFi, bool isTournament, bool isAirDrop, LandingMode requested) {
            if (isAirDrop) {
                return new ModeResult(true, false, LandingMode.Treasure);
            }
            if (isTournament) {
                return new ModeResult(false, true, LandingMode.Adventure);
            }
            if (!isUserFi) {
                return new ModeResult(false, true, LandingMode.Adventure);
            }
            var landing = requested == LandingMode.Adventure ? LandingMode.Adventure : LandingMode.Treasure;
            return new ModeResult(true, true, landing);
        }
    }
}
