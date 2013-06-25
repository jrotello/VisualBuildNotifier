using Microsoft.TeamFoundation.Build.Client;

namespace VisualBuildNotifier.Services {
    public interface IBuildStatusIndicator {
        void ReportSuccess(IBuildDetail build);
        void ReportFailure(IBuildDetail build);
        void ReportInProgress(IBuildDetail build);
    }
}