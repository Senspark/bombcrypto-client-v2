using System;

using UnityEditor;
using UnityEditor.PackageManager;

using UnityEngine;

namespace Senspark.Editor {
    [CustomEditor(typeof(Settings))]
    public class SettingsEditor : UnityEditor.Editor {
        [MenuItem("Tools/Senspark/Settings")]
        public static void OpenInspector() {
            Selection.activeObject = Settings.Instance;
        }

        public override void OnInspectorGUI() {
            var settings = Settings.Instance;
            if (GUILayout.Button("Update Player Settings")) {
                settings.UpdatePlayerSettings();
            }

            // Packages
            EditorGUILayout.LabelField("Packages");
            ++EditorGUI.indentLevel;
            void AddPackage(string label, SensparkPackages pkg) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(label);
                var package = SettingsPackages.GetPackageName(pkg);
                if (SettingsEditorHelper.IsPackageInstalled(pkg)) {
                    if (GUILayout.Button("Uninstall", GUILayout.MaxWidth(100))) {
                        Client.Remove(package);
                    }
                } else {
                    if (GUILayout.Button("Install", GUILayout.MaxWidth(100))) {
                        Client.Add($"file:../unity-extension/{package}");
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            AddPackage("EDM4U", SensparkPackages.EDM4U);
            AddPackage("Firebase App", SensparkPackages.FirebaseApp);
            AddPackage("Firebase Analytics", SensparkPackages.FirebaseAnalytics);
            AddPackage("Firebase Crashlytics", SensparkPackages.FirebaseCrashlytics);
            AddPackage("Firebase Remote Config", SensparkPackages.FirebaseRemoteConfig);
            AddPackage("AdMob", SensparkPackages.Admob);
            AddPackage("AppLovin", SensparkPackages.AppLovin);
            AddPackage("AppsFlyer", SensparkPackages.AppsFlyer);
            AddPackage("In App Purchase", SensparkPackages.InAppPurchase);
            AddPackage("Mobile Notification", SensparkPackages.MobileNotification);
            AddPackage("ALINE", SensparkPackages.ALINE);
            AddPackage("Asset Usage Detector", SensparkPackages.AssetUsageDetector);
            AddPackage("Build Report Tool", SensparkPackages.BuildReportTool);
            AddPackage("Editor Console Pro", SensparkPackages.EditorConsolePro);
            AddPackage("Graphy", SensparkPackages.Graphy);
            AddPackage("Ingame Debug Console", SensparkPackages.InGameDebugConsole);
            AddPackage("Pro Camera 2D", SensparkPackages.ProCamera2D);
            AddPackage("Super Tilemap Editor", SensparkPackages.SuperTilemapEditor);
            --EditorGUI.indentLevel;

            // Notifications
            {
                GUILayout.Space(10);

                EditorGUILayout.LabelField("Notifications");
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));
                GUILayout.Space(20);

                EditorGUILayout.BeginVertical(GUILayout.Width(80));
                EditorGUILayout.LabelField("Icon", GUILayout.Width(40));
                settings.NotificationIcon = (Texture2D)EditorGUILayout.ObjectField(settings.NotificationIcon,
                    typeof(Texture2D), false, GUILayout.Width(70), GUILayout.Height(70));
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(20);
                RenderBanners();
                
                GUILayout.Space(20);
                RenderNotificationData();
                
                EditorGUILayout.EndVertical();
            }

            if (GUI.changed) {
                EditorUtility.SetDirty(target);
                settings.WriteSettingsToFile();
            }
        }

        private void RenderBanners() {
            var settings = Settings.Instance;
            var banners = settings.NotificationBanners;
            const int maxBanners = 4;
            
            if (banners.Count == 0) {
                banners.Add(null);
            }
            // Remove banners until 4
            while (banners.Count > maxBanners) {
                banners.RemoveAt(banners.Count - 1);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Banner", GUILayout.Width(100))) {
                if (banners.Count < maxBanners) {
                    banners.Add(null);
                }
            }
            if (GUILayout.Button("Remove Banner", GUILayout.Width(100))) {
                if (banners.Count > 0) {
                    banners.RemoveAt(banners.Count - 1);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical();
            for (var i = 0; i < banners.Count; i++) {
                EditorGUILayout.BeginVertical(GUILayout.Width(80));
                EditorGUILayout.LabelField($"Banner {i}");

                banners[i] = (Texture2D)EditorGUILayout.ObjectField(banners[i],
                    typeof(Texture2D), false, GUILayout.Width(70 * 4), GUILayout.Height(70));

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }

        private void RenderNotificationData() {
            var settings = Settings.Instance;
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.BeginVertical();
            EditorGUILayout.PrefixLabel("Layout XML for Phone");
            settings.notificationData.layoutPhoneXml = (TextAsset)EditorGUILayout.ObjectField(
                settings.notificationData.layoutPhoneXml, typeof(TextAsset), false);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical();
            EditorGUILayout.PrefixLabel("Layout XML for Tablet");
            settings.notificationData.layoutTabletXml = (TextAsset)EditorGUILayout.ObjectField(
                settings.notificationData.layoutTabletXml, typeof(TextAsset), false);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndVertical();
        }
    }
}