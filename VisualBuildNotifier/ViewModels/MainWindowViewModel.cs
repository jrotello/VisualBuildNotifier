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
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using NLog;
using VisualBuildNotifier.Services;
using System.Timers;
using Configuration = VisualBuildNotifier.Models.Config;
using Timer = System.Timers.Timer;

namespace VisualBuildNotifier.ViewModels
{
    public class MainWindowViewModel: INotifyPropertyChanged {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly Configuration _configuration;
        private readonly ObservableCollection<string> _buildDefinitionNames = new ObservableCollection<string>();
        private readonly Timer _timer;

        public MainWindowViewModel(Configuration configuration = null, ITfsService tfsService = null) {
            StatusIndicators = new List<IBuildStatusIndicator>();
            _configuration = configuration ?? new Configuration();
            _tfsService = tfsService;

            _timer = new Timer(10000) {Enabled = false};
            _timer.Elapsed += QueryBuildStatus;
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
            get { return String.IsNullOrEmpty(_selectedServerUri) ? _configuration.Server: _selectedServerUri; }
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
            get { return String.IsNullOrEmpty(_selectedProjectName) ? _configuration.Project : _selectedProjectName; }
            set
            {
                if (_selectedProjectName != value)
                {
                    _selectedProjectName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _selectedBuildDefinitionName;
        public string SelectedBuildDefinitionName {
            get {
                return String.IsNullOrEmpty(_selectedBuildDefinitionName)
                           ? _configuration.Build
                           : _selectedBuildDefinitionName;
            }
            set {
                if (_selectedBuildDefinitionName != value) {
                    _selectedBuildDefinitionName = value;
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
                SelectedBuildDefinitionName = BuildDefinitionNames.FirstOrDefault() ?? String.Empty;
            }            
        }

        private void RefreshBuildDefinitions() {
            BuildDefinitionNames.Clear();            
            TfsService.GetBuildDefinitions(SelectedProjectName)
                .ToList()
                .ForEach(def => BuildDefinitionNames.Add(def.Name));
        }

        public void LoadConfiguration() {
            _configuration.Server = ConfigurationManager.AppSettings["TfsServer"];
            _configuration.Project = ConfigurationManager.AppSettings["TfsProject"];
            _configuration.Build = ConfigurationManager.AppSettings["TfsBuildDefinition"];

            RefreshBuildDefinitions();
            SelectedBuildDefinitionName = SelectedBuildDefinitionName;

            StartTracking();
        }

        public void SaveConfiguration() {
            _configuration.Server = SelectedServerUri;
            _configuration.Project = SelectedProjectName;
            _configuration.Build = SelectedBuildDefinitionName;

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["TfsServer"].Value = SelectedServerUri;
            config.AppSettings.Settings["TfsProject"].Value = SelectedProjectName;
            config.AppSettings.Settings["TfsBuildDefinition"].Value = SelectedBuildDefinitionName;

            config.Save(ConfigurationSaveMode.Modified);

            StartTracking();
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
