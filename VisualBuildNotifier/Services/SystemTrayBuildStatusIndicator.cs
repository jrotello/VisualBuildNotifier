using System.Text;
using System.Windows.Forms;
using Microsoft.TeamFoundation.Build.Client;

namespace VisualBuildNotifier.Services {
    public class SystemTrayBuildStatusIndicator: BuildStatusIndicatorBase {        
        private const int BalloonTimeout = 1500;

        private readonly NotifyIcon _notifyIcon;
        

        public SystemTrayBuildStatusIndicator(NotifyIcon notifyIcon) {
            _notifyIcon = notifyIcon;
        }

        public override void ReportSuccess(IBuildDetail build) {
            ReportStatus(
                build,
                "Build Succeeded",
                ToolTipIcon.Info
            );
        }

        public override void ReportFailure(IBuildDetail build) {
            ReportStatus(
                build,
                "Build Failed",
                ToolTipIcon.Error
            );
        }

        public override void ReportInProgress(IBuildDetail build) {
            ReportStatus(
                build,
                "Build In Progress",
                ToolTipIcon.Info
            );
        }

        private void ReportStatus(IBuildDetail build, string text, ToolTipIcon icon = ToolTipIcon.None)
        {
            if (ShouldDisplayNotification(build)) {
                StringBuilder message = new StringBuilder(); 
                message.AppendFormat("Build Number: {0}\n", build.BuildNumber);
                message.AppendFormat("User: {0}\n", build.RequestedFor);
                
                _notifyIcon.ShowBalloonTip(
                    BalloonTimeout,
                    text,
                    message.ToString(),
                    icon
                );
            }

            LastBuild = build;
        }

    }
}