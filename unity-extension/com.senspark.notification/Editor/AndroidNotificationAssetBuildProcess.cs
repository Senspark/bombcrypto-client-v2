#if UNITY_ANDROID && UNITY_EDITOR

using System.IO;
using System.Linq;

using Senspark.Editor;
using Senspark.Utility;

using UnityEditor;
using UnityEditor.Android;

using UnityEngine;

namespace Senspark.Notification.Editor {
    public class AndroidNotificationAssetBuildProcess : IPostGenerateGradleAndroidProject {
        public int callbackOrder => 0;

        public void OnPostGenerateGradleAndroidProject(string path) {
            // Sửa file build.gradle với nội dung *.jar thành *.aar
            // vd: implementation fileTree(dir: 'bin', include: ['*.jar'])

            // \Library\Bee\Android\Prj\IL2CPP\Gradle\unityLibrary\CustomNotification.androidlib\build.gradle

            var banners = Settings.Instance.NotificationBanners; 
            if (banners.Count == 0) {
                return;
            }

            {
                // Copy file NotificationBanner vào thư mục drawable
                // \Library\Bee\Android\Prj\IL2CPP\Gradle\unityLibrary\CustomNotification.androidlib\res\drawable
                var drawablePath = Path.Combine(path, Configs.AndroidLibName, "res", "drawable");
                
                if (Directory.Exists(drawablePath)) {
                    Directory.Delete(drawablePath, true);
                }
                Directory.CreateDirectory(drawablePath);

                for (var i = 0; i < banners.Count; i++) {
                    var b = banners[i];
                    if (b) {
                        // \Library\Bee\Android\Prj\IL2CPP\Gradle\unityLibrary\CustomNotification.androidlib\res\drawable\notification_background_{0}.png
                        var targetBannerPath =
                            Path.Combine(drawablePath, string.Format(Configs.BannerFileNameFormat, i));
                        var assetPath = AssetDatabase.GetAssetPath(b);
                        File.Copy(assetPath, targetBannerPath);
                    }
                }
            }

            {
                if (!Settings.Instance.NotificationIcon) {
                    return;
                }

                // Sửa các files Icon:
                // Bổ sung thư mục drawable-xxhdpi drawable-xxxhdpi nếu thiếu
                // \Library\Bee\Android\Prj\IL2CPP\Gradle\unityLibrary\src\main\res
                var resPath = Path.Combine(path, "src", "main", "res");
                ResizeImgAndSave(Settings.Instance.NotificationIcon, resPath, Configs.IconFileName);

                const string xHdpi = "drawable-xhdpi";
                var missing = new[] {
                    "drawable-xxhdpi",
                    "drawable-xxxhdpi"
                };

                var directories = Directory.GetDirectories(resPath);
                var src = directories.FirstOrDefault(e => e.Contains(xHdpi));
                if (string.IsNullOrWhiteSpace(src)) {
                    return;
                }

                foreach (var it in missing) {
                    var existed = directories.Any(e => e.Contains(it));
                    if (existed) {
                        continue;
                    }
                    var target = src.Replace(xHdpi, it);
                    FileUtils.CopyFolder(src, target, true);
                }
            }

            {
                // Overide file layout
                var notificationData = Settings.Instance.notificationData;
                if (notificationData == null || !notificationData.layoutPhoneXml || !notificationData.layoutTabletXml) {
                    return;
                }
                var resPath = Path.Combine(path, "src", "main", "res");
                var layoutPathPhone = Path.Combine(resPath, "layout");
                var layoutPathTablet = Path.Combine(resPath, "layout-sw600dp");
                if (!Directory.Exists(layoutPathPhone)) {
                    Directory.CreateDirectory(layoutPathPhone);
                }
                if (!Directory.Exists(layoutPathTablet)) {
                    Directory.CreateDirectory(layoutPathTablet);
                }
                File.Copy(AssetDatabase.GetAssetPath(notificationData.layoutPhoneXml),
                    Path.Combine(layoutPathPhone, "custom_notification_layout.xml"), true);
                File.Copy(AssetDatabase.GetAssetPath(notificationData.layoutTabletXml),
                    Path.Combine(layoutPathTablet, "custom_notification_layout.xml"), true);
            }
        }

        private void ResizeImgAndSave(Texture2D txt, string path, string fileName) {
            var sizes = new[] {
                (18, "drawable-ldpi"),
                (24, "drawable-mdpi"),
                (36, "drawable-hdpi"),
                (48, "drawable-xhdpi"),
                (72, "drawable-xxhdpi"),
                (96, "drawable-xxxhdpi")
            };
            foreach (var (s, n) in sizes) {
                var t = Resize(txt, s, s);
                var f = Path.Combine(path, n);
                var p = Path.Combine(f, fileName);
                if (Directory.Exists(f)) {
                    Directory.Delete(f, true);
                }
                Directory.CreateDirectory(f);
                SaveTextureAsPNG(t, p);
            }
        }

        private static Texture2D Resize(Texture2D texture2D, int targetX, int targetY) {
            var rt = new RenderTexture(targetX, targetY, 24);
            RenderTexture.active = rt;
            Graphics.Blit(texture2D, rt);
            var result = new Texture2D(targetX, targetY);
            result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
            result.Apply();
            return result;
        }

        private static void SaveTextureAsPNG(Texture2D texture, string fullPath) {
            var bytes = texture.EncodeToPNG();
            File.WriteAllBytes(fullPath, bytes);
        }
    }
}
#endif