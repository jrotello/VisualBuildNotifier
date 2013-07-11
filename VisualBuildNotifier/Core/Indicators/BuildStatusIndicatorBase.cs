using System;
using Microsoft.TeamFoundation.Build.Client;
using VisualBuildNotifier.Services;

namespace VisualBuildNotifier.Core.Indicators {
    public abstract class BuildStatusIndicatorBase : IBuildStatusIndicator {
        public abstract void ReportSuccess(IBuildDetail build);
        public abstract void ReportFailure(IBuildDetail build);
        public abstract void ReportInProgress(IBuildDetail build);

        protected IBuildDetail LastBuild { get; set; }

        protected virtual bool ShouldDisplayNotification(IBuildDetail build) {
            return (
                LastBuild == null
                || build.BuildNumber != LastBuild.BuildNumber)
                || (build.Status != BuildStatus.InProgress && LastBuild.Status == BuildStatus.InProgress
            );
        }
    }
}