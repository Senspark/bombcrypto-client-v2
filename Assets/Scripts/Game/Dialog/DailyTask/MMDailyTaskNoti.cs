using System.Collections;
using System.Collections.Generic;
using Senspark;
using UnityEngine;

public class MMDailyTaskNoti : MonoBehaviour {
    [SerializeField]
    private DailyTaskNotiItem dailyTaskNotiItem;

    [SerializeField]
    private Transform slot1;
    
    [SerializeField]
    private Transform slot2;
    
    [SerializeField]
    private DailyTaskConfig config;

    private Canvas _canvas;
    
    private IDailyTaskManager _dailyTaskManager;

    public void SetCanvas(Canvas canvas) {
        _canvas = canvas;
        _dailyTaskManager = ServiceLocator.Instance.Resolve<IDailyTaskManager>();
        StartCoroutine(DelayToShowNoti());
    }

    private IEnumerator DelayToShowNoti() {
        yield return new WaitUntil(() => _dailyTaskManager.IsInitDone());
        yield return new WaitForSeconds(1f);
        ShowDailyTaskNoti(_dailyTaskManager.GetDailyTaskNoti());
    }
    
    private void ShowDailyTaskNoti(List<DailyTaskManager.DailyTaskData> taskList) {
        if (taskList.Count <= 0) return;
        var firstNotiItem = Instantiate(dailyTaskNotiItem, slot1).GetComponent<DailyTaskNotiItem>();
        firstNotiItem.SetTaskIcon(config.GetTaskIconById(taskList[0].taskId), _canvas);
        firstNotiItem.PlayAnimSlot1();
        _dailyTaskManager.DisableShowNoti(taskList[0].taskId);
        if (taskList.Count > 1) {
            for (var i = 1; i < taskList.Count; i++) {
                var notiItem = Instantiate(dailyTaskNotiItem, slot2).GetComponent<DailyTaskNotiItem>();
                notiItem.SetTaskIcon(config.GetTaskIconById(taskList[i].taskId), _canvas);
                var posYSlot1 = slot1.GetComponent<RectTransform>().anchoredPosition.y;
                var posYSlot2 = slot2.GetComponent<RectTransform>().anchoredPosition.y;
                var range = posYSlot1 - posYSlot2;
                notiItem.PlayAnimSlot2(i - 1, range);
                _dailyTaskManager.DisableShowNoti(taskList[i].taskId);
            }
        }
    }
}
