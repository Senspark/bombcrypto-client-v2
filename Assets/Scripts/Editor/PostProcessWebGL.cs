using System.Collections.Generic;

using UnityEngine;

using UnityEditor;

using System.IO;
using System;
using System.Text.RegularExpressions;
using System.Linq;

namespace App.Editor {
    /// Original code by PlayItSafe_Fries
    /// modified by bourriquet and colinsenner
    /// https://forum.unity.com/threads/webgl-template.363057/
    /// https://gist.github.com/colinsenner/d779c1e974fb289448f0616adf985480
    public class PostProcessWebGL {
        // Fill in here the folder name from: `Assets/WebGLTemplates/<FOLDER>`
        // Hard-coding this variable is undesirable. Unity Cloud Build allows the user to define an environment variable,
        // which could be used here, but it's more to remember during setup and thus, the simpler hard-coded solution seems best.
        const string CUSTOM_TEMPLATE_NAME = "BombCrypto";

        static string TemplatePath =>
            Paths.Combine(Application.dataPath, "WebGLTemplates", CUSTOM_TEMPLATE_NAME);

#if UNITY_CLOUD_BUILD && UNITY_WEBGL
        // Make sure to add this to your Unity Cloud Build -> Advanced Options -> Post-Export Method -> `PostProcessWebGL.PostExport`
        public static void PostExport(string exportPath) {
            Debug.Log("PostProcessWebGL.PostExport() called.");

            Debug.LogFormat($"  PlayerSettings.WebGL.template = '{PlayerSettings.WebGL.template}'");
            Debug.LogFormat($"  TemplatePath: '{TemplatePath}'");

            // Clear the TemplateData folder, built by Unity.
            FileUtilExtended.CreateOrCleanDirectory(Paths.Combine(exportPath, "TemplateData"));

            // Copy contents from WebGLTemplate. Ignore all .meta files
            FileUtilExtended.CopyDirectoryFiltered(TemplatePath, exportPath, true, @".*/\.+|\.meta$", true);

            // Replace contents of index.html
            FixIndexHtml(exportPath);

            FixCss(exportPath);
        }
#endif

        // Replaces %...% defines in index.html
        static void FixIndexHtml(string exportPath) {
            const string buildFolderName = "Build";
            var buildDir = Paths.Combine(exportPath, buildFolderName);
            var replaceKeywordsMap = new Dictionary<string, string> {
                {@"\$MODULES_FOLDER\$", "Modules"},
                {"{{{ LOADER_FILENAME }}}", FindFileNameWithRegex(buildDir, @"[\w-]+\.loader\.js")},
                {"{{{ DATA_FILENAME }}}", FindFileNameWithRegex(buildDir, @"([0-9a-f]{32}|[\w-]+)\.data(\.gz)?")},
                {"{{{ FRAMEWORK_FILENAME }}}", FindFileNameWithRegex(buildDir, @"([0-9a-f]{32}|[\w-]+\.framework)\.js(\.gz)?")},
                {"{{{ CODE_FILENAME }}}", FindFileNameWithRegex(buildDir, @"([0-9a-f]{32}|[\w-]+)\.wasm(\.gz)?")},
                {@"#if MEMORY_FILENAME\n.*\n.*#endif", ""},
                {@"#if SYMBOLS_FILENAME\n.*\n.*#endif", ""},
                {@"#if BACKGROUND_FILENAME\n.*\n.*#endif", ""},
                {"{{{ COMPANY_NAME }}}", Application.companyName},
                {"{{{ PRODUCT_NAME }}}", Application.productName},
                {"{{{ PRODUCT_VERSION }}}", Application.version},
                {"{{{ WIDTH }}}", PlayerSettings.defaultWebScreenWidth.ToString()},
                {"{{{ HEIGHT }}}", PlayerSettings.defaultWebScreenWidth.ToString()},
            };

            var indexFilePath = Paths.Combine(exportPath, "index.html");
            if (File.Exists(indexFilePath)) {
                var content = File.ReadAllText(indexFilePath);
                content = replaceKeywordsMap.Aggregate(content,
                    (current, replace) =>
                        Regex.Replace(current, replace.Key, replace.Value));
                File.WriteAllText(indexFilePath, content);
            }
        }

