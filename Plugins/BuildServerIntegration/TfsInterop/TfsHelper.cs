using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Build.Client;
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
        private IBuildDefinition _buildDefinition;
        private string _hostname;
        private string _teamCollection;
        private string _projectName;
        private bool _isWebServer;

        public bool IsDependencyOk()
        {
            try
            {
                Trace.WriteLine("Test if Microsoft.TeamFoundation.Build assemblies dependencies are present : " + Microsoft.TeamFoundation.Build.Client.BuildStatus.Succeeded.ToString("G"));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void ConnectToTfsServer(string hostname, string teamCollection, string projectName, string buildDefinitionName = null)
        {
            _hostname = hostname;
            _teamCollection = teamCollection;
            _projectName = projectName;

            _isWebServer = _hostname.Contains("://");

            try
            {
                string url;
                if (_isWebServer)
                {
                    _hostname = _hostname.TrimEnd('\\', '/');
                    url = _hostname + "/" + teamCollection;
                }
                else
                {
                    url = "http://" + _hostname + ":8080/tfs/" + teamCollection;
                }

                // Get a connection to the desired Team Foundation Server
                var tfServer = TeamFoundationServerFactory.GetServer(url, new UICredentialsProvider());

                // Get a reference to a build service
                var buildServer = tfServer.GetService<IBuildServer>();

                // Retrieve a list of build definitions
                var buildDefs = buildServer.QueryBuildDefinitions(projectName);

                if (buildDefs.Length != 0)
                {
                    if (string.IsNullOrWhiteSpace(buildDefinitionName))
                        _buildDefinition = buildDefs[0];
                    else
                    {
                        foreach (var buildDefinition in buildDefs)
                        {
                            if (string.Compare(buildDefinition.Name, buildDefinitionName, true) == 0)
                            {
                                _buildDefinition = buildDefinition;
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        public IList<IBuild> QueryBuilds()
        {
            return _buildDefinition.QueryBuilds().Select(b =>
                {
                    var id = b.Uri.AbsoluteUri.Substring(b.Uri.AbsoluteUri.LastIndexOf('/') + 1);
                    string duration = string.Empty;
                    if (b.Status != Microsoft.TeamFoundation.Build.Client.BuildStatus.NotStarted
                        && b.Status != Microsoft.TeamFoundation.Build.Client.BuildStatus.None
                        && b.Status != Microsoft.TeamFoundation.Build.Client.BuildStatus.Stopped)
                    {
                        if (b.Status == Microsoft.TeamFoundation.Build.Client.BuildStatus.InProgress)
                            duration += " / " + GetDuration(DateTime.Now - b.StartTime);
                        else
                            duration += " / " + GetDuration(b.FinishTime - b.StartTime);
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
                            Url = _isWebServer ? _hostname + "/" + _teamCollection + "/" + _projectName + "/_build#buildUri=" + Uri.EscapeDataString(b.Uri.AbsoluteUri) + "&_a=summary"
                                : "http://" + _hostname + ":8080/tfs/Build/Build.aspx?artifactMoniker=" + id,
                        };
                }).Cast<IBuild>().ToList();
        }

        private static string GetStatus(IBuildDetail build)
        {
            if (build.Status == Microsoft.TeamFoundation.Build.Client.BuildStatus.NotStarted)
                return "Not started";
            if (build.Status == Microsoft.TeamFoundation.Build.Client.BuildStatus.InProgress)
                return "In progress...";
            if (build.Status == Microsoft.TeamFoundation.Build.Client.BuildStatus.None)
                return "No status";
            if (build.Status == Microsoft.TeamFoundation.Build.Client.BuildStatus.Stopped)
                return "Stopped";
            if (build.Status == Microsoft.TeamFoundation.Build.Client.BuildStatus.Succeeded)
                return "OK";
            if (build.Status == Microsoft.TeamFoundation.Build.Client.BuildStatus.Failed
                || build.Status == Microsoft.TeamFoundation.Build.Client.BuildStatus.PartiallySucceeded)
            {
                if (build.CompilationStatus != BuildPhaseStatus.Succeeded)
                    return "Compilation: " + GetStatusDescription(build.CompilationStatus);
                if (build.TestStatus != BuildPhaseStatus.Succeeded)
                    return "Tests: " + GetStatusDescription(build.TestStatus);
                if (build.Status == Microsoft.TeamFoundation.Build.Client.BuildStatus.Failed)
                    return "KO";
                return "Partially Succeeded";
            }
            return "-";
        }

        private static string GetStatusDescription(BuildPhaseStatus status)
        {
            if (status == BuildPhaseStatus.Succeeded)
                return "OK";
            if (status == BuildPhaseStatus.Failed)
                return "KO";
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

        private static string GetDuration(TimeSpan duration)
        {
            string s = string.Empty;
            if (duration.Hours != 0)
                s += duration.Hours + "h";
            if (duration.Minutes != 0)
                s += duration.Minutes.ToString("00") + "m";
            s += duration.Seconds.ToString("00") + "s";
            return s;
        }
    }
}
