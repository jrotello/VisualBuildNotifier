using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;

namespace VisualBuildNotifier.Services {
    public class TfsService: ITfsService {

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
                    _server = new TfsTeamProjectCollection(ServerUri);
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
            var buildDefinition = buildServer.GetBuildDefinition(projectName, buildDefinitionName);
            var spec = buildServer.CreateBuildDetailSpec(buildDefinition);
            spec.Status = BuildStatus.InProgress | BuildStatus.Succeeded | BuildStatus.PartiallySucceeded | BuildStatus.Failed;
            spec.MaxBuildsPerDefinition = maxBuilds;
            var result = buildServer.QueryBuilds(spec);
            return result.Builds;
        }

        public IEnumerable<IBuildDefinition> GetBuildDefinitions(string projectName) {
            var buildServer = Server.GetService<IBuildServer>();
            IBuildDefinitionSpec[] specs = new[] {
                buildServer.CreateBuildDefinitionSpec(projectName)
            };

            var asyncResult = buildServer.BeginQueryBuildDefinitions(specs, null, null);
            asyncResult.AsyncWaitHandle.WaitOne();
            asyncResult.AsyncWaitHandle.Close();
            var results = buildServer.EndQueryBuildDefinitions(asyncResult);

            return results.Length > 0 ? results[0].Definitions : Enumerable.Empty<IBuildDefinition>();
        }
    }
}