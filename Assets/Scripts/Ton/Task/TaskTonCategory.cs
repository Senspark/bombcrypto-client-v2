using System;
using System.Collections;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using Senspark;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class TaskTonCategory : MonoBehaviour {
    public int TaskCategory { get; set; }

    [SerializeField]
    private Transform root;
    
    [SerializeField] private TMP_Text title;
    [SerializeField] private Image icon;

    private ITaskTonManager _taskTonManager;
    
    private List<TaskContentTon> _taskObject = new();

    private readonly List<TaskContentTon> _taskContentTons = new();
    
    public void SetData(ICategoryTonData data) {
        TaskCategory = data.TaskCategory;
        title.text = data.Name;
        if (data.Icon != null) {
            icon.sprite = data.Icon;
        } 
    }

    public void Init() {
        _taskTonManager = ServiceLocator.Instance.Resolve<ITaskTonManager>();
    }

    /// <summary>
    /// Tạo tất cả các ui task cho category này
    /// </summary>
    /// <param name="prefabTask"></param>
    /// <param name="targetChestIcon"></param>
    /// <param name="taskTonData"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public List<TaskContentTon> InitTask(GameObject prefabTask, Vector3 targetChestIcon, List<ITaskTonData> taskTonData,
        Transform parent) {

        for (var i = 0 ; i < taskTonData.Count; i++) {
            //Task này ko đc server trả về nên skip
            if(!taskTonData[i].IsValidTask)
                continue;
            // Lấy logic của task từ TaskTonManager và info từ server trong taskTonData
            var logic = _taskTonManager.GetTaskLogic(taskTonData[i].Id);
            if(logic == null)
                continue;
            
            var task = GetTaskContentTon(i, prefabTask);
            if (task == null)
                continue;
            
            task.SetData(
                taskTonData[i],
                logic.Name ?? string.Empty,
                targetChestIcon,
                tsc => logic.OnGo(tsc),
                tsc => logic.OnClaim(tsc),
                parent
            );

            _taskContentTons.Add(task);
        }
        return _taskContentTons;
    }
    
    public TaskContentTon GetTaskContentTon(int index, GameObject prefabTask) {
        if (_taskObject.Count -1 < index) {
            var obj = Instantiate(prefabTask, root);
            obj.TryGetComponent<TaskContentTon>(out var task);
            _taskObject.Add(task);
            return task;
           
        }
        var taskContentTon = _taskObject[index];
        taskContentTon.gameObject.SetActive(true);
        return taskContentTon;
        
    }
    
    private void ReturnTaskContentTon(TaskContentTon taskContentTon) {
        taskContentTon.gameObject.SetActive(false);
    }

    public void ReturnAllTaskToPool() {
        foreach (var task in _taskContentTons) {
            ReturnTaskContentTon(task);
        }
    }
}