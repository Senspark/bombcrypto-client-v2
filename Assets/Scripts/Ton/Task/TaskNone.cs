using System.Collections.Generic;
using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using Senspark;

using UnityEngine;

namespace Ton.Task
{
    public class TaskNone :ObserverManager<UserTonObserver>, ITaskTonManager {
        public Task<bool> Initialize() {
            return System.Threading.Tasks.Task.FromResult(true);
        }

        public void Destroy() {
        }

        public bool IsInitialized { get; set; }

        public ITaskLogic GetTaskLogic(int id) {
            return default;
        }

        public ITaskTonData GetTaskTonData(int id) {
            return default;
        }

        public List<ITaskTonData> GetTaskTonDataList(int taskCategory) {
            return default;
        }
        

        public void InitializeTaskData(List<ITaskTonData> taskTonDataList) {
        }

        public void CheckBuyHeroTask() {
        
        }

        public void CheckBuyHouseTask() {
            
        }

        public void CompleteTask(int id) {
            
        }

        public void ClaimTask(int id) {
        }

        public bool IsHaveAnyTaskDoneWithoutClaimed() {
            return false;
        }

        public int GetHeroAmount() {
            return 0;
        }

        public void AddNewSimpleTaskLogic(int id, string name, string url) {
            
        }

        public Canvas GetCanvas() {
            return default;
        }

        public List<ICategoryTonData> TaskCategoryTonDataDict { get; set; }
        public bool IsValidCategory(int taskCategory) {
            return false;
        }

        public bool IsCompleteCategory(int taskCategory) {
            return false;
        }

        public bool IsHaveAnyTask() {
            return false;
        }

        public List<ITaskTonData> GetAllTaskFromCategory(int taskCategory) {
            return default;
        }

        public bool IsHaveLocalIcon(string iconName) {
            return false;
        }

        public UniTask<Sprite> LoadLocalIcon(string iconName) {
            return default;
        }
    }
}