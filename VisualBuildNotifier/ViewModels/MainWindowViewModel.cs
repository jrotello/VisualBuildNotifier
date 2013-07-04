using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using NLog;
using VisualBuildNotifier.Models;
using VisualBuildNotifier.Services;
using System.Timers;
using Timer = System.Timers.Timer;

namespace VisualBuildNotifier.ViewModels
{
    public class MainWindowViewModel: INotifyPropertyChanged {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private Config _config;
        private readonly IConfigManager _configManager;

        private readonly ObservableCollection<string> _buildDefinitionNames = new ObservableCollection<string>();
        private readonly Timer _timer;

        public MainWindowViewModel(IConfigManager configManager, ITfsService tfsService = null) {
            StatusIndicators = new List<IBuildStatusIndicator>();
            _configManager = configManager;
            _tfsService = tfsService;

            _timer = new Timer(10000) {Enabled = false};
            _timer.Elapsed += QueryBuildStatus;
        }

        private void QueryBuildStatus(object sender, EventArgs e) {
            InvokeTfsOperation(() => {
                IBuildDetail build = TfsService.GetLatestBuildInfo(_config.Project, _config.Build);
                if (build != null) {
                    ReportBuildStatus(build);
                    StatusText = String.Format("{0}: {1} for {2}", build.Status.ToString(), build.BuildNumber, build.RequestedFor);
                }
            });
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
                    if (!String.IsNullOrEmpty(_config.Server)) {
                        _tfsService = new TfsService(new Uri(_config.Server));                        
                    }
                }

                return _tfsService;
            }
            set { _tfsService = value; }
        }

        public List<IBuildStatusIndicator> StatusIndicators { get; set; }

        public void StartTracking() {
            _timer.Start();
        }

        public void StopTracking() {
            _timer.Stop();
        }

        private string _selectedServerUri;
        public string SelectedServerUri {
            get { return _selectedServerUri; }
            set {
                if (_selectedServerUri != value) {
                    _selectedServerUri = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _selectedProjectName;
        public string SelectedProjectName
        {
            get { return _selectedProjectName; }
            set {
                if (_selectedProjectName != value) {
                    _selectedProjectName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _selectedBuildDefinitionName;       
        public string SelectedBuildDefinitionName {
            get {
                return _selectedBuildDefinitionName;
            }
            set {
                if (_selectedBuildDefinitionName != value) {
                    _selectedBuildDefinitionName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _statusText;
        public string StatusText {
            get { return _statusText; }
            set {
                if (_statusText != value) {
                    _statusText = value;
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
                SelectedServerUri = picker.SelectedTeamProjectCollection.Uri.ToString();
                SelectedProjectName = picker.SelectedProjects[0].Name;

                TfsService = new TfsService(new Uri(SelectedServerUri));

                RefreshBuildDefinitions();                
            }            
        }

        private void InvokeTfsOperation(Action action) {
            if (action == null) {
                throw new ArgumentNullException("action");
            }

            if (TfsService == null) {
                const string msg = "Unable to connect to TFS with an incomplete configuration.";
                _logger.Warn(msg);
                StatusText = msg;
                return;
            }

            try {
                action();
            } catch (TeamFoundationServerException ex) {
                _logger.ErrorException(ex.Message, ex);
                StatusText = ex.Message;
            }
        }

        public void RefreshBuildDefinitions() {
            BuildDefinitionNames.Clear();
            InvokeTfsOperation(() => {
                TfsService.GetBuildDefinitions(SelectedProjectName)
                          .ToList()
                          .ForEach(def => BuildDefinitionNames.Add(def.Name));

                if (string.IsNullOrEmpty(_config.Build)) {
                    SelectedBuildDefinitionName = BuildDefinitionNames.FirstOrDefault() ?? String.Empty;
                } else {
                    SelectedBuildDefinitionName = _config.Build;
                }                
            });
        }

        public void LoadConfiguration() {
            _config = _configManager.LoadConfiguration();
            SelectedServerUri = _config.Server;
            SelectedProjectName = _config.Project;

            if (_config.IsComplete) {
                RefreshBuildDefinitions();
                StartTracking();
            }
        }

        public void SaveConfiguration() {
            _config.Server = SelectedServerUri;
            _config.Project = SelectedProjectName;
            _config.Build = SelectedBuildDefinitionName;
            
            _configManager.SaveConfiguration(_config);
            StartTracking();
        }

        public void DiscardPendingConfiguration() {
            SelectedServerUri = _config.Server;
            SelectedProjectName = _config.Project;
            SelectedBuildDefinitionName = _config.Build;
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
