using System;
using System.Configuration;
using VisualBuildNotifier.Models;

namespace VisualBuildNotifier.Core {
    public class ConfigManager: IConfigManager {
        public Config LoadConfiguration() {
            var config = new Config {
                Server = ConfigurationManager.AppSettings["TfsServer"],
                Project = ConfigurationManager.AppSettings["TfsProject"],
                Build = ConfigurationManager.AppSettings["TfsBuildDefinition"]
            };

            bool blink1Enabled;
            Boolean.TryParse(ConfigurationManager.AppSettings["Blink1Enabled"], out blink1Enabled);
            config.Blink1Enabled = blink1Enabled;

            return config;
        }

        public void SaveConfiguration(Config config) {
            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var appSettings = configuration.AppSettings.Settings;
            appSettings["TfsServer"].Value = config.Server;
            appSettings["TfsProject"].Value = config.Project;
            appSettings["TfsBuildDefinition"].Value = config.Build;
            appSettings["Blink1Enabled"].Value = config.Blink1Enabled.ToString();

            configuration.Save(ConfigurationSaveMode.Modified);
        }
    }
}