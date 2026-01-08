using System.IO;

namespace Senspark.Editor {
    public static class SettingsEditorHelper {
        public static bool IsPackageInstalled(string package) {
            const string path = "Packages/manifest.json";
            if (!File.Exists(path)) {
                return false;
            }
            var text = File.ReadAllText(path);
            return text.Contains(package);
        }

        public static bool IsPackageInstalled(SensparkPackages pkg) {
            return IsPackageInstalled(SettingsPackages.GetPackageName(pkg));
        }
        
        public static string GetPackagePath(SensparkPackages pkg) {
            return Path.Combine("Packages", SettingsPackages.GetPackageName(pkg));
        }
    }
}