using Microsoft.TeamFoundation.Build.Client;

namespace VisualBuildNotifier.Core.Indicators {
    public interface IBuildStatusIndicator {
        void ReportSuccess(IBuildDetail build);
        void ReportFailure(IBuildDetail build);
        void ReportInProgress(IBuildDetail build);
    }
}