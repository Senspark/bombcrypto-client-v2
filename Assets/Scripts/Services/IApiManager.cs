using System.Collections.Generic;
using System.Threading.Tasks;

using Data;

using PvpSchedule.Models;

using Senspark;

namespace App {
    [Service(nameof(IApiManager))]
    public interface IApiManager : IService {
        string Domain { get; }
        Task<double> GetCoinBalance(string walletAddress);
        Task<bool> CheckServerTime();
        Task<long> RequestServerUnixTime();
        Task<(int, int)> GetCcu();
        Task<List<IPvpRoomInfo>> GetPvpRoomList();
        Task<List<IPvpMatchSchedule>> GetPvpMatches();
        Task<List<string>> GetMyMatches(string userName);
    }
}