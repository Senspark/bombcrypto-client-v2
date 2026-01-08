using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using App;
using Cysharp.Threading.Tasks;
using Senspark;
using Share.Scripts.Communicate;
using UnityEngine;

public enum ClientLogPlatform {
    UNITY,
    REACT
}

public enum ClientLogType {
    INFO,
    WARNING,
    ERROR
}

[Service(nameof(IClientLogManager))]
public interface IClientLogManager : IService {
    void CollectReactLog(string message);
}

public class ClientLogEntry {
    public ClientLogType LogType { get; set; }
    public string LogMessage { get; set; }

    public ClientLogEntry(ClientLogType logType, string logMessage) {
        LogType = logType;
        LogMessage = logMessage;
    }
}

public class ClientLogManager : IClientLogManager {
    private bool _allowGetLog = false;
    private List<ClientLogEntry> _clientLogs = new List<ClientLogEntry>();

    private List<string> _ignoreLogs = new List<string>() {
        "PING_PONG",
        "There can be only one active Event System",
        "There are 2 event systems in the scene. Please ensure there is always exactly one event system in the scene",
        "There are 2 audio listeners in the scene. Please ensure there is always exactly one audio listener in the scene"
    };
    
    private IServerNotifyManager _serverNotifyManager;
    private IServerManager _serverManager;
    private ObserverHandle _handle;

    private const int SEND_LOG_INTERVAL = 5000; // Milliseconds
    
    public ClientLogManager(IServerNotifyManager serverNotifyManager, IServerManager serverManager) {
        _serverNotifyManager = serverNotifyManager;
        _serverManager = serverManager;
        _handle = new ObserverHandle();
        _handle.AddObserver(_serverNotifyManager, new ServerNotifyObserver() {
            OnClientSendLog = OnClientSendLog
        });
        Application.logMessageReceivedThreaded += CollectClientLog;
    }

    private void OnClientSendLog() {
        _allowGetLog = true;
        SendClientLog();
    }

    private void CollectClientLog(string message, string stackTrace, LogType type) {
        if (!_allowGetLog) return;
        if (_ignoreLogs.Any(message.Contains)) return;
        
        var logType = GetClientLogType(type);
        var logMessage = $"[{ClientLogPlatform.UNITY}] {message}";
        _clientLogs.Add(new ClientLogEntry(logType, logMessage));
    }

    public void CollectReactLog(string message) {
        if (!_allowGetLog) return;
        if (_ignoreLogs.Any(message.Contains)) return;
        
        var match = Regex.Match(message, @"^\[(\w+)\]\s*(.+)", RegexOptions.Singleline);
        if (match.Success) {
            var reactLogType = match.Groups[1].Value; // "INFO" or "ERROR"
            var reactLogMessage = match.Groups[2].Value;
            Enum.TryParse<ClientLogType>(reactLogType, out var logType);
            var logMessage = $"[{ClientLogPlatform.REACT}] {reactLogMessage}";
            _clientLogs.Add(new ClientLogEntry(logType, logMessage));
        } else {
            _clientLogs.Add(new ClientLogEntry(ClientLogType.ERROR, "[REACT] Wrong React log format"));
        }
    }

    private ClientLogType GetClientLogType(LogType type) {
        switch (type) {
            case LogType.Log:
                return ClientLogType.INFO;
            case LogType.Warning:
                return ClientLogType.WARNING;
            default:
                // Exception, Assert, Error
                return ClientLogType.ERROR;
        }
    }

    private void SendClientLog() {
        UniTask.Void(async () => {
            await StartSendLogAsync();
        });
        
    }
    
    private async UniTask StartSendLogAsync() {
        while (true) {
            if (_clientLogs.Count > 0) {
                var logsToSend = new List<ClientLogEntry>(_clientLogs);
                _serverManager.General.SendClientLog(logsToSend);
                _clientLogs.Clear();
            }
            await UniTask.Delay(SEND_LOG_INTERVAL);
        }
    }

    public Task<bool> Initialize() {
        return Task.FromResult(true);
    }

    public void Destroy() {
    }
}
