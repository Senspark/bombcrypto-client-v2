#if UNITY_IOS
using AppleAuth.Editor;

using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace Editor {
    public static class AppleAuthPostProcessor {
        [PostProcessBuild(1)]
        public static void OnPostProcessBuild(BuildTarget target, string path) {
            var projectPath = PBXProject.GetPBXProjectPath(path);

            // Adds entitlement depending on the Unity version used
            var project = new PBXProject();
            project.ReadFromString(System.IO.File.ReadAllText(projectPath));
            var manager =
                new ProjectCapabilityManager(projectPath, "Entitlements.entitlements", null,
                    project.GetUnityMainTargetGuid());
            manager.AddSignInWithAppleWithCompatibility(project.GetUnityFrameworkTargetGuid());
            manager.WriteToFile();
        }
    }
}
#endif