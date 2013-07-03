using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualBuildNotifier.Models;

namespace VisualBuildNotifier.Services
{
    public interface IConfigManager {
        Config LoadConfiguration();
        void SaveConfiguration(Config config);
    }
}
