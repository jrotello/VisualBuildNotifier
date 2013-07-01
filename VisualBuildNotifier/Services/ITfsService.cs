using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;

namespace VisualBuildNotifier.Services {
    public interface ITfsService {
        IBuildDetail GetLatestBuildInfo(string projectName, string buildDefinitionName);
        IEnumerable<IBuildDefinition> GetBuildDefinitions(string projectName);
    }
}