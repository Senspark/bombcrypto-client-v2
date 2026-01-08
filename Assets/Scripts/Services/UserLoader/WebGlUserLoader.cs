using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App;
using Senspark;

namespace Services.UserLoader {
    public class WebGlUserLoader: IUserLoader {
        public  List<(string, Func<Task>)> GetLoads() {
            var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            return new List<(string, Func<Task>)> {
                ("Get BHero and BHouse Details", () => serverManager.General.GetTreasureHuntDataConfig()),
                ("Get Min Stake Hero", () => serverManager.General.GetMinStakeHero()),
                ("Get Repair Shield Config", () => serverManager.General.GetRepairShieldConfig()),
                ("Get Rock Pack Config", () => serverManager.General.GetRockPackConfig()),
                ("Get Burn Hero Config", () => serverManager.General.GetBurnHeroConfig()),
                ("Get Upgrade Shield Config", () => serverManager.General.GetUpgradeShieldConfig()),
                ("Sync BHouse Details", () => serverManager.General.SyncHouse()),
                ("Sync BHero Details", () => serverManager.General.SyncHero(false)),
                ("Sync BHero Data", () => serverManager.General.GetHeroPower()),
                ("Sync Auto Mine Data", () => serverManager.General.GetAutoMinePrice()),
                ("Sync Map Data", () => serverManager.Pve.GetMapDetails()),
            };
        }

    }
}