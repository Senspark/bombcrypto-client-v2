using System;
using System.Threading.Tasks;

using BLPvpMode.Queue;

using Senspark;

namespace BLPvpMode.Manager {
    public enum PvpJoinExceptionType {
        CancelFinding
    }

    public class PvpJoinException : Exception {
        public readonly PvpJoinExceptionType Result;

        public PvpJoinException(PvpJoinExceptionType result, string message) : base(message) {
            Result = result;
        }
    }

    public enum JoinStatus {
        None,
        Joining,
        InQueue
    }

    public class PvpJoinObserver {
        public System.Action<JoinStatus> ChangeJoinStatus;
    }

    public interface IPvpJoinManager : IObserverManager<PvpJoinObserver> {
        JoinStatus JoinStatus { get; }
        Task<IFindMatchResult[]> FindMatch(global::BLPvpMode.Engine.Info.PvpMode modem, string matchId);
        Task CancelFinding();
        void Destroy();
    }
}