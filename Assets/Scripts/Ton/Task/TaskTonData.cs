using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Sfs2X.Entities.Data;

using UnityEngine;

public static class TaskData {
    #region BombCrypto

    public const int Buy1Hero = 1;
    public const int Buy5Hero = 2;
    public const int Buy15Hero = 3;
    public const int BuyHouse = 4;
    public const int FollowBombXPage = 5;
    public const int JoinBombDiscord = 6;
    public const int FollowBombTelegram = 7;
    public const int FollowBombSubStack = 8;
    public const int FollowBombTiTok = 9;

    public const string BombXPageLink = "https://x.com/BombCryptoGame";
    public const string BombDiscordLink = "https://discord.com/invite/bombcryptoofficial";
    public const string BomTelegramLink = "https://t.me/BombCryptoGroup";
    public const string BombSubStackLink = "https://bombcrypto.substack.com/";
    public const string BombTikTokLink = "https://www.tiktok.com/@bombcrypto_official?";

    #endregion

    #region Easy Cake

    public const int PlayEasyCake = 10;
    public const int FollowEasyCakeTelegram = 11;
    public const int SubscribeEasyCakeYoutube = 12;

    public const string EasyCakeBotLink = "http://t.me/EasyCakeTapbot";
    public const string EasyCakeTelegramLink = "https://t.me/EasycakeAnn";
    public const string EasyCakeYoutubeLink = "https://www.youtube.com/@EasyCake_youtube";

    #endregion

    #region Psy Duck

    public const int PlayPsyDuck = 13;
    public const int FollowPsyDuckXPage = 14;
    public const int FollowPsyDuckTelegram = 15;
    public const int SubscribeSpyDuckYoutube = 16;

    public const string PsyDuckBotLink = "https://t.me/psyduckgamexyzbot";
    public const string PsyDuckXPageLink = "https://x.com/psyduckgame_xyz";
    public const string PsyDuckTelegramLink = "https://t.me/psyduckgamexyz";
    public const string PsyDuckYoutubeLink = "https://www.youtube.com/@psyduckgamexyz";

    #endregion

    #region MemeTd

    public const int PlayMemeTd = 17;
    public const int FollowMemeTdTelegram = 18;
    public const int FollowMemeTdXPage = 19;

    public const string MemeTdBotLink = "https://t.me/memetd_bot/game?startapp=r_1100633165";
    public const string MemeTdTelegramLink = "http://t.me/memetd_official";
    public const string MemeTdXPageLink = "https://x.com/MemeTD_official";

    #endregion

    #region Snap Fly

    public const int PlaySnapFy = 20;
    public const int FollowSnapFlyTelegram = 21;
    public const int FollowSnapFlyXPage = 22;

    public const string SnapFlyBotLink = "https://t.me/snapfly_game_bot?start=GtDkhTXhzp";
    public const string SnapFlyTelegramLink = "https://t.me/SnapFly_updates";
    public const string SnapFlyXPageLink = "https://x.com/SnapFly_xyz";

    #endregion

    #region Money Garden AI

    public const int PlayMoneyGarden = 23;
    public const int FollowMoneyGardenTelegram = 24;
    public const int FollowMoneyGardenXPage = 25;

    public const string MoneyGardenBotLink = "https://t.me/moneygardenaibot";
    public const string MoneyGardenTelegramLink = "https://t.me/+oO3_-Dr6fzwwM2M1";
    public const string MoneyGardenXPageLink = "https://x.com/moneygardenai";

    #endregion

    #region Shark Attack

    public const int PlaySharkAttack = 26;
    public const int JoinSharkAttackTelegram = 27;

    public const string SharkAttackBotLink = "https://t.me/shark_attack_io_bot/shark_attack?startapp=1680024053";
    public const string SharkAttackTelegramLink = "https://t.me/sharkattack_channel";

    #endregion

    #region BFB Sport

    public const int PlayBFBSport = 28;
    public const int JoinBFBSportChannel = 29;

    public const string BFBSportBotLink = "https://t.me/BTCFootballBot/min?startapp=138152";
    public const string BFBSportChanelLink = "https://t.me/BFBSPORT/1";

    #endregion

    #region Shiok

    public const int PlayShiok = 30;
    public const int FollowShiokTelegram = 31;
    public const int FollowShiokXPage = 32;

    public const string ShiokBotLink = "https://t.me/the_shiok_bot";
    public const string ShiokTelegramLink = "https://t.me/shiok_ann";
    public const string ShiokXPageLink = "https://x.com/shiok_fun";

    #endregion

    #region Pirate Kings

    public const int PlayPirateKings = 33;
    public const string PirateKingsBotLink = "https://t.me/TONPirateKings_bot?start=2078591740";

    #endregion

    #region Fish War

    public const int PlayFishWar = 34;
    public const int FollowFishWarTelegram = 35;
    public const int FollowFishWarXPage = 36;

    public const string FishWarBotLink = "https://t.me/FishWarOceanKingBot?start=JmlkPUg5U1E3MUExN1g==";
    public const string FishWarTelegramLink = "https://t.me/Fishwar_io";
    public const string FishWarXPageLink = "https://x.com/FishWarOfficial";

    #endregion
}

public enum TaskCategory {
    BombCrypto,
    EasyCake,
    PsyDuck,
    MemeTd,
    SnapFly,
    MoneyGarden,
    SharkAttack,
    BfbSport,
    Shiok,
    PirateKings,
    FishWar
}

