using System.Collections.Generic;

using UnityEngine;

namespace Utils {
    public static class WriteEnumUtils {
        public static string GetScriptsPath() {
            return System.IO.Path.Combine(Application.dataPath, "Scripts");
        }

        public static void WriteEnum<T>(string filePath, List<string> values) {
            var strBuilder = new System.Text.StringBuilder();
            strBuilder.AppendLine("namespace " + typeof(T).Namespace + " {");
            strBuilder.AppendLine("    public enum " + typeof(T).Name + " {");
            foreach (var value in values) {
                strBuilder.AppendLine("        " + value + ",");
            }
            strBuilder.AppendLine("    }");
            strBuilder.AppendLine("}");
            System.IO.File.WriteAllText(filePath, strBuilder.ToString());
            Debug.Log($"Enum generated and written to {filePath}");
        }
    }
}