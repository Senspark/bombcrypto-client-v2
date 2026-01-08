using System.IO;

using UnityEngine;

namespace Senspark.Utility {
    public static class FileUtils {
        public static void CopyFolder(
            string sourceFolderPath,
            string destinationFolderPath,
            bool recursive
        ) {
            var sourceFolder = new DirectoryInfo(sourceFolderPath);

            if (!sourceFolder.Exists) {
                throw new DirectoryNotFoundException(
                    $"Source folder does not exist or could not be found: {sourceFolderPath}");
            }

            Directory.CreateDirectory(destinationFolderPath);

            // Copy files
            var files = sourceFolder.GetFiles();
            foreach (var file in files) {
                var destinationFilePath = Path.Combine(destinationFolderPath, file.Name);
                file.CopyTo(destinationFilePath, false);
            }

            if (recursive) {
                // Recursively copy subfolders
                var subfolders = sourceFolder.GetDirectories();
                foreach (var subfolder in subfolders) {
                    var destinationSubfolderPath = Path.Combine(destinationFolderPath, subfolder.Name);
                    CopyFolder(subfolder.FullName, destinationSubfolderPath, true);
                }    
            }
        }

        public static void ChangeFilesExt(
            string folderPath,
            string searchExt,
            string newExt
        ) {
            var pattern = $"*.{searchExt}";

            // Get all files with the specified extension in the folder
            var filePaths = Directory.GetFiles(folderPath, pattern);

            // Rename each file
            foreach (var filePath in filePaths) {
                var newFilePath = Path.ChangeExtension(filePath, newExt);
                File.Move(filePath, newFilePath);
            }
        }

        /// <summary>
        /// Trả về danh sách files tìm thấy
        /// </summary>
        /// <returns></returns>
        public static FileInfo[] FindFilesWithExt(string folderPath, string searchExt) {
            var pattern = $"*.{searchExt}";
            var filePaths = Directory.GetFiles(folderPath, pattern, SearchOption.AllDirectories);
            var fileNames = new FileInfo[filePaths.Length];
            for (var i = 0; i < filePaths.Length; i++) {
                fileNames[i] = new FileInfo(filePaths[i]);
                Debug.Log($"Has file: {fileNames[i]}");
            }
            return fileNames;
        }

        public static bool DeleteFiles(string folderPath, string[] fileNames) {
            var deleted = false;
            foreach (var it in fileNames) {
                var path = Path.Combine(folderPath, it);
                if (!File.Exists(path)) {
                    continue;
                }
                File.Delete(path);
                deleted = true;
                Debug.Log($"Delete file: {path}");
            }
            return deleted;
        }
    }
}