public interface ICategoryTonData {
    public int TaskCategory { get; }
    public string Name { get; }
    public Sprite Icon { get; }
    public bool IsNew { get; }
}

public class CategoryTonData : ICategoryTonData {
    public int TaskCategory { get; }
    public string Name { get; }
    public Sprite Icon { get; }
    public bool IsNew { get; }

    public CategoryTonData(int taskCategory, string name, Sprite icon) {
        TaskCategory = taskCategory;
        Name = name;
        Icon = icon;
    }
    
    public CategoryTonData(CategoryTonDataJson data, Sprite icon) {
        TaskCategory = data.CateGory;
        Name = data.Name;
        Icon = icon;
        IsNew = data.IsNew;
    }
}


public class TaskTonDataJson {
    [JsonProperty("task_id")]
    public int TaskId { get; set; }

    [JsonProperty("cat_id")]
    public int CateGory { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("link")]
    public string Link { get; set; }
}

public class CategoryTonDataJson {
    [JsonProperty("cat_id")]
    public int CateGory { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("icon")]
    public string Icon { get; set; }
    
    [JsonProperty("is_new")]
    public bool IsNew { get; set; } = false;
    
    [JsonProperty("is_icon_changed")]
    public bool IsIconChanged { get; set; } = false;
}
public interface ITaskTonData {
    public int TaskCategory { get; }
    public string Name { get; }
    public int Id { get; }
    public string Link { get; }
    public int Amount { get; }
    public bool IsClaimed { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsValidTask { get; }
    void UpdateData(ISFSObject data);
}

public class TaskTonData : ITaskTonData {
    public int TaskCategory { get; private set; }
    public string Name { get; private set; }
    public int Id { get; private set; }
    public string Link { get; private set; }
    public int Amount { get; private set; }
    public bool IsClaimed { get; set; }
    public bool IsCompleted { get; set; }

    // public TaskTonData(ISFSObject data) {
    //     Id = data.GetInt("id");
    //     Amount = data.GetInt("reward");
    //     IsClaimed = data.GetBool("is_claimed");
    //     IsCompleted = data.GetBool("is_completed");
    // }
    //
    // public void UpdateData(TaskTonDataJson data) {
    //     Name = data.Name;
    //     TaskCategory = data.CateGory;
    // }
    
    public TaskTonData(TaskTonDataJson data) {
        Id = data.TaskId;
        Name = data.Name;
        TaskCategory = data.CateGory;
        Link = data.Link;
    }
    //Kiểm tra xem task này có đc server trả về ko, == 0 => server ko trả về, bỏ task này
    public bool IsValidTask { get; private set; }

    public void UpdateData(ISFSObject data) {
        Amount = data.GetInt("reward");
        IsClaimed = data.GetBool("is_claimed");
        IsCompleted = data.GetBool("is_completed");
        IsValidTask = true;
    }



    private int GetTaskCategory(int id) {
        var categoryMap = new Dictionary<(int start, int end), int> {
            { (1, 9), 1},
            { (10, 12), 2 },
            { (13, 16), 3 },
            { (17, 19), 4 },
            { (20, 22), 5 },
            { (23, 25), 6 },
            { (26, 27), 7 },
            { (28, 29), 8 },
            { (30, 32), 9 },
            { (33, 33), 10 },
            { (34, 36), 11 }
        };

        foreach (var entry in categoryMap) {
            if (id >= entry.Key.start && id <= entry.Key.end) {
                return entry.Value;
            }
        }
        
        return 1;
    }
    //For test
    public TaskTonData(int id, string name, int taskCategory) {
        Id = id;
        TaskCategory = taskCategory;
    }
}

//For test only
public class TestTonTask {
    public Dictionary<int, TaskTonData> GetFakeData() {
        return new Dictionary<int, TaskTonData>() {
            { 1, new TaskTonData(1, "Buy 1 Hero", 1) },
            { 2, new TaskTonData(2, "Buy 5 Heroes", 2) },
            { 3, new TaskTonData(3, "Go to Easy Cake", 3) },
            { 4, new TaskTonData(4, "Go to PsyDuck", 4) },
            { 5, new TaskTonData(5, "Go to MemeTd", 5) },
            { 6, new TaskTonData(6, "Go to SnapFly", 6) },
            { 7, new TaskTonData(7, "Go to MoneyGarden", 7) }
        };
    }

    // public List<ITaskTonData> GetFakeDataList() {
    //     return new List<ITaskTonData>() {
    //         { new TaskTonData(1, "Buy 1 Hero", 1) },
    //         { new TaskTonData(2, "Buy 5 Heroes", TaskCategory.BombCrypto) },
    //         { new TaskTonData(3, "Go to Easy Cake", TaskCategory.EasyCake) },
    //         { new TaskTonData(4, "Go to PsyDuck", TaskCategory.PsyDuck) },
    //         { new TaskTonData(5, "Go to MemeTd", TaskCategory.MemeTd) },
    //         { new TaskTonData(6, "Test1", TaskCategory.PirateKings) },
    //         { new TaskTonData(7, "Test2", TaskCategory.PirateKings) },
    //         { new TaskTonData(8, "ABC123", TaskCategory.SharkAttack) },
    //         { new TaskTonData(9, "Shark shark shark", TaskCategory.SharkAttack) },
    //         { new TaskTonData(10, "Hungry shark", TaskCategory.SharkAttack) }
    //     };
    // }
}