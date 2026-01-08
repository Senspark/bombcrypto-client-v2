using System;

namespace Game.UI.Information {
    public enum BasicInformationTabType {
        Rename, 
        ResetRoi, 
        Stake, 
        AutoMine, 
        //DevHoang: Add new airdrop
        AutoMineTon, 
        AutoMineSol, 
        AutoMineRon, 
        AutoMineBas, 
        AutoMineVic
    }

    [Serializable]
    public class InformationData {
        public string displayName;
        public string[] code;
        public string content;
        public string network;
    }
}