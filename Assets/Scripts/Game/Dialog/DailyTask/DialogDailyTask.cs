using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using App;

using Cysharp.Threading.Tasks;

using Data;

using Engine.Utils;

using Game.Dialog;
using Game.Dialog.BomberLand.BLGacha;
using Game.Manager;

using Scenes.ShopScene.Scripts;

using Senspark;

using Services;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public enum DailyTaskState {
    NotDone,
    Done,
    Claimed
}

namespace Scenes.MainMenuScene.Scripts {
    public class DialogDailyTask : Dialog {
        [SerializeField]
        private GameObject waitingDialog;

        [SerializeField]
        private Text resetTxt;

        [SerializeField]
        private Button redDotBtn;

        [SerializeField]
        private TextMeshProUGUI progressTxt;

        [SerializeField]
        private GameObject[] claimBtnStates;

        [SerializeField]
        private DailyTaskItem dailyTaskItem;

        [SerializeField]
        private Transform content;

        [SerializeField]
        private GameObject dialogDailyTaskInfo;

        [SerializeField]
        private GameObject dialogRewardInfo;

        [SerializeField]
        private Image rewardIcon;

        [SerializeField]
        private TextMeshProUGUI rewardTxt;

        [SerializeField]
        private Canvas canvas;

        [SerializeField]
        private DailyTaskConfig config;

        [SerializeField]
        private BLGachaRes resource;

        [SerializeField]
        private Button closeBtn;

        private List<DailyTaskItem> _taskItemList = new List<DailyTaskItem>();
        private ObserverHandle _handle;
        private ISoundManager _soundManager;
        private IDailyTaskManager _dailyTaskManager;

        private const string TASKS_PROGRESS = "#F9CD00";
        private const string TASKS_DONE = "#33FF00";

        public static UniTask<DialogDailyTask> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogDailyTask>();
        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _dailyTaskManager = ServiceLocator.Instance.Resolve<IDailyTaskManager>();
            dialogDailyTaskInfo.SetActive(false);
            dialogRewardInfo.SetActive(false);
            _handle = new ObserverHandle();
            _handle.AddObserver(_dailyTaskManager, new DailyTaskObserver() {
                updateRedDot = state => { redDotBtn.gameObject.SetActive(state); }
            });
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            _handle.Dispose();
        }

        private void Start() {
            waitingDialog.SetActive(true);
            closeBtn.interactable = false;
            UniTask.Void(async () => {
                try {
                    await _dailyTaskManager.GetDailyTaskConfig();
                    GetTaskList();
                    CheckOrderTask();
                    waitingDialog.SetActive(false);
                    closeBtn.interactable = true;
                } catch (Exception e) {
                    await DialogError.ShowError(canvas, "Failed to load daily tasks", Hide);
                }
            });
        }

        private void GetTaskList() {
            var taskList = _dailyTaskManager.GetTodayTaskList();
            for (var i = 0; i < taskList.Count; i++) {
                if (_taskItemList.Count < taskList.Count) {
                    var task = Instantiate(dailyTaskItem, content).GetComponent<DailyTaskItem>();
                    var taskGoAction = config.GetTaskGoActionById(taskList[i].taskId);
                    task.InitTask(taskList[i]);
                    task.InitActions((taskData) => {
                        UniTask.Void(async () => {
                            var result = await ClaimDailyTask(taskData.taskId);
                            task.UpdateClaimBtnState(DailyTaskState.Claimed, true);
                            var icon = await resource.GetSpriteByItemId(taskData.rewardId);
                            if (result) {
                                ShowReward(icon, taskData.rewardQuantity);
                            }
                            GetTaskList();
                        });
                    }, () => {
                        Hide();
                        taskGoAction?.Invoke();
                    });
                    _taskItemList.Add(task);
                    StartCoroutine(CountDownToMidnight());
                } else {
                    _taskItemList[i].InitTask(taskList[i]);
                }
            }
            CheckClaimTask();
        }

        private IEnumerator CountDownToMidnight() {
            var now = DateTime.UtcNow;
            var midnight = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(1);
            var totalSecondsLeft = (midnight - now).TotalSeconds;
            while (totalSecondsLeft >= 0) {
                resetTxt.text =
                    $"Daily Tasks list will reset in: {Epoch.GetTimeStringHourMinuteSecond(totalSecondsLeft)}";
                totalSecondsLeft--;
                yield return new WaitForSeconds(1f);
            }
            //DevHoang_20250408: Wait a little bit to make sure the server reset all daily tasks
            waitingDialog.SetActive(true);
            closeBtn.interactable = false;
            yield return new WaitForSeconds(3f);
            waitingDialog.SetActive(false);
            closeBtn.interactable = true;
            RefreshDailyTask();
        }

