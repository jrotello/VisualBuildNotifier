using System;
using Microsoft.TeamFoundation.Build.Client;
using VisualBuildNotifier.Services;

namespace BuildNotifier.Services {
    public class ConsoleBuildStatusIndicator: IBuildStatusIndicator {        
        private IBuildDetail _lastBuild;

        public void ReportSuccess(IBuildDetail build) {
            ReportStatus(
                build,
                "Build Succeeded"
            );
        }

        public void ReportFailure(IBuildDetail build) {
            ReportStatus(
                build,
                "Build Failure"
            );
        }

        public void ReportInProgress(IBuildDetail build) {
            ReportStatus(
                build,
                "Build In Progress"
            );
        }

        private void ReportStatus(IBuildDetail build, string text)
        {
            if (ShouldShowBalloonTip(build))
            {
                Console.WriteLine(
                    "{0} [{1}]: {2}",
                    build.BuildNumber,
                    build.RequestedFor,
                    text
                );
            }

            _lastBuild = build;
        }

        private bool ShouldShowBalloonTip(IBuildDetail build) {
            return (
                _lastBuild == null
                || build.BuildNumber != _lastBuild.BuildNumber)
                || (build.Status != BuildStatus.InProgress && _lastBuild.Status == BuildStatus.InProgress);
        }
    }
}