using System;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

using UnityEngine;

namespace App.EditorTools {
    // Sau khi build WebGL: strip comment + minify runtime-config.json trong output.
    // File nguồn (Assets/StreamingAssets) giữ comment để làm tài liệu; bản ship sạch.
    public class RuntimeConfigPostprocessor : IPostprocessBuildWithReport {
        public int callbackOrder => 0;

        private const string RelativePath = "StreamingAssets/configs/runtime-config.json";

        public void OnPostprocessBuild(BuildReport report) {
            if (report.summary.platform != BuildTarget.WebGL) {
                return;
            }
            var configPath = Path.Combine(report.summary.outputPath, RelativePath);
            if (!File.Exists(configPath)) {
                return;
            }
            try {
                var raw = File.ReadAllText(configPath);
                var token = JToken.Parse(raw, new JsonLoadSettings {
                    CommentHandling = CommentHandling.Ignore
                });
                File.WriteAllText(configPath, token.ToString(Formatting.None));
                Debug.Log($"[RuntimeConfig] Stripped comments + minified: {configPath}");
            } catch (Exception e) {
                Debug.LogWarning($"[RuntimeConfig] Skip strip ({e.Message})");
            }
        }
    }
}
