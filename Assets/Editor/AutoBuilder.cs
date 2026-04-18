using UnityEditor;
using UnityEditor.Build.Reporting;
using System.Linq;
using UnityEngine;

public class AutoBuilder {
    public static void BuildWebGL() {
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        string buildPath = "C:/bomb/bombcrypto-client-v2/unity-web-template/public/webgl/build";
        if (!System.IO.Directory.Exists(buildPath)) {
            System.IO.Directory.CreateDirectory(buildPath);
            Debug.Log("Created " + buildPath);
        }
        BuildPlayerOptions opts = new BuildPlayerOptions();
        opts.scenes = scenes;
        opts.locationPathName = buildPath;
        opts.target = BuildTarget.WebGL;
        opts.options = BuildOptions.None;
        var report = BuildPipeline.BuildPlayer(opts);
        EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
    }
}
