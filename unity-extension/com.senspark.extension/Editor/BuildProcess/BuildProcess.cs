using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Senspark.Editor.BuildProcess {
    public class BuildProcess : IPreprocessBuildWithReport, IPostprocessBuildWithReport {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report) {
            AndroidDebugKeyBuildProcess.AddDefineSymbol();
        }

        public void OnPostprocessBuild(BuildReport report) {
            AndroidDebugKeyBuildProcess.RemoveDefineSymbol();
        }
    }
}