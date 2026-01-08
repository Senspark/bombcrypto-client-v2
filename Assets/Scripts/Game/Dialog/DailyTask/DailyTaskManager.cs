using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App;
using Constant;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Senspark;
using UnityEngine;

[Service(nameof(IDailyTaskManager))]
public interface IDailyTaskManager : IService, IObserverManager<DailyTaskObserver> {
    Task GetDailyTaskConfig();
    void ResetDailyTaskConfig();
    void SetClaimTask(int taskId);
    Task<Dictionary<int, int>> ClaimDailyTask(int id = 0);
    void DisableShowNoti(int taskId);
    List<DailyTaskManager.DailyTaskData> GetDailyTaskNoti();
    List<DailyTaskManager.DailyTaskData> GetTodayTaskList();
    List<int> GetDailyTaskChest();
    bool IsInitDone();
    bool GetFinalRewardClaimed();
}

public class DailyTaskObserver {
    public Action<bool> updateRedDot;
    public Action updateHeroChoose;
}

public class DailyTaskManager : ObserverManager<DailyTaskObserver>, IDailyTaskManager {
    private class DailyTaskReward {
        [JsonProperty("quantity")]
        public int quantity;
        
        [JsonProperty("item_id")]
        public int itemId;
    }
    
    private class DailyTaskInfo {
        [JsonProperty("reward")]
        public DailyTaskReward[] reward;
        
        [JsonProperty("claimed")]
        public bool claimed;

        [JsonProperty("progress")]
        public int progress;

        [JsonProperty("task_id")]
        public int taskId;
    }
    
    private class DailyTaskConfig {
        [JsonProperty("final_reward_claimed")]
        public bool finalRewardClaimed;

        [JsonProperty("url_config")]
        public string urlConfig;
        
        [JsonProperty("tasks")]
        public DailyTaskInfo[] tasks;
    }

    public class DailyTaskData {
        public int taskId;
        public int curProgress;
        public int maxProgress;
        public int rewardId;
        public int rewardQuantity;
        public bool isClaimed;
        public string desc;
        public ShowNotiState showNotiState;
    }

    public enum ShowNotiState {
        EnableAllow,
        EnableNotAllow,
        Disable
    }

    private bool _isLoadConfig = false;
    private bool _finalRewardClaimed;
    private List<DailyTaskData> _taskList = new List<DailyTaskData>();
    private List<int> _chestList = new List<int>();
    
    private readonly ILogManager _logManager;
    private readonly IServerRequester _serverRequester;
    
    public DailyTaskManager(
        ILogManager logManager,
        IServerRequester serverRequester
    ) {
        _logManager = logManager;
        _serverRequester = serverRequester;
    }
    
    public async Task GetDailyTaskConfig() {
        if (!_isLoadConfig) {
            await InitDailyTaskConfig();
        } else {
            await UpdateDailyTaskConfig();
        }
        UpdateRedDot();
    }
    
    public void ResetDailyTaskConfig() {
        _isLoadConfig = false;
    }

    private async Task InitDailyTaskConfig() {
        var result = JsonConvert.DeserializeObject<DailyTaskConfig>(
            await _serverRequester.GetDailyTaskConfig()
        );
        var dailyTaskLoader = new DailyTaskLoader(_logManager, result.urlConfig);
        await dailyTaskLoader.LoadJson();
        _chestList.AddRange(dailyTaskLoader.Chest);
        CreateTaskList(dailyTaskLoader.Tasks, result.tasks);
        _finalRewardClaimed = result.finalRewardClaimed;
        _isLoadConfig = true;
    }

    private async Task UpdateDailyTaskConfig() {
        var result = await _serverRequester.GetUserDailyProgress();
        for (var i = 0; i < result.Count; i++) {
            _taskList[i].curProgress = result[i];
            if (_taskList[i].curProgress >= _taskList[i].maxProgress &&
                _taskList[i].showNotiState == ShowNotiState.EnableNotAllow) {
                _taskList[i].showNotiState = ShowNotiState.EnableAllow;
            }
        }
    }
    
