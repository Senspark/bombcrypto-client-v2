using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App;
using App.BomberLand;
using CustomSmartFox;
using CustomSmartFox.SolCommands;
using Engine.Manager;
using Game.Dialog;
using PvpMode.Services;
using Senspark;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Share.Scripts.Communicate;
using Share.Scripts.Communicate.UnityReact;
using UnityEngine;

public class UserTonObserver {
    public Action<bool> OnDepositComplete;
    public Action<ISFSObject> OnDepositResponse;
    public Action<int> OnCompleteTask;
    public Action<int> OnClaimTask;
    public Action<IClubInfo> OnJoinClub;
    public Action OnLeaveClub;
}

public class UserTonManager : ObserverManager<UserTonObserver>, IUserTonManager {
    private readonly IServerDispatcher _serverDispatcher;
    private readonly ILogManager _logManager;
    private readonly ITaskTonManager _taskTonManager;
    private readonly IGeneralServerBridge _generalServerBridge;
    private readonly IUserAccountManager _accountManager;
    private readonly JavascriptProcessor _processor;
    private readonly IExtResponseEncoder _encoder;

    public UserTonManager(IServerDispatcher serverDispatcher, ITaskTonManager taskTonManager,
        IGeneralServerBridge generalServerBridge, IUserAccountManager accountManager, ILogManager logManager, IExtResponseEncoder encoder) {
        _serverDispatcher = serverDispatcher;
        _logManager = logManager;
        _taskTonManager = taskTonManager;
        _generalServerBridge = generalServerBridge;
        _accountManager = accountManager;
        _processor = JavascriptProcessor.Instance;
        _encoder = encoder;
    }

    public Task<bool> Initialize() {
        return Task.FromResult(true);
    }

    public void OnExtensionResponse(string cmd, ISFSObject value) {
        if (cmd == SFSDefine.SFSCommand.USER_INITIALIZED) {
            //FIXME: dùng tạm
            var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            serverManager.IsUserInitSuccess = true;
            _logManager.Log("USER_INITIALIZED success");
            // Dirty fix
            EventManager.Dispatcher(LoginEvent.UserInitialized);
        }
        if (cmd == SFSDefine.SFSCommand.USER_JOIN_CLUB) {
            DispatchEvent(e => e.OnJoinClub?.Invoke(new DefaultGeneralServerBridge.ClubInfo(value)));
            return;
        }
        if (cmd == SFSDefine.SFSCommand.USER_LEAVE_CLUB) {
            DispatchEvent(e => e.OnLeaveClub?.Invoke());
        }
        if (cmd != SFSDefine.SFSCommand.DEPOSIT_TON_RESPONSE) {
            return;
        }
        var response = DefaultPvpServerBridge.PvpGenericResult.FastParse(value);
        if (response.Code == 0) {
            DispatchEvent(e => e.OnDepositResponse?.Invoke(value));
            DispatchEvent(e => e.OnDepositComplete?.Invoke(true));
        } else {
            DispatchEvent(e => e.OnDepositComplete?.Invoke(false));
        }
    }

    public void OnExtensionResponse(string cmd, int requestId, byte[] data) {
        if (cmd == SFSDefine.SFSCommand.USER_INITIALIZED) {
            //FIXME: dùng tạm
            var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            serverManager.IsUserInitSuccess = true;
            _logManager.Log("USER_INITIALIZED success");
            // Dirty fix
            EventManager.Dispatcher(LoginEvent.UserInitialized);
        }
        
        if (cmd != SFSDefine.SFSCommand.DEPOSIT_TON_RESPONSE) {
            return;
        }
        var (outData, json) = _encoder.DecodeDataToSfsObject(data);
        DispatchEvent(e => e.OnDepositResponse?.Invoke(outData));
        DispatchEvent(e => e.OnDepositComplete?.Invoke(true));
          
    }

    public void OnExtensionError(string cmd, int requestId, int errorCode, string errorMessage) {
        if (cmd != SFSDefine.SFSCommand.DEPOSIT_TON_RESPONSE) {
            return;
        }
        DispatchEvent(e => e.OnDepositComplete?.Invoke(false));
    }
    

    public async Task<bool> GetTaskTonDataConfig() {
        //For test
        // var resultFake = new TestTonTask().GetFakeDataList();
        // _taskTonManager.InitializeTaskData(resultFake);
        // return await Task.FromResult(resultFake);
        var data = new SFSObject();
        
        var response = await _serverDispatcher.SendCmd(new CmdGetUserTasks(data));
        OnGetTaskData(response);
        // Ko đợi load task nữa, cho vô game luôn, sẽ load trong nền
        //await _loadIconTask.Task;
        return true;
    }

