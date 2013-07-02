using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.Client;
using NLog;

namespace VisualBuildNotifier.Services
{
    public class LoggingBuildStatusIndicator: BuildStatusIndicatorBase {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public override void ReportSuccess(IBuildDetail build) {
            _logger.Trace("Build Succeeded: {0} by {1}", build.BuildNumber, build.RequestedFor);
        }

        public override void ReportFailure(IBuildDetail build) {
            _logger.Trace("Build Failed: {0} by {1}", build.BuildNumber, build.RequestedFor);
        }

        public override void ReportInProgress(IBuildDetail build) {
            _logger.Trace("Build In Progress: {0} by {1}", build.BuildNumber, build.RequestedFor);
        }
    }
}
