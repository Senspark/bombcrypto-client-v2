using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using App;

using Castle.Core.Internal;

using Cysharp.Threading.Tasks;

using Senspark;

using UnityEngine;

using Object = UnityEngine.Object;

public class TaskTonManager : ObserverManager<UserTonObserver>, ITaskTonManager {
    private Dictionary<int, ITaskLogic> _taskLogic = new();

    private readonly Dictionary<int, ITaskTonData> _taskTonDataDict = new();
    private List<int> _allCategoryComplete = new();
    private readonly IPlayerStorageManager _playerStorage;
    private readonly IHouseStorageManager _houseStorage;
    private IUserTonManager _userTonManager;
    private readonly ILogManager _logManager;
    private Canvas _canvas;
    
    private const string TonPath = "Assets/Scenes/TreasureModeScene/Textures/Pack/Task/icon";

    public List<ICategoryTonData> TaskCategoryTonDataDict { get; set; } = new();

    public TaskTonManager(IPlayerStorageManager playerStorage, IHouseStorageManager houseStorage,
        ILogManager logManager) {
        _playerStorage = playerStorage;
        _houseStorage = houseStorage;
        _logManager = logManager;
    }

    public Task<bool> Initialize() {
        _taskLogic = new Dictionary<int, ITaskLogic>();

        try {
            var tasks = new ITaskLogic[] {
                new Buy1HeroTask(),
                new Buy5HeroTask(),
                new Buy15HeroTask(),
                new BuyHouseTask()
            };

            foreach (var task in tasks) {
                _taskLogic[task.Id] = task;
            }
        } catch (Exception e) {
            _logManager.Log($"CLIENT: Error when init task logic: {e.Message}");
        }

        return Task.FromResult(true);
    }

    public void Destroy() {
        _taskLogic.Clear();
    }

    public bool IsInitialized { get; set; }

