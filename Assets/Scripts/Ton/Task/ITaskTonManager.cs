using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using Senspark;

using UnityEngine;

[Service(nameof(ITaskTonManager))]
public interface ITaskTonManager : IService, IObserverManager<UserTonObserver> {
    bool IsInitialized { get; set; }
    ITaskLogic GetTaskLogic(int id);
    ITaskTonData GetTaskTonData(int id);
    List<ITaskTonData> GetTaskTonDataList(int taskCategory);
    void InitializeTaskData(List<ITaskTonData> taskTonDataList);
    void CheckBuyHeroTask();
    void CheckBuyHouseTask();
    void CompleteTask(int id);
    void ClaimTask(int id);
    bool IsHaveAnyTaskDoneWithoutClaimed();
    int GetHeroAmount();
    void AddNewSimpleTaskLogic(int id, string name, string url);
    Canvas GetCanvas();
    List<ICategoryTonData> TaskCategoryTonDataDict { get; set; }
    bool IsValidCategory(int taskCategory);
    bool IsCompleteCategory(int taskCategory);
    bool IsHaveAnyTask();
    List<ITaskTonData> GetAllTaskFromCategory(int taskCategory);
    bool IsHaveLocalIcon(string iconName);
    UniTask<Sprite> LoadLocalIcon(string iconName);
}