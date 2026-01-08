using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;
using Game.Manager;

using Senspark;

using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Scenes.PvpModeScene.Scripts {
    public class DialogPvPDashboard : Dialog {
        public IServerManager ServerManager { get; private set; }

        [FormerlySerializedAs("_battleLayout")]
        [SerializeField]
        private Transform battleLayout;

        [FormerlySerializedAs("_nextBattle")]
        [SerializeField]
        private PvPNextBattle nextBattle;

        [FormerlySerializedAs("_time")]
        [SerializeField]
        private Text time;

        [FormerlySerializedAs("_userLayout")]
        [SerializeField]
        private Transform userLayout;

        public static UniTask<DialogPvPDashboard> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogPvPDashboard>();
        }
        
        protected override void Awake() {
            base.Awake();
            ServerManager = ServiceLocator.Instance.Resolve<IServerManager>();
            OnWillShow(UpdateUI);
        }

        public void OnButtonCloseClicked() {
            Hide();
        }

        private void UpdateUI() {
            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.OnDidShow(() => {
                UniTask.Void(async () => {
                    // FIXME: not used.
                    /*
                    var result = await ServerManager.Pvp.GetPvPQueueUser();
                    if (result.Code == 0) {
                        var ts = TimeSpan.FromMilliseconds(result.Time);
                        time.text = $"{ts.Hours}:{ts.Minutes}:{ts.Seconds}";
                        foreach (Transform it in userLayout) {
                            Destroy(it.gameObject);
                        }
                        foreach (var it in result.Users) {
                            Instantiate(Resources.Load<PvPQueueUser>("Prefabs/Dialog/PvPQueueUser"), userLayout)
                                .UpdateUI(TimeSpan.FromMilliseconds(it.JoiningTime), it.Point,
                                    it.WalletAddress);
                        }
                        nextBattle.UpdateUI(TimeSpan.FromMilliseconds(result.Battle.Time), result.Battle.Users);
                        foreach (Transform it in battleLayout) {
                            Destroy(it.gameObject);
                        }
                        foreach (var it in result.Battles) {
                            Instantiate(Resources.Load<PvPBattle>("Prefabs/Dialog/PvPBattle"), battleLayout)
                                .UpdateUI(it.BattleId, it.SeasonId, it.Users);
                        }
                    } else {
                        waiting.OnDidHide(() => DialogError.ShowError(Canvas, result.Message));
                    }
                    */
                    waiting.End();
                });
            });
            waiting.Begin();
        }
    }
}