    /// <summary>
    /// Lấy ra object chứa logic của task đó từ id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ITaskLogic GetTaskLogic(int id) {
        return _taskLogic.GetValueOrDefault(id);
    }

    /// <summary>
    /// Lấy ra thông tin cơ bản của 1 task từ id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ITaskTonData GetTaskTonData(int id) {
        return _taskTonDataDict.GetValueOrDefault(id);
    }

    /// <summary>
    /// Lấy ra danh sách data của các task lấy từ server theo từng category
    /// </summary>
    /// <param name="taskCategory"></param>
    /// <returns></returns>
    public List<ITaskTonData> GetTaskTonDataList(int taskCategory) {
        return _taskTonDataDict.Values.Where(data => data.TaskCategory == taskCategory).ToList();
    }

    /// <summary>
    /// Lưu lại các task data đã load từ server, đây là nơi duy nhất để truy cập và sử dụng task ở client
    /// </summary>
    /// <param name="taskTonDataList"></param>
    public void InitializeTaskData(List<ITaskTonData> taskTonDataList) {
        foreach (var data in taskTonDataList) {
            _taskTonDataDict[data.Id] = data;
        }

        //Lưu lại các category đã hoàn thành hết task
        _allCategoryComplete = _taskTonDataDict.Values
            //Task công ty luôn hiện dù có làm hết hay chưa
            .Where(task => task.TaskCategory != 1)
            .GroupBy(task => task.TaskCategory)
            .Where(group => group.All(task => task.IsClaimed))
            .Select(group => group.Key)
            .ToList();
    }

    public List<ITaskTonData> GetAllTaskFromCategory(int taskCategory) {
        return _taskTonDataDict.Values.Where(data => data.TaskCategory == taskCategory).ToList();
    }

    /// <summary>
    /// Kiểm tra xem user này có từng mua đủ 15 hero chưa để hoàn thành task mua hero, gọi sau khi mua thành công hero
    /// </summary>
    public async void CheckBuyHeroTask() {
        var buy1Hero =
            _taskTonDataDict.Values.FirstOrDefault(data => data.Id == TaskData.Buy1Hero)?.IsCompleted is true;
        var buy5Hero =
            _taskTonDataDict.Values.FirstOrDefault(data => data.Id == TaskData.Buy5Hero)?.IsCompleted is true;
        var buy15Hero =
            _taskTonDataDict.Values.FirstOrDefault(data => data.Id == TaskData.Buy15Hero)?.IsCompleted is true;

        //Đã hoàn thành 3 task
        if (buy1Hero && buy5Hero && buy15Hero) {
            return;
        }
        _userTonManager ??= ServiceLocator.Instance.Resolve<IServerManager>().UserTonManager;

        var heroAmount = _playerStorage.GetPlayerDataList(HeroAccountType.Ton).Count;

        if (!buy1Hero && heroAmount >= 1) {
            var result = await _userTonManager.CompleteTask(TaskData.Buy1Hero);
            if (result) {
                CompleteTask(TaskData.Buy1Hero);
            }
        }
        if (!buy5Hero && heroAmount >= 5) {
            var result = await _userTonManager.CompleteTask(TaskData.Buy5Hero);
            if (result) {
                CompleteTask(TaskData.Buy5Hero);
            }
        }
        if (!buy15Hero && heroAmount >= 15) {
            var result = await _userTonManager.CompleteTask(TaskData.Buy15Hero);
            if (result) {
                CompleteTask(TaskData.Buy15Hero);
            }
        }
    }

    /// <summary>
    /// Kiểm tra xem user này có từng mua nhà chưa để hoàn thành task mua nhà
    /// </summary>
    public async void CheckBuyHouseTask() {
        const int id = TaskData.BuyHouse;
        var buyHouse = _taskTonDataDict.Values.FirstOrDefault(data => data.Id == id)?.IsCompleted is true;

        //Đã hoàn thành task
        if (buyHouse) {
            return;
        }
        _userTonManager ??= ServiceLocator.Instance.Resolve<IServerManager>().UserTonManager;

        var houseCount = _houseStorage.GetHouseCount();

        if (houseCount >= 1) {
            var result = await _userTonManager.CompleteTask(id);
            if (result) {
                CompleteTask(id);
            }
        }
    }

    /// <summary>
    /// Dùng để cập nhật lại task đã hoàn thành ở client
    /// </summary>
    /// <param name="id"></param>
    public void CompleteTask(int id) {
        if (_taskTonDataDict.TryGetValue(id, out var task)) {
            task.IsCompleted = true;
            _taskTonDataDict[id] = task;
            DispatchEvent(e => e.OnCompleteTask?.Invoke(id));
        }
    }

    /// <summary>
    /// Dùng để cập nhật lại task đã nhận thưởng ở client
    /// </summary>
    /// <param name="id"></param>
    public void ClaimTask(int id) {
        if (_taskTonDataDict.TryGetValue(id, out var task)) {
            task.IsClaimed = true;
            _taskTonDataDict[id] = task;
            DispatchEvent(e => e.OnClaimTask?.Invoke(id));
        }
    }

    /// <summary>
    /// Kiểm tra xem có bất kỳ task nào đã hoàn thành mà chưa nhận thưởng ko
    /// </summary>
    /// <returns></returns>
    public bool IsHaveAnyTaskDoneWithoutClaimed() {
        return _taskTonDataDict.Values.Any(data => data.IsCompleted && !data.IsClaimed);
    }

    public int GetHeroAmount() {
        return _playerStorage.GetPlayerDataList(HeroAccountType.Ton).Count;
    }

    //Thêm task mở link mới mà ko cần set sẵn từ client
    public void AddNewSimpleTaskLogic(int id, string name, string url) {
        if (_taskLogic.ContainsKey(id)) {
            return;
        }
        var task = new NewTaskOpenUrl(id, name, url);
        _taskLogic.TryAdd(id, task);
    }

    /// <summary>
    /// Kiểm tra xem category này có task nào hay ko
    /// </summary>
    /// <param name="taskCategory"></param>
    /// <returns></returns>
    public bool IsValidCategory(int taskCategory) {
        return _taskTonDataDict.Values.Any(data => data.TaskCategory == taskCategory && data.IsValidTask);
    }

    /// <summary>
    /// Kiểm tra xem category này đã hoàn thành hết task chưa
    /// </summary>
    /// <param name="taskCategory"></param>
    /// <returns></returns>
    public bool IsCompleteCategory(int taskCategory) {
        return _allCategoryComplete.Contains(taskCategory);
    }

    /// <summary>
    /// Kiểm tra xem có task nào hay ko
    /// </summary>
    /// <returns></returns>
    public bool IsHaveAnyTask() {
        return _taskTonDataDict.Count > 0 && TaskCategoryTonDataDict.Count > 0;
    }

    //Dùng để cho các task cần show dialog
    public Canvas GetCanvas() {
        if (_canvas == null) {
            var obj = Object.FindObjectOfType<CanvasDialogTag>();
            _canvas = obj?.GetComponent<Canvas>();
        }
        return _canvas;
    }
    
    public bool IsHaveLocalIcon(string iconName) {
        RemoveQuestionMark(ref iconName);
        return _taskIconNameList.Contains(iconName);
    }
    
    private void RemoveQuestionMark(ref string iconName) {
        if (iconName.Contains("?")) {
            iconName = iconName.Substring(0, iconName.IndexOf("?", StringComparison.Ordinal));
        }
    }
    
    public UniTask<Sprite> LoadLocalIcon(string iconName) {
        try {
            if (iconName.IsNullOrEmpty() )
                return UniTask.FromResult<Sprite>(null);
        
            RemoveQuestionMark(ref iconName);
        
            if (!IsHaveLocalIcon(iconName))
                return UniTask.FromResult<Sprite>(null);
        
            var sprite = AddressableLoader.LoadAsset<Sprite>($"{TonPath}/{iconName}");    
            return sprite;
        } catch (Exception e) {
            return UniTask.FromResult<Sprite>(null);
        }
        
    }

    /// <summary>
    /// list chứa các icon hình có sẵn ở local để khỏi cần tải từ cloud giúp giảm memory và laod nhanh
    /// </summary>
    private readonly List<string> _taskIconNameList = new() {
        "Icon_Agentz.png",
        "Icon_Aylab.png",
        "Icon_BFBGame.png",
        "Icon_BPay.png",
        "Icon_Bastion_Battle.png",
        "Icon_BearFi.png",
        "Icon_Bee_Harvest.png",
        "Icon_Bine.png",
        "Icon_Bomb.png",
        "Icon_Butterfly.png",
        "Icon_Capypara_Meme.png",
        "Icon_Cat_Gold_Miner.png",
        "Icon_Clarnium.png",
        "Icon_Clockie_Chaos.png",
        "Icon_CoinGatePad.png",
        "Icon_Coincrypto.png",
        "Icon_Cowtopia.png",
        "Icon_Cryston.png",
        "Icon_DealTON.png",
        "Icon_Def.png",
        "Icon_DogX.png",
        "Icon_Dragon_Slither.png",
        "Icon_Easy_Cake.png",
        "Icon_Egg_Tapper.png",
        "Icon_FOF.png",
        "Icon_FROGS.png",
        "Icon_Farm_Clicker.png",
        "Icon_Fish_War.png",
        "Icon_Fishtopia.png",
        "Icon_Fortune_Boss.png",
        "Icon_Fortune_Cats.png",
        "Icon_Froggy.png",
        "Icon_Fruicy_Blast.png",
        "Icon_GemGame.png",
        "Icon_Grand_Journey.png",
        "Icon_Greedy_Ball.png",
        "Icon_Habit_Network.png",
        "Icon_HangarX.png",
        "Icon_Happy_Farmer.png",
        "Icon_Kaboom.png",
        "Icon_Khosmium2054.png",
        "Icon_Kokomon.png",
        "Icon_LOL_Happy_Mining.png",
        "Icon_Lamaz.png",
        "Icon_Lil_Piggies.png",
        "Icon_Lion_Goal.png",
        "Icon_MemeTD.png",
        "Icon_Mimiland.png",
        "Icon_Mobiverse.png",
        "Icon_Money_GardenAI.png",
        "Icon_Monkey_Paw.png",
        "Icon_Musgard.png",
        "Icon_My_Corp.png",
        "Icon_OUTA.png",
        "Icon_OfferX.png",
        "Icon_Panda_Frenzy.png",
        "Icon_Pecks.png",
        "Icon_Piloton.png",
        "Icon_Plant_Harvest.png",
        "Icon_PokeTON.png",
        "Icon_PopLaunch.png",
        "Icon_Psyduck.png",
        "Icon_Purr.png",
        "Icon_RABBITS.png",
        "Icon_RUN_tap_tap_tap.png",
        "Icon_Ragdoll.png",
        "Icon_RoyalPet.png",
        "Icon_Scoo_g.png",
        "Icon_Scratch_That.png",
        "Icon_Shark_Attack.png",
        "Icon_Shiok.png",
        "Icon_Sirius_Pad.png",
        "Icon_Slimewifhat.png",
        "Icon_Snap_Fly.png",
        "Icon_Stability_World_AI.png",
        "Icon_Star_Ai.png",
        "Icon_TFarm.png",
        "Icon_TONOS.png",
        "Icon_TON_Flash.png",
        "Icon_TON_Realm.png",
        "Icon_TON_pirate_game.png",
        "Icon_Tea_Farm.png",
        "Icon_The_Caps.png",
        "Icon_Totemancer.png",
        "Icon_Tribe_TON.png",
        "Icon_UQUID.png",
        "Icon_Unicorn_Galaxy.png",
        "Icon_WCoin.png",
        "Icon_Wepunk.png",
        "Icon_XStar_Fleet.png",
        "Icon_Yumify.png",
        "Icon_Doonz_Squad.png",
        "Icon_TON_The_Sheep.png",
        "Icon_Trump_Fight.png",
        "Icon_Ballz_Of_Steel.png",  
        "Icon_HamsterRepublic.png",
        "Icon_Mirrion.png", 
        "Icon_Ancestral_Land.png",
        "Icon_Captcha.png",
        "Icon_Cat_Planets.png",
        "Icon_Meta_Racing.png",
        "Icon_Move.png",
        "Icon_The_Last_Dwarfs.png",
        "Icon_Joker.png",
        "Icon_TON_Garden.png",
        "Icon_Alpaca_Money.png",
        "Icon_Alpaca_Money.png",
        "Icon_Container_Rush.png",
        "Icon_Dino_TOP.png",
        "Icon_Mushroom_Warrior.png",
        "Icon_TON_Free.png",
        "Icon_Green_Thumb_Farm.png",
        "Icon_Notdust.png",
        "Icon_Titan.png",
    };
}