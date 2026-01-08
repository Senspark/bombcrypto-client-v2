using System;

using App;

using Newtonsoft.Json;

using Server.Models;

using Sfs2X.Entities.Data;

namespace Scenes.TreasureModeScene.Scripts.Solana.Server_Response {
    public class SyncHeroServerResponse : ISyncHeroResponse {
        public IHeroDetails[] Details { get; }
        public int[] NewIds { get; }
        public int HeroesSize { get; }
        
        public SyncHeroServerResponse(ISFSObject data) {
            Details = HeroDetails.ParseArray(data);
            if (data.ContainsKey(SFSDefine.SFSField.NewBombers)) {
                var newIdsArray = data.GetSFSArray(SFSDefine.SFSField.NewBombers);
                var newIdsJson = newIdsArray.ToJson();
                NewIds = JsonConvert.DeserializeObject<int[]>(newIdsJson);    
            } else {
                NewIds = Array.Empty<int>();
            }
            if (data.ContainsKey(SFSDefine.SFSField.HeroesSize)) {
                HeroesSize = data.GetInt(SFSDefine.SFSField.HeroesSize);
            } else {
                HeroesSize = -1;
            }
        }
    }
}