    private void CreateTaskList(List<DailyTaskLoader.TaskDataJson> allTaskInfo, DailyTaskInfo[] todayTaskInfo) {
        var taskInfo = new Dictionary<int, DailyTaskLoader.TaskDataJson>();
        foreach (var info in allTaskInfo) {
            taskInfo.Add(info.taskId, info);
        }
        _taskList.Clear();
        foreach (var task in todayTaskInfo) {
            var taskData = new DailyTaskData();
            taskData.taskId = task.taskId;
            taskData.curProgress = task.progress;
            taskData.maxProgress = taskInfo[task.taskId].taskMaxProgress;
            taskData.rewardId = task.reward[0].itemId;
            taskData.rewardQuantity = task.reward[0].quantity;
            taskData.isClaimed = task.claimed;
            taskData.desc = taskInfo[task.taskId].taskDesc;
            if (taskData.isClaimed) {
                taskData.showNotiState = ShowNotiState.Disable;
            } else {
                if (taskData.curProgress >= taskData.maxProgress) {
                    taskData.showNotiState = ShowNotiState.EnableAllow;
                } else {
                    taskData.showNotiState = ShowNotiState.EnableNotAllow;
                }
            }
            _taskList.Add(taskData);
        }
    }
    
    private void UpdateRedDot() {
        var doneTasks = 0;
        var claimedTasks = 0;
        foreach (var task in _taskList) {
            if (!task.isClaimed && (task.curProgress >= task.maxProgress)) {
                doneTasks++;
            }
            if (task.isClaimed) {
                claimedTasks++;
            }
        }
        var enableRedDot = doneTasks > 0 || (claimedTasks == _taskList.Count && !_finalRewardClaimed);
        DispatchEvent(e => e.updateRedDot?.Invoke(enableRedDot));
    }

    public void SetClaimTask(int taskId) {
        var updateHeroChooseList = new List<GachaChestProductId>() {
            GachaChestProductId.Key,
            GachaChestProductId.Shield,
            GachaChestProductId.BombAdd1,
            GachaChestProductId.RangeAdd1,
            GachaChestProductId.SpeedAdd1
        };
        foreach (var task in _taskList) {
            if (task.taskId == taskId) {
                task.isClaimed = true;
                var rewardType = (GachaChestProductId)task.rewardId;
                if (updateHeroChooseList.Contains(rewardType)) {
                    UniTask.Void(async () => {
                        //DevHoang_20250408: Wait a little bit so server can finish updating rewards
                        await UniTask.Delay(1000);
                        DispatchEvent(e => e.updateHeroChoose?.Invoke());
                    });
                }
            }
        }
        UpdateRedDot();
    }
    
    public async Task<Dictionary<int, int>> ClaimDailyTask(int id = 0) {
        var result = await _serverRequester.ClaimDailyTask(id);
        if (id == 0) {
            _finalRewardClaimed = true;
            UpdateRedDot();
        }
        return result;
    }

    public void DisableShowNoti(int taskId) {
        foreach (var task in _taskList) {
            if (task.taskId == taskId) {
                task.showNotiState = ShowNotiState.Disable;
            }
        }
    }
    
    public List<DailyTaskData> GetDailyTaskNoti() {
        var result = new List<DailyTaskData>();
        foreach (var task in _taskList) {
            if (task.showNotiState == ShowNotiState.EnableAllow) {
                result.Add(task);
            }
        }
        return result;
    }
    
    public List<DailyTaskData> GetTodayTaskList() {
        return _taskList;
    }

    public List<int> GetDailyTaskChest() {
        return _chestList;
    }

    public bool IsInitDone() {
        return _isLoadConfig;
    }

    public bool GetFinalRewardClaimed() {
        return _finalRewardClaimed;
    }

    public Task<bool> Initialize() {
        return Task.FromResult(true);
    }

    public void Destroy() {
    }
}
