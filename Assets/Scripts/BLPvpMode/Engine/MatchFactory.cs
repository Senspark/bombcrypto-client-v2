using BLPvpMode.Engine.Info;
using BLPvpMode.Engine.Manager;
using BLPvpMode.Manager;

using JetBrains.Annotations;

using Senspark;

namespace BLPvpMode.Engine {
    public class MatchFactory : IMatchFactory {
        [NotNull]
        private readonly ILogManager _logger;

        [NotNull]
        private readonly IMapGenerator _mapGenerator;

        [NotNull]
        private readonly IMatchManager _matchManager;

        [NotNull]
        private readonly IMessageBridge _messageBridge;

        [NotNull]
        private readonly IMatchInfo _observerInfo;

        [NotNull]
        private readonly ITimeManager _timeManager;

        [NotNull]
        private readonly IScheduler _scheduler;

        public IMatchController Controller { get; private set; } = null!; // Late init.

        public MatchFactory(
            [NotNull] ILogManager logger,
            [NotNull] IMapGenerator mapGenerator,
            [NotNull] IMatchManager matchManager,
            [NotNull] IMessageBridge messageBridge,
            [NotNull] IMatchInfo observerInfo
        ) {
            _logger = logger;
            _mapGenerator = mapGenerator;
            _matchManager = matchManager;
            _messageBridge = messageBridge;
            _observerInfo = observerInfo;
            _timeManager = new EpochTimeManager();
            _scheduler = new DefaultScheduler();
        }

        public void Initialize() {
            var controller = new DefaultMatchController(
                this,
                _observerInfo,
                _timeManager.Timestamp,
                _logger,
                _scheduler,
                _matchManager,
                _messageBridge,
                _mapGenerator
            );
            controller.Initialize();
            Controller = controller;
        }

        public void Destroy() {
            _scheduler.ClearAll();
        }
    }
}