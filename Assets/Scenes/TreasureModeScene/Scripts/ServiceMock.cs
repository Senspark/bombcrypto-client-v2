using System;
using System.Collections.Generic;

using Analytics;

using App;

using Game.Manager;

using Scenes.TreasureModeScene.Scripts.Mocks;

using Senspark;

using Services;

using Share.Scripts.Services;

using Ton.Task;

using UnityEngine;

using IDataManager = App.IDataManager;

namespace Scenes.TreasureModeScene.Scripts {
    /**
     * Tự động Mock các service để test
     */
    public class ServiceMock : MonoBehaviour {
        public void Initialize() {
            if (ServiceUtils.IsInitialized) {
                return;
            }
            IServerManager serverManager = new NullServerManager();
            var services = new List<IService> {
                new DefaultLogManager(true),
                new DefaultDataManager(new LocalDataStorage()),
                new DefaultChestRewardManager(NetworkType.Ton),
                serverManager,
                new DefaultPveHeroStateManager(new DefaultLogManager(true), new NullServerManager()),
                new NullStorageManager(),
                new EditorFeatureManager(),
                new DefaultSoundManager(new DefaultDataManager(new LocalDataStorage())),
                new TonNetworkConfig(false),
                new TaskNone(),
                new DefaultFusionManager(),
                new NullHouseStorageManager(),
                new NullPlayerStorageManager(),
                new DefaultLanguageManager(),
                new NullAnalytics(new DefaultLogManager(false)),
                new NullAccountManager(),
                new NullLaunchPadManager(),
                new DefaultPveModeManager(NetworkType.Ton),
                new NullHeroStatsManager(),
                new NullHeroIdManager()
            };
            var sceneManager = new SceneManager();
            serverManager.Initialize();

            var serviceLocator = ServiceLocator.Instance;
            foreach (var service in services) {
                serviceLocator.Provide(service);
            }
            serviceLocator.Provide(sceneManager);
        }
    }
}