        static void FixCss(string exportPath) {
            var cssFilePath = Paths.Combine(exportPath, "TemplateData", "style.css");
            var content = File.ReadAllText(cssFilePath);
            content = Regex.Replace(content, @"{{{ SPLASH_SCREEN_STYLE\.toLowerCase\(\) }}}", "dark");
            File.WriteAllText(cssFilePath, content);
        }

        private static string FindFileNameWithRegex(string buildDirectory, string regex) {
            var dir = new DirectoryInfo(buildDirectory);
            foreach (var f in dir.GetFiles()) {
                if (Regex.IsMatch(f.Name, regex)) {
                    return f.Name;
                }
            }
            return null;
        }

        private class FileUtilExtended {
            internal static void CreateOrCleanDirectory(string dir) {
                if (Directory.Exists(dir)) {
                    Directory.Delete(dir, true);
                }
                Directory.CreateDirectory(dir);
            }

            // Fix forward slashes on other platforms than windows
            private static string FixForwardSlashes(string unityPath) {
                return Application.platform != RuntimePlatform.WindowsEditor
                    ? unityPath
                    : unityPath.Replace("/", @"\");
            }

            // Copies the contents of one directory to another.
            public static void CopyDirectoryFiltered(string source, string target, bool overwrite,
                string regExExcludeFilter, bool recursive) {
                var excluder = new RegexMatcher {
                    exclude = null
                };
                try {
                    if (regExExcludeFilter != null) {
                        excluder.exclude = new Regex(regExExcludeFilter);
                    }
                } catch (ArgumentException) {
                    Debug.Log("CopyDirectoryRecursive: Pattern '" + regExExcludeFilter +
                              "' is not a correct Regular Expression. Not excluding any files.");
                    return;
                }
                CopyDirectoryFiltered(source, target, overwrite, excluder.CheckInclude, recursive);
            }

            private static void CopyDirectoryFiltered(string sourceDir, string targetDir, bool overwrite,
                Func<string, bool> filtercallback, bool recursive) {
                // Create directory if needed
                if (!Directory.Exists(targetDir)) {
                    Directory.CreateDirectory(targetDir);
                    overwrite = false;
                }

                // Iterate all files, files that match filter are copied.
                foreach (var filepath in Directory.GetFiles(sourceDir)) {
                    var localPath = filepath.Substring(sourceDir.Length);
                    if (filtercallback(localPath)) {
                        var fileName = Path.GetFileName(filepath);
                        var to = Path.Combine(targetDir, fileName);

                        File.Copy(FixForwardSlashes(filepath), FixForwardSlashes(to), overwrite);
                    }
                }

                // Go into sub directories
                if (recursive) {
                    foreach (var subdirectorypath in Directory.GetDirectories(sourceDir)) {
                        var localPath = subdirectorypath.Substring(sourceDir.Length);
                        if (filtercallback(localPath)) {
                            var directoryName = Path.GetFileName(subdirectorypath);
                            CopyDirectoryFiltered(Path.Combine(sourceDir, directoryName),
                                Path.Combine(targetDir, directoryName), overwrite, filtercallback, recursive);
                        }
                    }
                }
            }

            private struct RegexMatcher {
                public Regex exclude;

                public bool CheckInclude(string s) {
                    return exclude == null || !exclude.IsMatch(s);
                }
            }
        }

        private class Paths {
            // Combine multiple paths using Path.Combine
            public static string Combine(params string[] components) {
                if (components.Length < 1) {
                    throw new ArgumentException("At least one component must be provided!");
                }
                var str = components[0];
                for (var i = 1; i < components.Length; i++) {
                    str = Path.Combine(str, components[i]);
                }
                return str;
            }
        }
    }
}