    private async void OnGetTaskData(ISFSObject data) {
        var taskData = new List<ITaskTonData>();
        var taskCategoryData = new List<ICategoryTonData>();
        //Dùng để đảm bảo mỗi id  task là duy nhất, bỏ qua các id trùng lặp nếu có
        var idList = new List<int>();
        var linkData = data.GetUtfString("url_tasks");
        //DevHoang_20250613: Temporary change the link to test
        if (!AppConfig.IsProduction) {
            const string originalBase = "https://game.bombcrypto.io/";
            const string newBase = "";
            Debug.LogError("need newBase url");
            linkData = linkData.Replace(originalBase, newBase);
        }
        var jsonTaskLoader = new JsonTaskLoader(linkData, _logManager);
        await jsonTaskLoader.LoadJson();

        var tasks = jsonTaskLoader.Tasks;
        var categories = jsonTaskLoader.Categories;
        // Parse thông tin hiển thị cơ bản cho các task
        // Lấy data từ json trước vì như vậy sẽ dễ dàng kiểm soát thứ tự hiển thị ở client
        // mà ko cần thay đổi cách trả về của server
        foreach (var task in tasks) {
            var baseData = new TaskTonData(task);
            //Bỏ qua các id trùng lặp
            if (idList.Contains(baseData.Id))
                continue;
            taskData.Add(baseData);
            idList.Add(baseData.Id);
        }

        idList.Clear();

        var array = data.GetSFSArray("data");

        //Update các thông tin còn thiếy của task từ server
        for (var i = 0; i < array.Size(); i++) {
            var taskDataObj = array.GetSFSObject(i);
            var haveTask = taskDataObj.ContainsKey("id");
            var id = taskDataObj.GetInt("id");
            if (!haveTask) {
                continue;
            }
            var t = taskData.FirstOrDefault(e => e.Id == id);
            if (t != null && !idList.Contains(t.Id)) {
                idList.Add(t.Id);
                t.UpdateData(taskDataObj);
                _taskTonManager.AddNewSimpleTaskLogic(t.Id, t.Name, t.Link);
            }
        }

        //Lưu các task vào taskTonManager để sử dụng ở client
        _taskTonManager.InitializeTaskData(taskData);

        //Parse các category

        foreach (var category in categories) {
            //Để optimize, các category làm hết task rồi sẽ ko hiện nữa
            if (_taskTonManager.IsCompleteCategory(category.CateGory))
                continue;
            Sprite icon = null;
            // Nếu đây là icon mới thêm hoặc là icon cũ mà có thay dổi thì mới load từ cloud về, ko thì dùng local
            if (!category.IsIconChanged && _taskTonManager.IsHaveLocalIcon(category.Icon)) {
                icon = await _taskTonManager.LoadLocalIcon(category.Icon);
                //Trường hợp load từ local ko đc thì vẫn load tư cloud
                if (icon == null)
                    icon = await jsonTaskLoader.LoadIcon(category.Icon);
            } else {
                icon = await jsonTaskLoader.LoadIcon(category.Icon);
            }
            var categoryData = new CategoryTonData(category, icon);
            taskCategoryData.Add(categoryData);
        }
        //Lưu các category vào taskTonManager để sử dụng ở client
        _taskTonManager.TaskCategoryTonDataDict = taskCategoryData;

        _taskTonManager.IsInitialized = true;
    }

    public async Task<bool> CompleteTask(int taskId) {
        var data = new SFSObject();
        data.PutInt("task_id", taskId);

        var response = await _serverDispatcher.SendCmd(new CmdCompleteTask(data));
        return OnCompleteTask(response);
    }

    private bool OnCompleteTask(ISFSObject data) {
        var result = data.GetBool("is_complete");
        return result;
    }

    public async Task<bool> ClaimTask(int taskId) {
        var data = new SFSObject();
        data.PutInt("task_id", taskId);
        
        var response = await _serverDispatcher.SendCmd(new CmdClaimTaskReward(data));
        return OnClaimTask(response);
    }

    private bool OnClaimTask(ISFSObject data) {
        _generalServerBridge.UpdateUserReward(data);
        return true;
    }

    public async Task<string> GetInvoice(double amount, DepositType depositType) {
        var data = new SFSObject();
        data.PutInt("deposit_type", (int) depositType);
        var response = await _serverDispatcher.SendCmd(new CmdGetInvoiceDepositTon(data));
        return OnDepositTon(response);
    }

    private string OnDepositTon(ISFSObject data) {
        var invoice = data.GetUtfString("invoice");
        return invoice;
    }

    public async Task<IReferralData> GetReferralData() {
        var data = new SFSObject();
        
        var response = await _serverDispatcher.SendCmd(new CmdGetReferralData(data));
        return OnGetReferralData(response);
    }

    private IReferralData OnGetReferralData(ISFSObject data) {
        return _generalServerBridge.InitReferralData(data);
    }

    public async Task<bool> ClaimReferralReward() {
        var data = new SFSObject();
        
        var response = await _serverDispatcher.SendCmd(new CmdClaimReferralReward(data));
        return OnClaimReferralReward(response);
    }

    public async Task<bool> ReactiveHouse(int houseId) {
        var data = new SFSObject();
        data.PutInt("house_id", houseId);
        
        var response = await _serverDispatcher.SendCmd(new CmdReactiveHouse(data));
        return OnReactiveHouse(response);
    }
    
    private bool OnReactiveHouse(ISFSObject data) {
        _generalServerBridge.UpdateUserReward(data);
        return true;
    }

    private bool OnClaimReferralReward(ISFSObject data) {
        _generalServerBridge.UpdateUserReward(data);
        return true;
    }

    public void Destroy() {
    }

    public void OnConnection() {
    }

    public void OnConnectionError(string message) {
    }

    public void OnConnectionRetry() {
    }

    public void OnConnectionResume() {
    }

    public void OnConnectionLost(string reason) {
    }

    public void OnLogin() {
    }

    public void OnLoginError(int code, string message) {
    }

    public void OnUdpInit(bool success) {
    }

    public void OnPingPong(int lagValue) {
    }

    public void OnRoomVariableUpdate(SFSRoom room) {
    }

    public void OnJoinRoom(SFSRoom room) {
    }
}