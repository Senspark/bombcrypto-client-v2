using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using App;

using UnityEditor;
using UnityEditor.U2D;

using UnityEngine;
using UnityEngine.U2D;

namespace Editor {
    public class SolanaBuildWindow : EditorWindow {
        private string _buildName = "solana";
        private bool _useBrotli = true;
        private bool _developerMode = false;
        private bool _autoConnectProfiler = false;
        private bool _deepProfiling = false;
        private TextureCompression _textureCompression = TextureCompression.ASTC;
        private WebGLPowerPreference _powerPreference = WebGLPowerPreference.LowPower;
        private int _maximumMemorySize = 2048;
        private WebGLExceptionSupport _exceptionSupport = WebGLExceptionSupport.FullWithoutStacktrace;
        private bool _forTestOnly = false;


        [MenuItem("Build/Build for Solana")]
        public static void ShowWindowSolana() {
            GetWindow<SolanaBuildWindow>("Solana Build Window");
        }

        private void OnGUI() {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Enter Build Name", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Build Name", GUILayout.Width(200));
            _buildName = EditorGUILayout.TextField(_buildName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Use Brotli", GUILayout.Width(200));
            _useBrotli = EditorGUILayout.Toggle(_useBrotli);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Developer Mode", GUILayout.Width(200));
            _developerMode = EditorGUILayout.Toggle(_developerMode);
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginDisabledGroup(!_developerMode);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Auto Connect Profiler", GUILayout.Width(200));
            _autoConnectProfiler = EditorGUILayout.Toggle(_autoConnectProfiler);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Deep Profiling", GUILayout.Width(200));
            _deepProfiling = EditorGUILayout.Toggle(_deepProfiling);
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Texture Compression", GUILayout.Width(200));
            _textureCompression = (TextureCompression)EditorGUILayout.EnumPopup(_textureCompression);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Power Preference", GUILayout.Width(200));
            _powerPreference = (WebGLPowerPreference)EditorGUILayout.EnumPopup(_powerPreference);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Maximum Memory Size (MB)", GUILayout.Width(200));
            _maximumMemorySize = EditorGUILayout.IntField(_maximumMemorySize);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Enable Exceptions", GUILayout.Width(200));
            _exceptionSupport = (WebGLExceptionSupport)EditorGUILayout.EnumPopup(_exceptionSupport);
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(30);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("For test only", GUILayout.Width(200));
            _forTestOnly = EditorGUILayout.Toggle(_forTestOnly);
            EditorGUILayout.EndHorizontal();

            if (_forTestOnly) {
                GUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Before build")) {
                    ComboBuildSolana.BeforeBuild(_useBrotli);
                }
                if (GUILayout.Button("After build")) {
                    ComboBuildSolana.AfterBuild();
                }
                if (GUILayout.Button("Do both")) {
                    ComboBuildSolana.BeforeBuild(_useBrotli);
                    ComboBuildSolana.AfterBuild();
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Build", GUILayout.Height(50))) {
                ComboBuildSolana.BuildGame(_buildName, _useBrotli, _developerMode, _autoConnectProfiler, _deepProfiling,
                    _textureCompression, _powerPreference,
                    _maximumMemorySize, _exceptionSupport);
            }
        }

        public class ComboBuildSolana {
            private static readonly Dictionary<string, TextureImporterCompression> OriginalCompressionSettings = new();

            private static readonly Dictionary<string, bool> OriginalIncludeInBuildSettings = new();

            //Danh sách các atlas cần phải compress khi build SOL (Compress Normal Quality)
            private static readonly List<string> SpriteAtlasNames = new List<string> {
            };

            //Danh sách chứa các atlas ko cần build cho bản SOL
            private static readonly List<string> ExcludeFromBuildAtlasNames = new List<string> {
                "BomberLand_UI", "Icons", "Misc_UI", "Objects",
                "Others", "Polygon_UI", "Resources", "UI", "UI_MainMenu",
                "LoadingAnim", "Character", "TonAsset",
                "BorderAmazon", "FireAndBricks",
            };

            // Các folder cần di chuyển ra khỏi Resources trước khi build Ton
            private static readonly List<string> FoldersToMove = new List<string> {
                "Assets/Resources/Blockchain",
                "Assets/Resources/Firebase",
                "Assets/Resources/Data",

                "Assets/Resources/Prefabs/Announcements",
                "Assets/Resources/Prefabs/Bcoin",
                "Assets/Resources/Prefabs/Enemies",
                "Assets/Resources/Prefabs/StoryMode",
                "Assets/Resources/Prefabs/Ghost",
                "Assets/Resources/Prefabs/PvpMode",
                "Assets/Resources/Prefabs/Stream",
                "Assets/Resources/Prefabs/TrailEffect",
                "Assets/Resources/Prefabs/UI",

                "Assets/TextMesh Pro/Resources/Sprite Assets",
                "Assets/TextMesh Pro/Resources/TMP Settings",
            };

