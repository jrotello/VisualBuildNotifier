using VisualBuildNotifier.Models;

namespace VisualBuildNotifier.Core
{
    public interface IConfigManager {
        Config LoadConfiguration();
        void SaveConfiguration(Config config);
    }
}
