using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using App;

using Senspark;

namespace Services.UserLoader {
    public class TonUserLoader :IUserLoader {
        public  List<(string, Func<Task>)> GetLoads() {
            var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            return new List<(string, Func<Task>)> {
                
                ("Get Task Config", ()=> serverManager.UserTonManager.GetTaskTonDataConfig()),
                ("Get BHero and BHouse Details", () => serverManager.General.GetTreasureHuntDataConfig()),
                ("Sync Map Data", () => serverManager.Pve.GetMapDetails()),
                ("Add hero trial for TON user", () => serverManager.General.AddHeroForAirdropUser()),
                ("Sync BHouse Details", () => serverManager.General.SyncHouse()),
                ("Sync Auto Mine Data", () => serverManager.General.GetAutoMinePrice()),
                ("Get Rent House Pack Config", () => serverManager.General.GetRentHousePackageConfig()),
                ("Get Leaderboard Config", ()=> serverManager.Pvp.GetCoinLeaderboardConfig()),
                ("Get Ranking", ()=> serverManager.Pvp.GetCoinRanking()),
            };
        }

    }
}