            private static readonly List<string> ScenesToActivate = new List<string> {
                "Assets/Scenes/ConnectScene/ConnectScene.unity",
                "Assets/Scenes/TreasureModeScene/TreasureModeScene.unity",
            };

            // Các thư mục resource ko cần cho bản build Sol liệt kê dứoi đây để bỏ qua khi build
            private static readonly List<string> FoldersToRename = new List<string> {
                "Assets/Textures/Resources"
            };

            private static List<EditorBuildSettingsScene> originalScenes;

            public static void BuildGame(string buildName, bool useBrotli, bool developerMode, bool autoConnectProfiler,
                bool deepProfiling,
                TextureCompression textureCompression, WebGLPowerPreference powerPreference, int maximumMemorySize,
                WebGLExceptionSupport exceptionSupport) {
                AppConfig.Initialize();

                if (!AppConfig.IsSolana()) {
                    EditorUtility.DisplayDialog("Build Error",
                        "This build is only for Solana builds.\nGo to AppConfig.json change gamePlatform -> SOL",
                        "OK");
                    return;
                }

                EditorUserBuildSettings.development = developerMode;
                PlayerSettings.WebGL.powerPreference = powerPreference;
                PlayerSettings.WebGL.memorySize = maximumMemorySize;
                PlayerSettings.WebGL.exceptionSupport = exceptionSupport;

                try {
                    BeforeBuild(useBrotli);

                    if (textureCompression == TextureCompression.BOTH) {
                        BuildBoth(buildName);
                    } else {
                        Build(buildName, developerMode, autoConnectProfiler, deepProfiling, textureCompression);
                    }
                } catch (Exception e) {
                    Debug.LogError($"Lỗi khi build Solana: {e.Message}");
                    Debug.LogError("Revert lại các thay đổ atlas và compression");
                } finally {
                    AfterBuild();
                }
            }
            
            public static void BeforeBuild(bool useBrotli) {
                SetupBeforeBuild(useBrotli);
                UpdateSpriteAtlasCompression(SpriteAtlasNames, TextureImporterCompression.Compressed);
                UpdateIncludeInBuildOption(ExcludeFromBuildAtlasNames, false);
                DeactivateAllScenesExcept(ScenesToActivate);
                MoveFoldersOut(FoldersToMove);
                RenameFoldersBeforeBuild(FoldersToRename);
                Debug.Log("Setup build solana complete");
            }
            
            public static void AfterBuild() {
                RevertSpriteAtlasCompression(SpriteAtlasNames);
                RevertIncludeInBuildOption(ExcludeFromBuildAtlasNames);
                ReactivateAllScenes();
                MoveFoldersBack(FoldersToMove);
                RenameFoldersAfterBuild(FoldersToRename);
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
                Debug.Log("Build solana complete");
            }
            

            private static void MoveFoldersOut(List<string> folders) {
                string tempPath = "Assets/TempResources";

                foreach (var folder in folders) {
                    var relativePath = folder.Replace("Assets/", string.Empty);
                    var destinationFolderPath = Path.Combine(tempPath, relativePath);

                    // Ensure the destination directory exists
                    var destinationDir = Path.GetDirectoryName(destinationFolderPath);
                    if (!Directory.Exists(destinationDir)) {
                        if (destinationDir != null) Directory.CreateDirectory(destinationDir);
                    }

                    if (Directory.Exists(folder)) {
                        Directory.Move(folder, destinationFolderPath);
                    }
                }

                // Rename any folder named "Resources" to "TEMP"
                RenameFolders(tempPath, "Resources", "TEMP");
            }

            private static void MoveFoldersBack(List<string> folders) {
                string tempPath = "Assets/TempResources";

                // Rename any folder named "TEMP" back to "Resources"
                RenameFolders(tempPath, "TEMP", "Resources");

                foreach (var folder in folders) {
                    var relativePath = folder.Replace("Assets/", string.Empty);
                    var sourceFolderPath = Path.Combine(tempPath, relativePath);

                    if (Directory.Exists(sourceFolderPath)) {
                        if (Directory.Exists(folder)) {
                            Directory.Delete(folder, true);
                        }
                        Directory.Move(sourceFolderPath, folder);
                    }
                }

                tempPath = "Assets/TempResources";
                if (Directory.Exists(tempPath)) {
                    Directory.Delete(tempPath, true);
                }
            }

