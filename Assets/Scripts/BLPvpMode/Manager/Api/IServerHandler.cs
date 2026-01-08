using System.Threading.Tasks;

using JetBrains.Annotations;

namespace BLPvpMode.Manager.Api {
    public interface IServerHandlerVoid : IServerListener {
        [MustUseReturnValue]
        [NotNull]
        Task Start([NotNull] IServerBridge bridge);
    }

    public interface IServerHandlerVoid<in T> : IServerListener {
        [MustUseReturnValue]
        [NotNull]
        Task Start([NotNull] IServerBridge bridge, T arg);
    }

    public interface IServerHandler<R> : IServerListener {
        [ItemNotNull]
        [MustUseReturnValue]
        [NotNull]
        Task<R> Start([NotNull] IServerBridge bridge);
    }

    public interface IServerHandler<R, in T> : IServerListener {
        [ItemNotNull]
        [MustUseReturnValue]
        [NotNull]
        Task<R> Start([NotNull] IServerBridge bridge, T arg);
    }
}