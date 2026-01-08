using System.Threading.Tasks;

using BLPvpMode.Engine.Info;

namespace BLPvpMode.Manager {
    public interface IMatchFinder {
        Task<IMatchInfo[]> Find();
    }
}