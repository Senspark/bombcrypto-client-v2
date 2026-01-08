using App;

using Server.Models;

using Sfs2X.Entities.Data;

namespace Scenes.TreasureModeScene.Scripts.Solana.ServerSolDeclaration {
    public class BuyHeroSolResponse : IBuyHeroServerResponse {
        public IHeroDetails[] Details { get; }

        public BuyHeroSolResponse(ISFSObject data) {
            Details = HeroDetails.ParseArray(data);
        }
    }
}