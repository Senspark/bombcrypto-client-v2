using System;
using System.Threading.Tasks;

namespace BLPvpMode.Manager.Api {
    public interface ILagSimulator {
        Task Process(Func<Task> action);
        Task<T> Process<T>(Func<Task<T>> action);
    }
}