            private static void RenameFolders(string rootPath, string oldName, string newName) {
                var directories = Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories);
                foreach (var dir in directories) {
                    if (Path.GetFileName(dir).Equals(oldName, StringComparison.OrdinalIgnoreCase)) {
                        var newDir = Path.Combine(Path.GetDirectoryName(dir)!, newName);
                        Directory.Move(dir, newDir);
                    }
                }
            }

            private static void DeactivateAllScenesExcept(List<string> scenesToActivate) {
                originalScenes = EditorBuildSettings.scenes.ToList();
                var newScenes = new List<EditorBuildSettingsScene>();

                foreach (var scene in originalScenes) {
                    if (scenesToActivate.Contains(scene.path)) {
                        newScenes.Add(new EditorBuildSettingsScene(scene.path, true));
                    } else {
                        newScenes.Add(new EditorBuildSettingsScene(scene.path, false));
                    }
                }

                EditorBuildSettings.scenes = newScenes.ToArray();
            }

            private static void ReactivateAllScenes() {
                EditorBuildSettings.scenes = originalScenes.ToArray();
            }

            /// <summary>
            /// Compression các atlas ko dùng cho TON khi build
            /// </summary>
            /// <param name="spriteAtlasNames"></param>
            /// <param name="compression"></param>
            private static void UpdateSpriteAtlasCompression(List<string> spriteAtlasNames,
                TextureImporterCompression compression) {
                string[] guids = AssetDatabase.FindAssets("t:SpriteAtlas");
                OriginalCompressionSettings.Clear();
                foreach (string guid in guids) {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    SpriteAtlas spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
                    if (spriteAtlas != null && spriteAtlasNames.Contains(spriteAtlas.name)) {
                        var settings = spriteAtlas.GetPlatformSettings("DefaultTexturePlatform");
                        if (!OriginalCompressionSettings.ContainsKey(spriteAtlas.name)) {
                            OriginalCompressionSettings[spriteAtlas.name] = settings.textureCompression;
                        }
                        settings.textureCompression = compression;
                        spriteAtlas.SetPlatformSettings(settings);
                        EditorUtility.SetDirty(spriteAtlas);
                    }
                }
                AssetDatabase.SaveAssets();
            }

            /// <summary>
            /// Revert lại compression cho các atlas đã đổi khi nãy
            /// </summary>
            /// <param name="spriteAtlasNames"></param>
            private static void RevertSpriteAtlasCompression(List<string> spriteAtlasNames) {
                string[] guids = AssetDatabase.FindAssets("t:SpriteAtlas");
                foreach (string guid in guids) {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    SpriteAtlas spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
                    if (spriteAtlas != null && spriteAtlasNames.Contains(spriteAtlas.name) &&
                        OriginalCompressionSettings.ContainsKey(spriteAtlas.name)) {
                        var settings = spriteAtlas.GetPlatformSettings("DefaultTexturePlatform");
                        settings.textureCompression = OriginalCompressionSettings[spriteAtlas.name];
                        spriteAtlas.SetPlatformSettings(settings);
                        EditorUtility.SetDirty(spriteAtlas);
                    }
                }
                AssetDatabase.SaveAssets();
            }

            /// <summary>
            /// Bỏ các atlas ko cần cho bản build TON
            /// </summary>
            /// <param name="spriteAtlasNames"></param>
            /// <param name="includeInBuild"></param>
            private static void UpdateIncludeInBuildOption(List<string> spriteAtlasNames, bool includeInBuild) {
                string[] guids = AssetDatabase.FindAssets("t:SpriteAtlas");
                OriginalIncludeInBuildSettings.Clear();
                foreach (string guid in guids) {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    SpriteAtlas spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
                    if (spriteAtlas != null && spriteAtlasNames.Contains(spriteAtlas.name)) {
                        if (!OriginalIncludeInBuildSettings.ContainsKey(spriteAtlas.name)) {
                            OriginalIncludeInBuildSettings[spriteAtlas.name] = spriteAtlas.IsIncludeInBuild();
                        }
                        spriteAtlas.SetIncludeInBuild(includeInBuild);
                        EditorUtility.SetDirty(spriteAtlas);
                    }
                }
                AssetDatabase.SaveAssets();
            }

            private static void RevertIncludeInBuildOption(List<string> spriteAtlasNames) {
                string[] guids = AssetDatabase.FindAssets("t:SpriteAtlas");
                foreach (string guid in guids) {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    SpriteAtlas spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
                    if (spriteAtlas != null && spriteAtlasNames.Contains(spriteAtlas.name) &&
                        OriginalIncludeInBuildSettings.ContainsKey(spriteAtlas.name)) {
                        spriteAtlas.SetIncludeInBuild(OriginalIncludeInBuildSettings[spriteAtlas.name]);
                        EditorUtility.SetDirty(spriteAtlas);
                    }
                }
                AssetDatabase.SaveAssets();
            }

