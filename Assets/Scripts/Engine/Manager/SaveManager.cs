
using System;
using System.IO;
using System.Text;

using Cysharp.Threading.Tasks;

using UnityEngine;

public static class SaveLoadManager {
    private static string _content;
    private static string _fullPath;
    private static byte[] _binary;
    private const string Extension = ".bin";

    public static void Initialize() { }
    private static string GetPath(string filename) {
        return Application.persistentDataPath + "/GameData/" + filename;
    }

#region Normal Version
    
    public static T Load<T>(string filename, bool encryption = false, string key = "") {
        try {
            _content = ReadFile(GetPath(filename), encryption, key);
            
            if (string.IsNullOrEmpty(_content) || _content == "{}") {
                if (typeof(T).IsClass) {
                    // T is a reference type, create and return a new instance
                    return Activator.CreateInstance<T>();
                }
                // T is a value type, return the default value
                return default;
            }
            
            T res = JsonUtility.FromJson<T>(_content);
            
            return res;
        } catch (Exception e) {
            Debug.LogWarning($"Load file {filename} fail with error: ${e}");
            return default;
        }
        
    }

    public static void Save<T>(T toSave, string filename, bool encryption = false, string key = "") {
        _content = JsonUtility.ToJson(toSave);
        WriteFile(GetPath(filename), _content, encryption, key);
    }

    private static void WriteFile(string path, string content, bool encryption, string key) {
        _fullPath = path + Extension;

        // Ensure the directory exists, creating it if necessary
        if (!File.Exists(_fullPath)) {
            Directory.CreateDirectory(Path.GetDirectoryName(_fullPath) ?? string.Empty);
        }
        // if (encryption) {
        //     _binary = SEngine.EncryptionToBinary(content, key);
        // }
        // else
            _binary = Encoding.UTF8.GetBytes(content);

        File.WriteAllBytes(_fullPath, _binary);
    }

    private static string ReadFile(string path, bool encryption, string key) {
        _fullPath = path + Extension;
        if (File.Exists(_fullPath)) {
            _binary = File.ReadAllBytes(_fullPath);
            string content;
            // if (encryption) {
            //     content = SEngine.DecryptionFromBinary(_binary, key);
            // } 
            // else {
                content = Encoding.UTF8.GetString(_binary);
            //}
            return content;
        }
        return "";
    }

    #endregion
    
#region Async version

    public static async UniTask SaveAsync<T>(T toSave, string filename, bool encryption = false, string key = "") {
        string content = JsonUtility.ToJson(toSave);
        await WriteFileAsync(GetPath(filename), content, encryption, key);
    }

    public static async UniTask<T> LoadAsync<T>(string filename, bool encryption = false, string key = "") {
        try {
            string content = await ReadFileAsync(GetPath(filename), encryption, key);
            
            if (string.IsNullOrEmpty(content) || content == "{}") {
                if (typeof(T).IsClass) {
                    // T is a reference type, create and return a new instance
                    return Activator.CreateInstance<T>();
                } else {
                    // T is a value type, return the default value
                    return default;
                }
            }
            
            T res = JsonUtility.FromJson<T>(content);
            
            return res;
        } catch (Exception e) {
            return default;
        }
        
    }

    private static async UniTask WriteFileAsync(string path, string content, bool encryption, string key) {
        var fullPath = path + Extension;

        // Ensure the directory exists, creating it if necessary
        if (!File.Exists(fullPath)) {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? string.Empty);
        }

        byte[] binary;
        // if (encryption) {
        //     binary = SEngine.EncryptionToBinary(content, key);
        // }
        // else
            binary = Encoding.UTF8.GetBytes(content);

        await using FileStream sourceStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None,
            bufferSize: 4096, useAsync: true);
        await sourceStream.WriteAsync(binary, 0, binary.Length);
    }

    private static async UniTask<string> ReadFileAsync(string path, bool encryption, string key) {
        var fullPath = path + Extension;
        if (File.Exists(fullPath)) {
            byte[] binary = await File.ReadAllBytesAsync(fullPath);
            string content;
            // if (encryption) {
            //     content = SEngine.DecryptionFromBinary(binary, key);
            // } 
            // else {
                content = Encoding.UTF8.GetString(binary);
            //}
            return content;
        }
        return "";
    }

    #endregion

    
}