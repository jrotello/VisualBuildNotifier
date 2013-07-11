using System.Configuration;
using VisualBuildNotifier.Models;
using VisualBuildNotifier.Services;

namespace VisualBuildNotifier.Core {
    public class ConfigManager: IConfigManager {
        public Config LoadConfiguration() {
            var config = new Config {
                Server = ConfigurationManager.AppSettings["TfsServer"],
                Project = ConfigurationManager.AppSettings["TfsProject"],
                Build = ConfigurationManager.AppSettings["TfsBuildDefinition"]
            };

            return config;
        }

        public void SaveConfiguration(Config config) {
            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings["TfsServer"].Value = config.Server;
            configuration.AppSettings.Settings["TfsProject"].Value = config.Project;
            configuration.AppSettings.Settings["TfsBuildDefinition"].Value = config.Build;

            configuration.Save(ConfigurationSaveMode.Modified);
        }
    }
}