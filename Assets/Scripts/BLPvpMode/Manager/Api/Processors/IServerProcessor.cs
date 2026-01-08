using System.Threading.Tasks;

using JetBrains.Annotations;

namespace BLPvpMode.Manager.Api.Processors {
    public interface IServerProcessorVoid {
        [MustUseReturnValue]
        [NotNull]
        Task Process([NotNull] IServerHandlerVoid handlerVoid);
    }

    public interface IServerProcessorVoidT {
        [MustUseReturnValue]
        [NotNull]
        Task Process<T>([NotNull] IServerHandlerVoid<T> handler, T arg);
    }

    public interface IServerProcessor {
        [ItemNotNull]
        [MustUseReturnValue]
        [NotNull]
        Task<R> Process<R>([NotNull] IServerHandler<R> handler);
    }

    public interface IServerProcessorT {
        [ItemNotNull]
        [MustUseReturnValue]
        [NotNull]
        Task<R> Process<R, T>([NotNull] IServerHandler<R, T> handler, T arg);
    }
}