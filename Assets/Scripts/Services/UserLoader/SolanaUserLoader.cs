using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using App;

using Senspark;

namespace Services.UserLoader {
    public class SolanaUserLoader  :IUserLoader {
        public  List<(string, Func<Task>)> GetLoads() {
            var userSolManager = ServiceLocator.Instance.Resolve<IServerManager>().UserSolanaManager;

            return new List<(string, Func<Task>)> {
                
                ("Get BHero and BHouse Details", () => userSolManager.GetTreasureHuntDataConfigSol()),
                ("Sync Map Data", () => userSolManager.GetMapDetailsSol()),
                ("Add hero", () => userSolManager.AddHeroForSolUser()),
                ("Sync BHouse Details", () => userSolManager.SyncHouseSol()),
                ("Sync Auto Mine Data", () => userSolManager.GetAutoMinePriceSol()),
                ("Get Leaderboard Config", ()=> userSolManager.GetCoinLeaderboardConfigSol()),
                ("Get Ranking", () => userSolManager.GetCoinRankingSol()),
                ("Get Rent House Pack Config", () => userSolManager.GetRentHousePackageConfig()),
            };
        }

    }
}