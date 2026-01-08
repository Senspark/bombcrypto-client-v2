using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

#if UNITY_EDITOR
using System.Text.RegularExpressions;

using App;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
#endif

using UnityEngine;

using Debug = UnityEngine.Debug;

namespace Utils {
#if UNITY_EDITOR
    public static class BuildScripts {
        [MenuItem("Tools/Deploy WebGL")]
        public static void DeployWebGL() {
            var terminalPath = "/System/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal";
            var path = Path.Combine(Application.dataPath, "BashScripts", "Deploy.sh");
            var info = new ProcessStartInfo {
                FileName = terminalPath,
                Arguments = path,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
            };
            Process.Start(info);
        }
    }

    public class PreBuildProcessor : IPreprocessBuildWithReport {
        public int callbackOrder { get; }

        public void OnPreprocessBuild(BuildReport report) {
            AppConfig.Initialize();
            SetFirstScene();
            ReplaceFacebookProviderPackageName();
            ReplaceFirebaseProjectAppId();
            var now = DateTime.Now;
            WriteBuildTimeStamp(now);
            WriteVersionCode(now);
            SetAndroidPackageNameAndVersionCode();
            ChangeIconAtBuildTime();
            ChangeTitleAtBuildTime();
        }

#if UNITY_CLOUD_BUILD
        public static void InitializeCloudBuild() {
            AppConfig.Initialize();
            // var now = DateTime.Now;
            // WriteBuildTimeStamp(now);
            // WriteVersionCode(now);
            // ChangeIconAtBuildTime();
            // ChangeTitleAtBuildTime();
        }
#endif // UNITY_CLOUD_BUILD

        private static void WriteBuildTimeStamp(DateTime dateTime) {
            var now = dateTime.ToString("HH:mm:ss dd/MM/yyyy");
            var filePath = "Assets/Resources/configs/BuildTimeStamp.txt";
            using var stream = File.CreateText(filePath);
            stream.Write(now);
            stream.Close();
        }

        private static void WriteVersionCode(DateTime dateTime) {
            const string fileFullPath = "Assets/Resources/configs/AppConfig.json";
            var version = VersionUtils.Parse(dateTime);
            Debug.Log($"version: {version}");
            var fileStream = new FileStream(fileFullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            var reader = new StreamReader(fileStream);
            var config = JObject.Parse(reader.ReadToEnd());
            var buildInfo = config["buildInfo"] ?? throw new Exception();
            reader.Close();
            buildInfo["productionVersion"] = version;
            buildInfo["testVersion"] = version;
#if IS_PRODUCTION
            config["isProduction"] = true;
#else
            // config["isProduction"] = false;
#endif
            Debug.Log(JsonConvert.SerializeObject(config));
            var writer = File.CreateText(fileFullPath);
            writer.Write(JsonConvert.SerializeObject(config, Formatting.Indented));
            writer.Close();
        }

        private static void SetAndroidPackageNameAndVersionCode() {
            // PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, AppConfig.GetPackageName());
            // PlayerSettings.Android.bundleVersionCode = AppConfig.GetAndroidBundleCode();
        }

        private static void ChangeIconAtBuildTime() {
            // var replacePath = AppConfig.GetIconPath();
            // var originalPath = "Assets/Textures/AppIcon/ic_launcher_foreground.png";
            // FileUtil.ReplaceFile(replacePath, originalPath);
        }

        private static void ChangeTitleAtBuildTime() {
            // PlayerSettings.productName = AppConfig.GetProductName();
        }
        
        private static void ReplaceFirebaseProjectAppId() {
            // var networkType = AppConfig.NetworkType;
            // var name = networkType switch {
            //     NetworkType.Binance => "binance",
            //     NetworkType.Polygon => "polygon",
            //     _ => throw new InvalidEnumArgumentException(networkType.ToString())
            // };
            // var googleServiceJsonDestPath = "Assets/StreamingAssets/google-services.json";
            // var googleServiceDesktopDestPath = "Assets/StreamingAssets/google-services-desktop.json";
            // var googleServiceXmlDestPath = "Assets/Plugins/Android/FirebaseApp.androidlib/res/values/google-services.xml";
            //
            // var googleServiceJsonSrcPath = $"Assets/Resources/Firebase/google-services-json-{name}.json";
            // var googleServiceDesktopSrcPath = $"Assets/Resources/Firebase/google-services-desktop-{name}.json";
            // var googleServiceXmlSrcPath = $"Assets/Resources/Firebase/google-services-xml-{name}.xml";
            //
            // FileUtil.ReplaceFile(googleServiceJsonSrcPath, googleServiceJsonDestPath);
            // FileUtil.ReplaceFile(googleServiceDesktopSrcPath, googleServiceDesktopDestPath);
            // FileUtil.ReplaceFile(googleServiceXmlSrcPath, googleServiceXmlDestPath);
        }
        
        private static void SetFirstScene()
        {
            // string firstScenePath = AppConfig.IsTon() ? "Assets/Scenes/ConnectScene-Portrait.unity" : "Assets/Scenes/ConnectScene.unity";
            //
            // EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            // for (int i = 0; i < scenes.Length; i++)
            // {
            //     if (scenes[i].path == firstScenePath)
            //     {
            //         (scenes[0], scenes[i]) = (scenes[i], scenes[0]);
            //         break;
            //     }
            // }
            //
            // EditorBuildSettings.scenes = scenes;
            // Debug.Log("First scene set to: " + firstScenePath);
        }

        private static void ReplaceFacebookProviderPackageName() {
            // const string filePath = "Assets/Plugins/Android/AndroidManifest.xml";
            // const string firstTag = "<provider android:name=\"com.facebook.FacebookContentProvider\" android:authorities=";
            // const string secondTag = " android:exported=\"true\" />";
            // var packageName = AppConfig.GetPackageName();
            // var providerName = $"\"{packageName}.facebook.app.FacebookContentProvider\"";
            // var content = File.ReadAllText(filePath);
            // var newContent = Regex.Replace(content, $"(?<={firstTag})(.*)(?={secondTag})", providerName);
            // File.WriteAllText(filePath, newContent);
        }
    }

    public static class PostBuildProcessor {
        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
            if (target != BuildTarget.WebGL) {
                return;
            }
            const string tag = "$MODULES_FOLDER$";
            var buildTime = DateTime.Now.ToString("dd.MM.HH.mm.ss");
            const string oldFolderName = "Modules";
            var newFolderName = $"{oldFolderName}.{buildTime}";

            // Xóa folder Modules cũ
            var searchDirectories = new DirectoryInfo(pathToBuiltProject);
            var files = searchDirectories.GetFileSystemInfos($"*{oldFolderName}*");
            foreach (var f in files) {
                if (f is not DirectoryInfo) {
                    continue;
                }
                if (f.Name == oldFolderName) {
                    continue;
                }
                Directory.Delete(f.FullName, true);
            }

            // Sửa src=Main.js trong index.html -> src='./Modules.dd.MM.HH.mm.ss/Main.js'
            var indexHtmlFilePath = Path.Combine(pathToBuiltProject, "index.html");
            var text = File.ReadAllText(indexHtmlFilePath);
            text = text.Replace(tag, newFolderName);
            File.WriteAllText(indexHtmlFilePath, text);

            // Đổi tên thư mục Modules -> Modules.dd.MM.HH.mm.ss
            var oldModulesFolderPath = Path.Combine(pathToBuiltProject, oldFolderName);
            var newModulesFolderPath = Path.Combine(pathToBuiltProject, newFolderName);
            Directory.Move(oldModulesFolderPath, newModulesFolderPath);
        }
    }
#endif
}