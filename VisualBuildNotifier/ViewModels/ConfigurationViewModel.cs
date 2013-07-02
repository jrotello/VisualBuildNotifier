using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using NLog;
using VisualBuildNotifier.Models;
using VisualBuildNotifier.Services;

namespace VisualBuildNotifier.ViewModels
{
    public class ConfigurationViewModel: INotifyPropertyChanged {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly Configuration _configuration;
        private readonly ObservableCollection<string> _buildDefinitionNames = new ObservableCollection<string>();
        private readonly DispatcherTimer _timer;

        public ConfigurationViewModel(Configuration configuration = null, ITfsService tfsService = null) {
            StatusIndicators = new List<IBuildStatusIndicator>();
            _configuration = configuration ?? new Configuration();
            _tfsService = tfsService;

            _timer = new DispatcherTimer {
                Interval = TimeSpan.FromSeconds(10),
                IsEnabled = tfsService != null
            };
            _timer.Tick += QueryBuildStatus;
        }

        private void QueryBuildStatus(object sender, EventArgs e) {

            if (TfsService == null) {
                throw new ArgumentNullException();
            }

            IBuildDetail build = TfsService.GetLatestBuildInfo(_configuration.Project, _configuration.Build);
            if (build != null) {
                ReportBuildStatus(build);
            }
        }

        private void ReportBuildStatus(IBuildDetail build) {
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

        private ITfsService _tfsService; 
        private ITfsService TfsService {
            get {
                if (_tfsService == null) {
                    if (!String.IsNullOrEmpty(_configuration.Server)) {
                        _tfsService = new TfsService(new Uri(_configuration.Server));
                    }
                }

                return _tfsService;
            }
        }

        public List<IBuildStatusIndicator> StatusIndicators { get; set; }

        public void StartTracking() {
            _timer.Start();
        }

        public void StopTracking() {
            _timer.Stop();
        }

        public string Project {
            get { return _configuration.Project; }
            set {
                if (_configuration.Project != value) {
                    _configuration.Project = value;
                    OnPropertyChanged();
                }                
            }
        }

        public string Server {
            get { return _configuration.Server; }
            set {
                if (_configuration.Server != value) {
                    _configuration.Server = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedBuildDefinitionName {
            get { return _configuration.Build; }
            set {
                if (_configuration.Build != value) {
                    _logger.Trace("Build definition selected: {0}", value);
                    _configuration.Build = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<string> BuildDefinitionNames {
            get { return _buildDefinitionNames; }
        }

        public void SelectServer() {
            var picker = new TeamProjectPicker(TeamProjectPickerMode.SingleProject, false, new UICredentialsProvider());
            if (picker.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                Server = picker.SelectedTeamProjectCollection.Uri.ToString();
                _tfsService = new TfsService(new Uri(_configuration.Server));
                Server = _configuration.Server;
                Project = picker.SelectedProjects[0].Name;

                RefreshBuildDefinitions();
                SelectedBuildDefinitionName = BuildDefinitionNames.FirstOrDefault() ?? String.Empty;

                _timer.IsEnabled = true;
            }            
        }

        private void RefreshBuildDefinitions() {
            BuildDefinitionNames.Clear();            
            TfsService.GetBuildDefinitions(Project)
                .ToList()
                .ForEach(def => BuildDefinitionNames.Add(def.Name));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
