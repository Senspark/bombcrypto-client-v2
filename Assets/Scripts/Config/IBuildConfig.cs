using System;

namespace App {
    public interface IBuildConfig {
        bool IsProduction { get; }
        int Version { get; }
        string Salt { get; }
    }

    public class DefaultBuildConfig : IBuildConfig {
        public bool IsProduction { get; }
        public int Version { get; }
        public string Salt { get; }

        public DefaultBuildConfig(bool isProduction) {
            IsProduction = isProduction;
            Version = AppConfig.GetVersion(isProduction);
            Salt = AppConfig.GetSaltKey(isProduction);
        }
    }
}