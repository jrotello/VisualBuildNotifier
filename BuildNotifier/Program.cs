using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using BuildNotifier.Services;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using VisualBuildNotifier.Services;

namespace BuildNotifier {
    internal class Program {
        private static void Main(string[] args) {


            _tfsService = new TfsService(TfsUri);
            using (var timer = new Timer()) {
                timer.Interval = 10000;
                timer.Elapsed += QueryBuildStatus;
                timer.Enabled = true;

                Console.ReadLine();
                timer.Enabled = false;
            }
        }

        private const string TfsProjectName = "Tracker";
        private const string TfsBuildName = "11.0";

        private static readonly Uri TfsUri = new Uri("http://wa-devmain-dev.tlr.thomson.com:8080/tfs");
        private static ITfsService _tfsService;

        private static readonly List<IBuildStatusIndicator> StatusIndicators = new List<IBuildStatusIndicator>() {
            new ConsoleBuildStatusIndicator()
        };

        private static void QueryBuildStatus(object sender, EventArgs e) {
            IBuildDetail build = _tfsService.GetLatestBuildInfo(TfsProjectName, TfsBuildName);
            if (build != null) {
                ReportBuildStatus(build);
            }
        }

        private static void ReportBuildStatus(IBuildDetail build) {
            foreach (var statusIndicator in StatusIndicators) {
                switch (build.Status) {
                    case BuildStatus.Succeeded:
                        statusIndicator.ReportSuccess(build);
                        break;
                    case BuildStatus.InProgress:
                        statusIndicator.ReportInProgress(build);
                        break;
                    default:
                        statusIndicator.ReportFailure(build);
                        break;
                }
            }
        }
    }
}