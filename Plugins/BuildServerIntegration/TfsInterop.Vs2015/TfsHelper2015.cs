using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitUI;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using TfsInterop.Interface;
using BuildStatus = TfsInterop.Interface.BuildStatus;

namespace TfsInterop
{
    public class BuildInfo : IBuild
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public DateTime StartDate { get; set; }
        public BuildStatus Status { get; set; }
        public bool IsFinished { get; set; }
        public string Description { get; set; }
        public string Revision { get; set; }
        public string Url { get; set; }
    }

    public class TfsHelper : ITfsHelper
    {
        private DefinitionReference[] _buildDefinitions2015;
        private IBuildDefinition[] _buildDefinitions;
        private string _hostname;
        private bool _isWebServer;
        private string _urlPrefix;
        private IBuildServer _buildServer;
        private TfsTeamProjectCollection _tfsCollection;
        private BuildHttpClient _buildClient;
        private string _projectName;

        public bool IsDependencyOk()
        {
            try
            {
                Trace.WriteLine("Test if Microsoft.TeamFoundation.Build assemblies dependencies are present : " + Microsoft.TeamFoundation.Build.Client.BuildStatus.Succeeded.ToString("G"));
                return IsDependencyOk2015();
            }
            catch
            {
                return false;
            }
        }

        public void ConnectToTfsServer(string hostname, string teamCollection, string projectName, Regex buildDefinitionNameFilter = null)
        {
            _hostname = hostname;

            _isWebServer = _hostname.Contains("://");
            try
            {
                string url;
                if (_isWebServer)
                {
                    _hostname = _hostname.TrimEnd('\\', '/');
                    url = _hostname + "/" + teamCollection;
                    _urlPrefix = hostname + "/" + teamCollection + "/" + projectName + "/_build#buildUri=";
                }
                else
                {
                    url = "http://" + _hostname + ":8080/tfs/" + teamCollection;
                    _urlPrefix = "http://" + hostname + ":8080/tfs/" + (string.IsNullOrEmpty(teamCollection) ? "" : teamCollection + "/") + "Build/Build.aspx?artifactMoniker=";
                }

                _tfsCollection = new TfsTeamProjectCollection(new Uri(url), new TfsClientCredentials());

                _buildServer = _tfsCollection.GetService<IBuildServer>();

                var buildDefinitions = _buildServer.QueryBuildDefinitions(projectName);

                if (buildDefinitions.Length != 0)
                {
                    _buildDefinitions = string.IsNullOrWhiteSpace(buildDefinitionNameFilter.ToString())
                        ? buildDefinitions
                        : buildDefinitions.Where(b => buildDefinitionNameFilter.IsMatch(b.Name)).ToArray();
                }

                ThreadHelper.JoinableTaskFactory.RunAsync(() => ConnectToTfsServer2015Async(hostname, teamCollection, projectName, buildDefinitionNameFilter));
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        public IReadOnlyList<IBuild> QueryBuilds(DateTime? sinceDate, bool? running)
        {
            var result = new List<IBuild>();
            foreach (var buildDefinition in _buildDefinitions)
            {
                var buildSpec = _buildServer.CreateBuildDetailSpec(buildDefinition);
                buildSpec.InformationTypes = null;
                if (sinceDate.HasValue)
                {
                    buildSpec.MinFinishTime = sinceDate.Value;
                }

                if (running.HasValue && running.Value)
                {
                    buildSpec.Status = Microsoft.TeamFoundation.Build.Client.BuildStatus.InProgress;
                }

                result.AddRange(_buildServer.QueryBuilds(buildSpec).Builds.Select(b =>
                {
                    var id = b.Uri.AbsoluteUri.Substring(b.Uri.AbsoluteUri.LastIndexOf('/') + 1);
                    string duration = string.Empty;
                    if (b.Status != Microsoft.TeamFoundation.Build.Client.BuildStatus.NotStarted
                        && b.Status != Microsoft.TeamFoundation.Build.Client.BuildStatus.None
                        && b.Status != Microsoft.TeamFoundation.Build.Client.BuildStatus.Stopped)
                    {
                        if (b.Status == Microsoft.TeamFoundation.Build.Client.BuildStatus.InProgress)
                        {
                            duration = " / " + GetDuration(DateTime.Now - b.StartTime);
                        }
                        else
                        {
                            duration = " / " + GetDuration(b.FinishTime - b.StartTime);
                        }
                    }

                    return new BuildInfo
                    {
                        Id = id,
                        Label = b.BuildNumber,
                        StartDate = b.StartTime,
                        Status = ConvertStatus(b.Status),
                        IsFinished = b.BuildFinished,
                        Description = GetStatus(b) + duration,
                        Revision = b.SourceGetVersion,
                        Url = _urlPrefix + (_isWebServer
                                      ? Uri.EscapeDataString(b.Uri.AbsoluteUri) + "&_a=summary"
                                      : id),
                    };
                }).Cast<IBuild>().ToList());
            }

            result = ThreadHelper.JoinableTaskFactory.Run(() => QueryBuilds2015Async(result, sinceDate, running));
            return result;
        }

        private static string GetStatus(IBuildDetail build)
        {
            if (build.Status == Microsoft.TeamFoundation.Build.Client.BuildStatus.NotStarted)
            {
                return "Not started";
            }

            if (build.Status == Microsoft.TeamFoundation.Build.Client.BuildStatus.InProgress)
            {
                return "In progress...";
            }

            if (build.Status == Microsoft.TeamFoundation.Build.Client.BuildStatus.None)
            {
                return "No status";
            }

            if (build.Status == Microsoft.TeamFoundation.Build.Client.BuildStatus.Stopped)
            {
                return "Stopped";
            }

            if (build.Status == Microsoft.TeamFoundation.Build.Client.BuildStatus.Succeeded)
            {
                return "OK";
            }

            if (build.Status == Microsoft.TeamFoundation.Build.Client.BuildStatus.Failed
                || build.Status == Microsoft.TeamFoundation.Build.Client.BuildStatus.PartiallySucceeded)
            {
                if (build.CompilationStatus != Microsoft.TeamFoundation.Build.Client.BuildPhaseStatus.Succeeded)
                {
                    return "Compilation: " + GetStatusDescription(build.CompilationStatus);
                }

                if (build.TestStatus != Microsoft.TeamFoundation.Build.Client.BuildPhaseStatus.Succeeded)
                {
                    return "Tests: " + GetStatusDescription(build.TestStatus);
                }

                if (build.Status == Microsoft.TeamFoundation.Build.Client.BuildStatus.Failed)
                {
                    return "KO";
                }

                return "Partially Succeeded";
            }

            return "-";
        }

        private static string GetStatusDescription(Microsoft.TeamFoundation.Build.Client.BuildPhaseStatus status)
        {
            if (status == Microsoft.TeamFoundation.Build.Client.BuildPhaseStatus.Succeeded)
            {
                return "OK";
            }

            if (status == Microsoft.TeamFoundation.Build.Client.BuildPhaseStatus.Failed)
            {
                return "KO";
            }

            return "-";
        }

        private static BuildStatus ConvertStatus(Microsoft.TeamFoundation.Build.Client.BuildStatus status)
        {
            switch (status)
            {
                case Microsoft.TeamFoundation.Build.Client.BuildStatus.Succeeded:
                    return BuildStatus.Success;
                case Microsoft.TeamFoundation.Build.Client.BuildStatus.Stopped:
                    return BuildStatus.Stopped;
                case Microsoft.TeamFoundation.Build.Client.BuildStatus.Failed:
                    return BuildStatus.Failure;
                case Microsoft.TeamFoundation.Build.Client.BuildStatus.PartiallySucceeded:
                    return BuildStatus.Unstable;
                case Microsoft.TeamFoundation.Build.Client.BuildStatus.InProgress:
                    return BuildStatus.InProgress;
                default:
                    return BuildStatus.Unknown;
            }
        }

        public bool IsDependencyOk2015()
        {
            try
            {
                Trace.WriteLine("Test if Microsoft.TeamFoundation.Build.WebApi assemblies dependencies are present : " + Microsoft.TeamFoundation.Build.WebApi.BuildStatus.Completed.ToString("G"));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task ConnectToTfsServer2015Async(string hostname, string teamCollection, string projectName, Regex buildDefinitionNameFilter = null)
        {
            _hostname = hostname;

            _isWebServer = _hostname.Contains("://");
            try
            {
                string url;
                if (_isWebServer)
                {
                    _hostname = _hostname.TrimEnd('\\', '/');
                    url = _hostname + "/" + teamCollection;
                    _urlPrefix = hostname + "/" + teamCollection + "/" + projectName + "/_build?_a=summary&buildId=";
                }
                else
                {
                    url = "http://" + _hostname + ":8080/tfs/" + teamCollection;
                    _urlPrefix = "http://" + hostname + ":8080/tfs/" + (string.IsNullOrEmpty(teamCollection) ? "" : teamCollection + "/") + projectName + "/_build?_a=summary&buildId=";
                }

                var connection = new VssConnection(new Uri(url), new VssCredentials(true));

                connection.Settings.BypassProxyOnLocal = false;
                BuildHttpClient buildClient = await connection.GetClientAsync<BuildHttpClient>().ConfigureAwait(false);
                var definitions = await buildClient.GetDefinitionsAsync(project: projectName).ConfigureAwait(false);
                var buildDefinitions = new List<DefinitionReference>();

                foreach (var def in definitions)
                {
                    if (string.IsNullOrWhiteSpace(buildDefinitionNameFilter.ToString()) || buildDefinitionNameFilter.IsMatch(def.Name))
                    {
                        buildDefinitions.Add(def);
                    }
                }

                _buildDefinitions2015 = buildDefinitions.ToArray();
                _buildClient = buildClient;
                _projectName = projectName;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        public async Task<List<IBuild>> QueryBuilds2015Async(List<IBuild> result, DateTime? sinceDate, bool? running)
        {
            ////var result = new List<IBuild>();
            if (_buildDefinitions2015 == null)
            {
                return result;
            }
            ////foreach (var buildDefinition in _buildDefinitions)
            ////{
            Microsoft.TeamFoundation.Build.WebApi.BuildStatus statusFilter = Microsoft.TeamFoundation.Build.WebApi.BuildStatus.All;
            if (running.HasValue && running.Value)
            {
                statusFilter = Microsoft.TeamFoundation.Build.WebApi.BuildStatus.InProgress;
            }

            ////List<Build> builds = Vs2015.AsyncHelpers.RunSync<List<Build>(()=>_buildClient.GetBuildsAsync(definitions: new int[] { buildDefinition.Id },
            ////                                       minFinishTime: sinceDate,
            ////                                       statusFilter: statusFilter));
            var builds = await _buildClient.GetBuildsAsync(project: _projectName,
                                                      definitions: _buildDefinitions2015.Select(b => b.Id),
                                                      minFinishTime: sinceDate,
                                                      statusFilter: statusFilter).ConfigureAwait(false);
            ////while (!task.IsCompleted) { }
            ////task.Wait();
            foreach (var b in builds)
            {
                var id = b.Id.ToString();
                string duration = string.Empty;
                if (b.Status.HasValue
                    && b.Status.Value != Microsoft.TeamFoundation.Build.WebApi.BuildStatus.None
                    && b.Status.Value != Microsoft.TeamFoundation.Build.WebApi.BuildStatus.NotStarted
                    && b.Status.Value != Microsoft.TeamFoundation.Build.WebApi.BuildStatus.Postponed
                    && b.StartTime.HasValue)
                {
                    if (b.Status == Microsoft.TeamFoundation.Build.WebApi.BuildStatus.InProgress)
                    {
                        duration = " / " + GetDuration(DateTime.UtcNow - b.StartTime.Value);
                    }
                    else
                    {
                        if (b.FinishTime.HasValue)
                        {
                            duration = " / " + GetDuration(b.FinishTime.Value - b.StartTime.Value);
                        }
                        else
                        {
                            duration = " / ???";
                        }
                    }
                }

                if (b.StartTime.HasValue)
                {
                    result.Add(new BuildInfo
                    {
                        Id = id,
                        Label = b.BuildNumber,
                        StartDate = b.StartTime.Value,
                        Status = ConvertStatus2015(b),
                        IsFinished = b.Status == Microsoft.TeamFoundation.Build.WebApi.BuildStatus.Completed,
                        Description = GetStatus2015(b) + duration,
                        Revision = GetCommitFromSourceVersion(b.SourceVersion),
                        Url = _urlPrefix + id
                        ////Url = b.Url
                    });
                }
            }

            return result;
        }

        private static string GetCommitFromSourceVersion(string sourceVersion)
        {
            if (sourceVersion.LastIndexOf(':') > 0)
            {
                return sourceVersion.Substring(sourceVersion.LastIndexOf(':') + 1);
            }

            return sourceVersion;
        }

        private static string GetStatus2015(Build build)
        {
            if (build.Status == Microsoft.TeamFoundation.Build.WebApi.BuildStatus.NotStarted)
            {
                return "Not started";
            }

            if (build.Status == Microsoft.TeamFoundation.Build.WebApi.BuildStatus.InProgress)
            {
                return "In progress...";
            }

            if (build.Status == Microsoft.TeamFoundation.Build.WebApi.BuildStatus.None)
            {
                return "No status";
            }

            if (build.Status == Microsoft.TeamFoundation.Build.WebApi.BuildStatus.Postponed)
            {
                return "Postponed";
            }

            if (build.Status == Microsoft.TeamFoundation.Build.WebApi.BuildStatus.Cancelling)
            {
                return "Cancelling";
            }

            if (build.Status == Microsoft.TeamFoundation.Build.WebApi.BuildStatus.Completed)
            {
                if (build.Result.Value == BuildResult.Failed)
                {
                    return "KO";
                }

                if (build.Result.Value == BuildResult.PartiallySucceeded)
                {
                    return "Partially Succeeded";
                }

                if (build.Result.Value == BuildResult.Succeeded)
                {
                    return "OK";
                }
            }

            return "-";
        }

        ////private static string GetStatusDescription(BuildPhaseStatus status)
        ////{
        ////   if (status == BuildPhaseStatus.Succeeded)
        ////       return "OK";
        ////   if (status == BuildPhaseStatus.Failed)
        ////       return "KO";
        ////   return "-";
        ////}

        private static BuildStatus ConvertStatus2015(Build build)
        {
            if (build.Status == Microsoft.TeamFoundation.Build.WebApi.BuildStatus.InProgress)
            {
                return BuildStatus.InProgress;
            }

            if (build.Status == Microsoft.TeamFoundation.Build.WebApi.BuildStatus.Completed)
            {
                if (build.Result.Value == BuildResult.Failed)
                {
                    return BuildStatus.Failure;
                }

                if (build.Result.Value == BuildResult.Succeeded)
                {
                    return BuildStatus.Success;
                }

                if (build.Result.Value == BuildResult.Canceled)
                {
                    return BuildStatus.Stopped;
                }

                if (build.Result.Value == BuildResult.PartiallySucceeded)
                {
                    return BuildStatus.Unstable;
                }
            }

            return BuildStatus.Unknown;
        }

        private static string GetDuration(TimeSpan duration)
        {
            string s = string.Empty;
            if (duration.Hours != 0)
            {
                s += duration.Hours + "h";
            }

            if (duration.Minutes != 0)
            {
                s += duration.Minutes.ToString("00") + "m";
            }

            s += duration.Seconds.ToString("00") + "s";
            return s;
        }

        public void Dispose()
        {
            _buildServer = null;
            _tfsCollection?.Dispose();
            _buildDefinitions = null;
            _buildDefinitions2015 = null;
            _buildClient = null;
            GC.Collect();
        }
    }
}
