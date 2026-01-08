using App;

using Sfs2X.Entities.Data;

namespace Server.Models {
    public class InvestedDetail : IInvestedDetail {
        public float Invested { get; }
        public float Mined { get; }
        public float Roi { get; }
        public float Reward { get; }

        public InvestedDetail(ISFSObject data) {
            Invested = data.GetFloat("invested");
            Mined = data.GetFloat("mined");
            if (Invested > 0) {
                Roi = Mined / Invested;
            }
            Reward = data.GetFloat("rewards");
        }
    }
}