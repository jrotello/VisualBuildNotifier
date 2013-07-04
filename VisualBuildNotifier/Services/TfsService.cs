using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Threading;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using NLog;

namespace VisualBuildNotifier.Services {
    public class TfsService: ITfsService {
        private Logger _logger = LogManager.GetCurrentClassLogger();

        public TfsService(Uri serverUri) {
            if (serverUri == null) {
                throw new ArgumentNullException("serverUri");
            }

            ServerUri = serverUri;
        }

        private Uri ServerUri { get; set; }

        private TfsTeamProjectCollection _server;
        private TfsTeamProjectCollection Server {
            get {
                if (_server == null) {
                    _server = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(ServerUri);
                    _server.EnsureAuthenticated();
                }

                return _server;
            }
        }

        public IBuildDetail GetLatestBuildInfo(string projectName, string buildDefinitionName) {
            return GetBuildInfo(projectName, buildDefinitionName, maxBuilds: 1).SingleOrDefault();
        }

        private IEnumerable<IBuildDetail> GetBuildInfo(string projectName, string buildDefinitionName, int maxBuilds) {
            var buildServer = Server.GetService<IBuildServer>();
            
            var spec = buildServer.CreateBuildDetailSpec(projectName, buildDefinitionName);
            spec.QueryOrder = BuildQueryOrder.StartTimeDescending;
            spec.Status = BuildStatus.InProgress | BuildStatus.Succeeded | BuildStatus.PartiallySucceeded | BuildStatus.Failed;
            spec.MaxBuildsPerDefinition = maxBuilds;
            spec.InformationTypes = new string[0]; // Don't return extra information. We are only interested in the basic build info.
            
            var result = buildServer.QueryBuilds(spec);
            return result.Builds;
        }

        public IEnumerable<IBuildDefinition> GetBuildDefinitions(string projectName) {
            var buildServer = Server.GetService<IBuildServer>();
            IBuildDefinitionSpec spec = buildServer.CreateBuildDefinitionSpec(projectName);

            var result = buildServer.QueryBuildDefinitions(spec);
            return result.Definitions;
        }
    }
}