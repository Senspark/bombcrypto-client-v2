using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;

#if UNITY_IOS
using UnityEditor.iOS;
#endif

using UnityEngine;

namespace Senspark.Editor {
    public class Settings : ScriptableObject {
        private const string LibrarySettingsDir = "Assets/Senspark";
        private const string LibrarySettingsFile = "Assets/Senspark/Settings.asset";

        private static Settings _sharedInstance;

        public static Settings Instance {
            get {
                if (_sharedInstance == null) {
                    if (!AssetDatabase.IsValidFolder(LibrarySettingsDir)) {
                        AssetDatabase.CreateFolder("Assets", "Senspark");
                    }
                    _sharedInstance = AssetDatabase.LoadAssetAtPath<Settings>(LibrarySettingsFile);
                    if (_sharedInstance == null) {
                        _sharedInstance = CreateInstance<Settings>();
                        AssetDatabase.CreateAsset(_sharedInstance, LibrarySettingsFile);
                    }
                }
                return _sharedInstance;
            }
        }
        
        [SerializeField] 
        private Texture2D _notificationIcon;

        [SerializeField] 
        private List<Texture2D> _notificationBanners = new();
        
        public NotificationData notificationData;
        
        public Texture2D NotificationIcon {
            get => _notificationIcon;
            set => _notificationIcon = value;
        }
        
        public List<Texture2D> NotificationBanners {
            get => _notificationBanners;
            set => _notificationBanners = value;
        }
        
        public void WriteSettingsToFile() {
            AssetDatabase.SaveAssets();
        }

        public void UpdatePlayerSettings() {
            // Common settings.
            PlayerSettings.companyName = "Senspark";
            // Icon.
            var backgroundIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Packages/com.senspark.extension/Images/Icon/icon_background.png");
            void UpdateIcon(BuildTargetGroup platform, PlatformIconKind kind, params Texture2D[] textures) {
                var dirty = false;
                var icons = PlayerSettings.GetPlatformIcons(platform, kind);
                foreach (var icon in icons) {
                    var currentTextures = icon.GetTextures();
                    if (currentTextures.Length == textures.Length &&
                        Enumerable.Range(0, currentTextures.Length).All(index =>
                            currentTextures[index] != null &&
                            currentTextures[index].GetInstanceID() == textures[index].GetInstanceID())) {
                        continue;
                    }
                    icon.SetTextures(textures);
                    dirty = true;
                }
                if (dirty) {
                    PlayerSettings.SetPlatformIcons(platform, kind, icons);
                }
            }
            
            // Splash image.
            PlayerSettings.SplashScreen.unityLogoStyle = PlayerSettings.SplashScreen.UnityLogoStyle.LightOnDark;
            PlayerSettings.SplashScreen.animationMode = PlayerSettings.SplashScreen.AnimationMode.Dolly;
            PlayerSettings.SplashScreen.drawMode = PlayerSettings.SplashScreen.DrawMode.UnityLogoBelow;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            var companyLogo =
                AssetDatabase.LoadAssetAtPath<Sprite>("Packages/com.senspark.extension/Images/Icon/company_logo.png");
            PlayerSettings.SplashScreen.logos = new[] { // 
                PlayerSettings.SplashScreenLogo.Create(2f, companyLogo),
            };
            PlayerSettings.SplashScreen.overlayOpacity = 1;
            PlayerSettings.SplashScreen.backgroundColor = Color.black;
            PlayerSettings.SplashScreen.blurBackgroundImage = true;
            PlayerSettings.SplashScreen.background = null;
            // Other settings.
            PlayerSettings.MTRendering = true;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions) 33;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.gcIncremental = true;
            PlayerSettings.assemblyVersionValidation = true;
            PlayerSettings.muteOtherAudioSources = false;
            
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
            PlayerSettings.Android.buildApkPerCpuArchitecture = false;
            PlayerSettings.iOS.targetOSVersionString = "12.0";
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.Low);
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.iOS, ManagedStrippingLevel.Low);
            // Publishing settings.
            PlayerSettings.Android.minifyWithR8 = false;
            PlayerSettings.Android.minifyRelease = false;
            PlayerSettings.Android.minifyDebug = false;
            AssetDatabase.SaveAssets();
        }
    }

    [Serializable]
    public class NotificationData {
        public TextAsset layoutPhoneXml;
        public TextAsset layoutTabletXml;
    }
}