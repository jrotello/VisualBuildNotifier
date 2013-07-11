using System;
using Microsoft.TeamFoundation.Build.Client;
using Plenom.Components.Busylight.Sdk;

namespace VisualBuildNotifier.Core.Indicators
{
    public class BusylightBuildStatusIndicator: BuildStatusIndicatorBase, IDisposable {
        private readonly BusylightController _busylight;

        public BusylightBuildStatusIndicator() {
            _busylight = new BusylightLyncController();
            _busylight.Light(BusylightColor.Off);
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

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;

        private void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    _busylight.Light(BusylightColor.Off);
                }
            }

            _disposed = true;
        }

        ~BusylightBuildStatusIndicator() {
            Dispose(false);
        }
    }
}
