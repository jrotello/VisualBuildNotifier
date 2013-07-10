using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.Client;
using NLog;
using ThingM.Blink1;
using ThingM.Blink1.ColorProcessor;

namespace VisualBuildNotifier.Core.Indicators
{
    public class Blink1BuildStatusIndicator: BuildStatusIndicatorBase {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public override void ReportSuccess(IBuildDetail build) {
            ReportStatus(build, HtmlColorName.Green);
        }

        public override void ReportFailure(IBuildDetail build) {
            ReportStatus(build, HtmlColorName.Red);
        }

        public override void ReportInProgress(IBuildDetail build) {
            ReportStatus(build, HtmlColorName.Yellow);
        }

        private void ReportStatus(IBuildDetail build, string color) {
            if (!ShouldDisplayNotification(build)) {
                return;
            }

            using (var blink1 = new Blink1()) {
                try {
                    blink1.Open();
                    blink1.SetColor(0, 0, 0);
                    blink1.FadeToColor(500, new HtmlHexadecimal(color), true);
                } catch (InvalidOperationException exception) {
                    _logger.ErrorException(exception.Message, exception);
                } finally {
                    blink1.Complete();
                }
            }
        }
    }
}
