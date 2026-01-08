using System;

using App;

public static partial class SFSDefine {
    public static string GetSlogans(EntryPoint e) {
        return e switch {
            EntryPoint.Login => "enter_game",
            EntryPoint.StartPvE => "start_mining",
            EntryPoint.StopPvE => "shutting_down",
            EntryPoint.GetStoryMap => "reveal_map",
            EntryPoint.EnterStoryDoor => "finish_game",
            _ => throw new ArgumentOutOfRangeException(nameof(e), e, null)
        };
    }

    public static class SFSField
    {
        public const string ErrorCode = "ec";
        public const string PLAYER_NAME = "pln";
        public const string TELEGRAM_USER_DATA = "telegram_user_data";
        public const string PASSWORD = "password";
        public const string LOGIN_TYPE = "lt";
        public const string PLATFORM = "platform";
        public const string SLOGAN = "slogan";
        public const string LOGIN_CODE = "lc";
        public const string SIGNATURE = "signature";
        public const string VERSION_CODE = "version_code";
        public const string ACTIVATION_CODE = "activation_code";
        public const string REFERRAL_CODE = "referral_code";

        public const string ID = "id";
        public const string Datas = "datas";

        public const string Bombers = "bombers";
        public const string NewBombers = "new_bombers";
        public const string HeroesSize = "heroes_size";
        public const string BLocks = "blocks";
        //public const string GenId = "gen_id";
        public const string Id = "id";
        public const string Enegy = "energy";
        public const string HouseGenId = "house_gen_id";
        public const string HouseId = "house_id";
        public const string active = "active";
        public const string stage = "stage";
        public const string AccountType = "account_type";
        public const string HeroType = "hero_type";
        public const string OldSeason = "old_season";

        public const string Houses = "houses";
        public const string NewHouses = "new_houses";

        public const string Type = "type";
        public const string NewUser = "new_user";
        public const string RequestId = "requestId";
        public const string AttendPools = "attend_pools";
        
        public const string NewRequestId = "rid";
        public const string Data = "data";
        public const string ErrorString = "es";
        public const string LoginTokenData = "lk";
        
        public const string Config = "config";
        public const string CurrentStep = "current_step";
    }
}