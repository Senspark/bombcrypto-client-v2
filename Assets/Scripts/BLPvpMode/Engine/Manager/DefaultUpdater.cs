using BLPvpMode.Manager.Api;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Manager {
    public class DefaultUpdater : IUpdater {
        [NotNull]
        private readonly IUpdater[] _items;

        public DefaultUpdater(
            [NotNull] IUpdater[] items
        ) {
            _items = items;
        }

        public void Step(int delta) {
            _items.ForEach(it => { //
                it.Step(delta);
            });
        }
    }
}