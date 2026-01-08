using System;

using Analytics;

using App;

using Cysharp.Threading.Tasks;

using Data;

using Senspark;

using PvpMode.Manager;

using Services;

using UnityEngine;

using AudioPlayer = App.AudioPlayer;
using DefaultDataManager = App.DefaultDataManager;
using IBoosterManager = PvpMode.Manager.IBoosterManager;

namespace BLPvpMode.Test {
    public class BLServiceFake : MonoBehaviour {
        [SerializeField]
        private GameObject[] inGameEnable;

        [SerializeField]
        protected TextAsset textEarlyConfig;

        [SerializeField]
        protected GameObject audioPlayer;

        private void Awake() {
            UniTask.Void(async () => {
                var earlyConfigManager = TryResolve<IEarlyConfigManager>((() => {
                    var r = new EarlyConfigManager(null);
                    r.Initialize(textEarlyConfig.text);
                    return r;
                }));

                var logManager = TryResolve<ILogManager>(() => {
                    var r = new DefaultLogManager(true);
                    return r;
                });

                var bm = TryResolve<IBoosterManager>(() => {
                    var r = new DefaultBoosterManager();
                    var boosters = new BoosterData[] {
                        new BoosterData(DefaultBoosterManager.ConvertFromEnum(BoosterType.Shield), 0, "des shield", 0,
                            0),
                        new BoosterData(DefaultBoosterManager.ConvertFromEnum(BoosterType.Key), 0, "des key", 0, 0)
                    };
                    r.SetBoosterData(boosters);
                    return r;
                });

                var serverManager = TryResolve<IServerManager>(() => new TestServerManagerPvp());

                TryResolve<IStorageManager>(() => {
                    var dataManager = new DefaultDataManager(new LocalDataStorage());
                    var r = new DefaultStoreManager(dataManager);
                    r.Initialize();
                    return r;
                });

                TryResolve<IPveModeManager>(() => new DefaultPveModeManager(NetworkType.Polygon));
                TryResolve<IHeroStatsManager>(() => new HeroStatsManager(logManager, earlyConfigManager));
                TryResolve(() => new HeroIdManager(earlyConfigManager));

                TryResolve<IAnalytics>(() => new NullAnalytics(logManager));
                TryResolve<IBLTutorialManager>(() => new BLTutorialManager());
                TryResolve<IProductItemManager>(() => new ProductItemManager(earlyConfigManager));
                TryResolve<IItemUseDurationManager>(() => new ItemUseDurationManager(logManager));
                if (AudioPlayer.Instance == null) {
                    Instantiate(audioPlayer);
                    TryResolve<ISoundManager>(() => new NullSoundManager());
                }

                await serverManager.Initialize();
                foreach (var obj in inGameEnable) {
                    obj.gameObject.SetActive(true);
                }
            });
        }

        private static T TryResolve<T>(Func<T> createService) {
            try {
                var result = ServiceLocator.Instance.Resolve<T>();
                if(result == null)
                    throw new Exception();
                return result;
            } catch (Exception e) {
                var r = createService();
                ServiceLocator.Instance.Provide(r);
                return r;
            }
        }
    }
}