using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Senspark;

using UnityEngine;

namespace App {
    public class FileLoggingLogManager : IService, ILogManager {
        private readonly bool _enableLog;
        private readonly BlockingCollection<string> _queue = new(new ConcurrentQueue<string>());
        private readonly Thread _writerThread;
        private StreamWriter _writer;
        private volatile bool _shuttingDown;

        public FileLoggingLogManager(bool enableLog) {
            _enableLog = enableLog;
            OpenLogFile();
            _writerThread = new Thread(ProcessQueue) {
                IsBackground = true,
                Name = "FileLoggingLogManager.Writer"
            };
            _writerThread.Start();
        }

        public Task<bool> Initialize() => Task.FromResult(true);

        public Task Initialize(float timeOut) => Task.FromResult(true);

        public void Destroy() {
            _shuttingDown = true;
            _queue.CompleteAdding();
            _writerThread?.Join(TimeSpan.FromSeconds(1));
            try {
                _writer?.Flush();
                _writer?.Dispose();
            } catch {
                // ignore
            }
        }

        public void Log(
            string message = "",
            string memberName = "",
            string sourceFilePath = "",
            int sourceLineNumber = 0) {
            var fileName = Path.GetFileName(sourceFilePath);
            var time = DateTime.Now.ToString("hh:mm:ss");
            var messageWithColon = message.Length == 0 ? "" : $": {message}";
            var fullMessage = $"{fileName}:{sourceLineNumber}-{time}: {memberName + messageWithColon}";
            Debug.Log(fullMessage);
            if (!_shuttingDown) {
                try {
                    _queue.Add(fullMessage);
                } catch (InvalidOperationException) {
                }
            }
        }

        private void OpenLogFile() {
            try {
                var projectRoot = Path.GetDirectoryName(Application.dataPath);
                var logsDir = Path.Combine(projectRoot!, "Logs");
                Directory.CreateDirectory(logsDir);
                var logPath = Path.Combine(logsDir, "app.log");
                var stream = new FileStream(logPath, FileMode.Create, FileAccess.Write, FileShare.Read);
                _writer = new StreamWriter(stream) { AutoFlush = false };
                _writer.WriteLine($"=== Session started {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
                _writer.Flush();
            } catch (Exception ex) {
                Debug.LogWarning($"[FileLoggingLogManager] Failed to open log file: {ex.Message}");
                _writer = null;
            }
        }

        private void ProcessQueue() {
            if (_writer == null) return;
            try {
                foreach (var line in _queue.GetConsumingEnumerable()) {
                    _writer.WriteLine(line);
                    if (_queue.Count == 0) {
                        _writer.Flush();
                    }
                }
                _writer.Flush();
            } catch (Exception ex) {
                try {
                    Debug.LogWarning($"[FileLoggingLogManager] Writer thread error: {ex.Message}");
                } catch {
                    // ignore
                }
            }
        }
    }
}
