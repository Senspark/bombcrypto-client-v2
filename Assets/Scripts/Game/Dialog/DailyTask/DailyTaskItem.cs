using System;
using App;
using Cysharp.Threading.Tasks;
using Game.Dialog.BomberLand.BLGacha;
using Senspark;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyTaskItem : MonoBehaviour
{
    [SerializeField]
    private Image taskIcon;
    
    [SerializeField]
    private Slider taskSlider;
    
    [SerializeField]
    private TextMeshProUGUI taskProgressTxt;
    
    [SerializeField]
    private TextMeshProUGUI taskDescTxt;
    
    [SerializeField]
    private Image rewardIcon;
    
    [SerializeField]
    private TextMeshProUGUI rewardTxt;
    
    [SerializeField]
    private GameObject[] claimBtnStates;
    
    [SerializeField]
    private DailyTaskConfig config;
    
    [SerializeField]
    private BLGachaRes resource;
    
    [SerializeField]
    private Image fill;
    
    private DailyTaskState _taskState;
    private DailyTaskManager.DailyTaskData _dailyTaskData;
    private Action _onGoAction;
    private Action<DailyTaskManager.DailyTaskData> _onClaimed;
    private ISoundManager _soundManager;
    private IDailyTaskManager _dailyTaskManager;
    
    private const string TASKS_PROGRESS = "#F9CD00";
    private const string TASKS_DONE = "#33FF00";

    private void Awake() {
        _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        _dailyTaskManager = ServiceLocator.Instance.Resolve<IDailyTaskManager>();
    }

    public void InitTask(DailyTaskManager.DailyTaskData dailyTaskData) {
        _dailyTaskData = dailyTaskData;
        UpdateTask();
    }

    private void UpdateTask() {
        var rewardQuantity = _dailyTaskData.rewardQuantity;
        var curProgress = _dailyTaskData.curProgress;
        var maxProgress = _dailyTaskData.maxProgress;
        
        taskIcon.sprite = config.GetTaskIconById(_dailyTaskData.taskId);
        taskDescTxt.SetText($"{_dailyTaskData.desc}");
        UniTask.Void(async () => {
            rewardIcon.sprite = await resource.GetSpriteByItemId(_dailyTaskData.rewardId);
        });
        
        rewardTxt.SetText($"x{rewardQuantity}");
        taskSlider.maxValue = maxProgress;
        taskSlider.value = Mathf.Min(curProgress, maxProgress);
        taskProgressTxt.SetText($"{Mathf.Min(curProgress, maxProgress)}/{maxProgress}");

        if (curProgress < maxProgress) {
            fill.color = ColorTypeConverter.ToHexRGB(TASKS_PROGRESS);
            UpdateClaimBtnState(DailyTaskState.NotDone);
        } else {
            fill.color = ColorTypeConverter.ToHexRGB(TASKS_DONE);
            UpdateClaimBtnState(_dailyTaskData.isClaimed ? DailyTaskState.Claimed : DailyTaskState.Done);
        }
    }

    public void InitActions(Action<DailyTaskManager.DailyTaskData> onClaimed, Action onGoAction) {
        _onClaimed = onClaimed;
        _onGoAction = onGoAction;
    }

    public void UpdateClaimBtnState(DailyTaskState state, bool isUpdate = false) {
        _taskState = state;
        for (var i = 0; i < claimBtnStates.Length; i++) {
            claimBtnStates[i].SetActive(i == (int)state);
        }
        if (state == DailyTaskState.Claimed && isUpdate) {
            _dailyTaskManager.SetClaimTask(_dailyTaskData.taskId);
        }
    }
    
    public void OnGoBtn() {
        _soundManager.PlaySound(Audio.Tap);
        _onGoAction?.Invoke();
    }
    
    public void OnClaimBtn() {
        _soundManager.PlaySound(Audio.Tap);
        _onClaimed?.Invoke(_dailyTaskData);
    }

    public DailyTaskState GetTaskState() {
        return _taskState;
    }
}
