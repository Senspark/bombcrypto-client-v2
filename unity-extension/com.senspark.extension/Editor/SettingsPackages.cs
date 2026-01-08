using System;
using System.IO;

namespace Senspark.Editor {
    public static class SettingsPackages {
        public static string GetPackageName(SensparkPackages pkg) {
            return pkg switch {
                SensparkPackages.EDM4U => "com.google.external-dependency-manager",
                SensparkPackages.FirebaseApp => "com.google.firebase.app.ex",
                SensparkPackages.FirebaseAnalytics => "com.google.firebase.analytics.ex",
                SensparkPackages.FirebaseCrashlytics => "com.google.firebase.crashlytics.ex",
                SensparkPackages.FirebaseRemoteConfig => "com.google.firebase.remote-config.ex",
                SensparkPackages.Admob => "com.senspark.admob",
                SensparkPackages.AppLovin => "com.senspark.applovin",
                SensparkPackages.AppsFlyer => "appsflyer-unity-plugin",
                SensparkPackages.InAppPurchase => "com.senspark.iap",
                SensparkPackages.MobileNotification => "com.senspark.notification",
                SensparkPackages.ALINE => "com.arongranberg.aline",
                SensparkPackages.AssetUsageDetector => "com.yasirkula.assetusagedetector",
                SensparkPackages.BuildReportTool => "com.anomalousunderdog.buildreporttool",
                SensparkPackages.EditorConsolePro => "com.flyingworm.editorconsolepro",
                SensparkPackages.Graphy => "com.tayx.graphy",
                SensparkPackages.InGameDebugConsole => "com.yasirkula.ingamedebugconsole",
                SensparkPackages.ProCamera2D => "com.luispedrofonseca.procamera2d",
                SensparkPackages.SuperTilemapEditor => "com.creativespore.supertilemapeditor",
                _ => throw new ArgumentOutOfRangeException(nameof(pkg), pkg, null)
            };
        }
    }

    public enum SensparkPackages {
        EDM4U,
        FirebaseApp,
        FirebaseAnalytics,
        FirebaseCrashlytics,
        FirebaseRemoteConfig,
        Admob,
        AppLovin,
        AppsFlyer,
        InAppPurchase,
        MobileNotification,
        ALINE,
        AssetUsageDetector,
        BuildReportTool,
        EditorConsolePro,
        Graphy,
        InGameDebugConsole,
        ProCamera2D,
        SuperTilemapEditor,
    }
}