            private static void RenameFoldersBeforeBuild(List<string> folders) {
                foreach (var folder in folders) {
                    var tempFolder = folder.Replace("Resources", "TEMP");
                    if (Directory.Exists(tempFolder)) {
                        Directory.Delete(tempFolder, true); // Delete the existing directory
                    }
                    if (Directory.Exists(folder)) {
                        Directory.Move(folder, tempFolder);
                    }
                }
            }

            private static void RenameFoldersAfterBuild(List<string> folders) {
                foreach (var folder in folders) {
                    var tempFolder = folder.Replace("Resources", "TEMP");
                    if (Directory.Exists(tempFolder)) {
                        if (Directory.Exists(folder)) {
                            Directory.Delete(folder, true); // Delete the existing directory
                        }
                        Directory.Move(tempFolder, folder);
                    }
                }
            }

            /// <summary>
            /// Chỉnh sang format brotli trước khi build
            /// </summary>
            /// <param name="useBrotli"></param>
            private static void SetupBeforeBuild(bool useBrotli) {
                PlayerSettings.WebGL.compressionFormat =
                    useBrotli ? WebGLCompressionFormat.Brotli : WebGLCompressionFormat.Disabled;
            }

            /// <summary>
            /// Chỉ build 1 bản theo dạng compress chỉ định
            /// </summary>
            /// <param name="buildName"></param>
            /// <param name="developerMode"></param>
            /// <param name="autoConnectProfiler"></param>
            /// <param name="textureCompression"></param>
            private static void Build(string buildName, bool developerMode, bool autoConnectProfiler,
                bool deepProfiling, TextureCompression textureCompression) {
                string name = buildName;
                WebGLTextureSubtarget textureSubtarget = WebGLTextureSubtarget.ASTC;
                if (textureCompression == TextureCompression.DXT) {
                    name = buildName + "_dxt";
                    textureSubtarget = WebGLTextureSubtarget.DXT;
                } else if (textureCompression == TextureCompression.ASTC) {
                    name = buildName + "_astc";
                    textureSubtarget = WebGLTextureSubtarget.ASTC;
                }

                EditorUserBuildSettings.webGLBuildSubtarget = textureSubtarget;
                BuildPipeline.BuildPlayer(
                    GetBuildPlayerOptions(name, developerMode, autoConnectProfiler, deepProfiling));
            }

            /// <summary>
            /// Build cả 2 bản astc và dtx để support các máy cũ
            /// </summary>
            /// <param name="buildName"></param>
            private static void BuildBoth(string buildName) {
                string dtxBuildName = buildName;
                string astcBuildName = dtxBuildName + "_astc";

                EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.DXT;
                BuildPipeline.BuildPlayer(GetBuildPlayerOptions(dtxBuildName));

                EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.ASTC;
                BuildPipeline.BuildPlayer(GetBuildPlayerOptions(astcBuildName));

                CopyBuildData(GetBuildPath(astcBuildName) + "/Build", GetBuildPath(dtxBuildName));
            }

            /// <summary>
            /// Copy file data của bản build astc sang thư mục build dtx để sử dụng
            /// </summary>
            /// <param name="sourcePath"></param>
            /// <param name="destinationPath"></param>
            private static void CopyBuildData(string sourcePath, string destinationPath) {
                string[] files = Directory.GetFiles(sourcePath, "*.data")
                    .Concat(Directory.GetFiles(sourcePath, "*.data.br"))
                    .ToArray();

                foreach (string file in files) {
                    string fileName = Path.GetFileName(file);
                    string destFile = Path.Combine(destinationPath, fileName);
                    FileUtil.CopyFileOrDirectory(file, destFile);
                }
            }

            private static BuildPlayerOptions GetBuildPlayerOptions(string buildName, bool developerMode = false,
                bool autoConnectProfiler = false, bool deepProfiling = false) {
                var options = BuildOptions.None;
                options |= developerMode ? BuildOptions.Development : 0;
                if (developerMode) {
                    options |= autoConnectProfiler ? BuildOptions.ConnectWithProfiler : 0;
                    options |= deepProfiling ? BuildOptions.EnableDeepProfilingSupport : 0;
                }
                return new BuildPlayerOptions {
                    scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
                    locationPathName = GetBuildPath(buildName),
                    target = BuildTarget.WebGL,
                    //options = BuildOptions.None
                    options = options
                };
            }

            private static string GetBuildPath(string buildName) {
                return Path.Combine("Build", buildName);
            }
        }
    }
}