        private void RefreshDailyTask() {
            _dailyTaskManager.ResetDailyTaskConfig();
            _taskItemList.Clear();
            foreach (Transform child in content) {
                Destroy(child.gameObject);
            }
            Start();
        }

        private void ShowReward(Sprite icon, int quantity) {
            rewardIcon.sprite = icon;
            rewardTxt.SetText($"x{quantity}");
            EnableDialogRewardInfo(true);
        }

        private void CheckClaimTask() {
            var claimedTasks = 0;
            foreach (var task in _taskItemList) {
                if (task.GetTaskState() == DailyTaskState.Claimed) {
                    claimedTasks++;
                }
            }
            progressTxt.SetText($"{claimedTasks}/{_taskItemList.Count}");
            if (claimedTasks < _taskItemList.Count) {
                progressTxt.color = ColorTypeConverter.ToHexRGB(TASKS_PROGRESS);
                UpdateClaimBtnState(DailyTaskState.NotDone);
            } else {
                progressTxt.color = ColorTypeConverter.ToHexRGB(TASKS_DONE);
                var finalRewardClaimed = _dailyTaskManager.GetFinalRewardClaimed();
                UpdateClaimBtnState(finalRewardClaimed ? DailyTaskState.Claimed : DailyTaskState.Done);
            }
        }

        private void CheckOrderTask() {
            var children = new List<Transform>();
            foreach (Transform child in content) {
                children.Add(child);
            }

            foreach (var child in children) {
                var item = child.GetComponent<DailyTaskItem>();
                if (item != null) {
                    switch (item.GetTaskState()) {
                        case DailyTaskState.NotDone:
                            break;
                        case DailyTaskState.Done:
                            child.SetAsFirstSibling();
                            break;
                        case DailyTaskState.Claimed:
                            child.SetAsLastSibling();
                            break;
                    }
                }
            }
        }

        private void UpdateClaimBtnState(DailyTaskState state) {
            for (var i = 0; i < claimBtnStates.Length; i++) {
                claimBtnStates[i].SetActive(i == (int)state);
            }
        }

        public void OnCloseButtonClicked() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }

        protected override void OnYesClick() {
            // Do nothing
        }

        public void EnableDialogDailyTasksInfo(bool state) {
            _soundManager.PlaySound(Audio.Tap);
            dialogDailyTaskInfo.SetActive(state);
        }

        public void EnableDialogRewardInfo(bool state) {
            _soundManager.PlaySound(Audio.Tap);
            dialogRewardInfo.SetActive(state);
        }

        private async Task<bool> ClaimDailyTask(int taskId = 0) {
            var waiting = new WaitingUiManager(canvas);
            waiting.Begin();
            try {
                var result = await _dailyTaskManager.ClaimDailyTask(taskId);
                if (taskId == 0 && result.Count > 0) {
                    var productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
                    var rewards = new List<GachaChestItemData>();
                    foreach (var (itemId, quantity) in result) {
                        var reward = new GachaChestItemData(itemId, quantity, productItemManager);
                        rewards.Add(reward);
                    }
                    var dialog = await BLDialogGachaChest.CreateFromChestDailyTask(rewards.ToArray());
                    dialog.Show(canvas);
                }
                return true;
            } catch (Exception e) {
                DialogOK.ShowInfo(canvas, "Reward already claimed");
                return false;
            } finally {
                waiting.End();
            }
        }

        public void OnClaimDailyTaskChestBtn() {
            _soundManager.PlaySound(Audio.Tap);
            UniTask.Void(async () => {
                UpdateClaimBtnState(DailyTaskState.Claimed);
                await ClaimDailyTask();
            });
        }

        public void OnInformationBtn() {
            _soundManager.PlaySound(Audio.Tap);
            var chestShopData = new GachaChestShopData(
                -1,
                -1,
                _dailyTaskManager.GetDailyTaskChest().ToArray(),
                Array.Empty<GachaChestPrice>());
            DialogGachaChestInfo.Create(canvas, chestShopData);
        }
    }
}