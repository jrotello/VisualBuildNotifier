using System;
using Microsoft.TeamFoundation.Build.Client;
using VisualBuildNotifier.Services;

namespace BuildNotifier.Services {
    public class ConsoleBuildStatusIndicator: BuildStatusIndicatorBase {        
        private IBuildDetail _lastBuild;

        public override void ReportSuccess(IBuildDetail build) {
            ReportStatus(
                build,
                "Build Succeeded"
            );
        }

        public override void ReportFailure(IBuildDetail build) {
            ReportStatus(
                build,
                "Build Failure"
            );
        }

        public override void ReportInProgress(IBuildDetail build) {
            ReportStatus(
                build,
                "Build In Progress"
            );
        }

        private void ReportStatus(IBuildDetail build, string text)
        {
            if (ShouldDisplayNotification(build))
            {
                Console.WriteLine(
                    "{0} [{1}]: {2}",
                    build.BuildNumber,
                    build.RequestedBy,
                    text
                );
            }

            _lastBuild = build;
        }

        protected override bool ShouldDisplayNotification(IBuildDetail build) {
            return true;
        }
    }
}