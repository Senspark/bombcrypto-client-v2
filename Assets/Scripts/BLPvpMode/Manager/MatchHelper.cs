using System.Linq;
using System.Threading.Tasks;

using BLPvpMode.Data;
using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.User;

using JetBrains.Annotations;

using PvpMode.UI;

using PvpSchedule;

using Scenes.MainMenuScene.Scripts;
using Scenes.PvpModeScene.Scripts;
using Scenes.PvpReadyScene.Scripts;

using Senspark;

using Share.Scripts.Utils;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace BLPvpMode.Manager {
    public class MatchHelper {
        [NotNull]
        private readonly IMatchFinder _matchFinder;

        [NotNull]
        private readonly IUserFactory _userFactory;

        public MatchHelper(
            [NotNull] IMatchFinder matchFinder,
            [NotNull] IUserFactory userFactory
        ) {
            _matchFinder = matchFinder;
            _userFactory = userFactory;
        }

        public async Task Start(string matchId = null) {
            var infoList = await _matchFinder.Find();
            var (users, modules) = await _userFactory.Create(infoList);
            var handle = new ObserverHandle();
            try {
                //var sceneLoader = ServiceLocator.Instance.Resolve<ISceneManager>();
                var startReadyTcs = new TaskCompletionSource<object>();
                var startRoundTcs = new TaskCompletionSource<IMatchStartData>();
                var startReadyCounter = 0;
                var startRoundCounter = 0;
                await Task.WhenAll(users.Select(async user => {
                    handle.AddObserver(user, new UserObserver {
                        OnStartReady = () => {
                            ++startReadyCounter;
                            if (startReadyCounter == users.Length) {
                                startReadyTcs.SetResult(null);
                            }
                        },
                        OnStartRound = data => {
                            ++startRoundCounter;
                            if (startRoundCounter == users.Length) {
                                startRoundTcs.SetResult(data);
                            }
                        },
                    });
                    await user.Connect();
                }));
                var task = await Task.WhenAny(startReadyTcs.Task, startRoundTcs.Task);
                if (task == startReadyTcs.Task) {
                    void OnLoaded(GameObject obj) {
                        var pvpReady = obj.GetComponent<PvpReadyScene>();
                        pvpReady.SetInfo(users, modules);
                        if (!string.IsNullOrEmpty(matchId)) {
                            TournamentUtils.InsertToJoinedList(matchId);
                        }
                    }
                    const string sceneName = "PvpReadyScene";
                    await SceneLoader.LoadSceneAsync(sceneName, OnLoaded);
                    return;
                }
                if (task == startRoundTcs.Task) {
                    var data = await startRoundTcs.Task;
                    void OnLoaded(GameObject obj) {
                        var pvpScene = obj.GetComponent<BLLevelScenePvp>();
                        pvpScene.IsCountDownEnabled = false;
                        pvpScene.SetPvpMapDetails(
                            users,
                            modules,
                            data.Match,
                            data.Map,
                            new BoosterStatus(0)
                        );
                    }
                    const string sceneName = "PvpModeScene";
                    await SceneLoader.LoadSceneAsync(sceneName, OnLoaded);
                    return;
                }
                Assert.IsTrue(false, "Unexpected match status");
            } finally {
                handle.Dispose();
            }
        }
    }
}