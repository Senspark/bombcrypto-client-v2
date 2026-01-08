using System.Linq;
using System.Threading.Tasks;

using BLPvpMode.Manager.Api;

using JetBrains.Annotations;

namespace Reconnect {
    public class MultiReconnectStrategy : IReconnectStrategy {
        [NotNull]
        private readonly IReconnectStrategy[] _items;

        public MultiReconnectStrategy([NotNull] params IReconnectStrategy[] items) {
            _items = items;
        }

        public void Dispose() {
            _items.ForEach(it => it.Dispose());
        }

        public async Task Start() {
            await Task.WhenAll(_items.Select(it => it.Start()));
        }
    }
}