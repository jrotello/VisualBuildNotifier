using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.Client;
using Plenom.Components.Busylight.Sdk;

namespace VisualBuildNotifier.Services
{
    public class BusylightBuildStatusIndicator: BuildStatusIndicatorBase {
        private readonly BusylightController _busylight;

        public BusylightBuildStatusIndicator(BusylightController busylight) {
            if (busylight == null) {
                throw new ArgumentNullException("busylight");
            }

            _busylight = busylight;
        }

        public override void ReportSuccess(IBuildDetail build) {
            _busylight.Light(BusylightColor.Green);
        }

        public override void ReportFailure(IBuildDetail build) {
            _busylight.Light(BusylightColor.Red);
        }

        public override void ReportInProgress(IBuildDetail build) {
            var sequence = new PulseSequence {
                Color = BusylightColor.Blue,
                Step1 = 0,
                Step2 = 255,
                Step3 = 0,
                Step4 = 255
            };
            _busylight.Pulse(sequence);
        }
    }
}
