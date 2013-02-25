using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitCommands;
using Nini.Config;
using TeamCitySharp;
using TeamCitySharp.Locators;

namespace GitUI.BuildServerIntegration
{
    internal class TeamCityAdapter : IBuildServerAdapter
    {
        private TeamCityClient Client { get; set; }

        private string ProjectName { get; set; }

        public TeamCityAdapter(string projectName, IConfig config)
        {
            var hostName = config.Get("BuildServerUrl");
            if (string.IsNullOrEmpty(hostName))
            {
                throw new InvalidOperationException();
            }

            Client = new TeamCityClient(hostName);
            ProjectName = projectName;

            Client.Connect(string.Empty, string.Empty, true);
        }

        public IObservable<BuildInfo> CreateObservable(IScheduler scheduler, DateTime? sinceDate = null)
        {
            return Observable.Create<BuildInfo>((observer, cancellationToken) =>
                Task<IDisposable>.Factory.StartNew(() =>
                    {
                        try
                        {
                            var project = Client.ProjectByName(ProjectName);
                            var buildTypes = project.BuildTypes.BuildType;
                            var builds = buildTypes.SelectMany(x => Client.BuildsByBuildLocator(TeamCityBuildLocator2.WithDimensions(BuildTypeLocator.WithId(x.Id), sinceDate: sinceDate, defaultBranch: "any"))).ToArray();
                            var tasks = new List<Task>(8);
                            var buildsLeft = builds.Length;

                            foreach (var build in builds)
                            {
                                var buildHref = build.Href.Replace("guestAuth/", string.Empty);
                                var callByUrlTask =
                                    Task.Factory
                                        .StartNew(() => Client.CallByUrl<dynamic>(buildHref), cancellationToken)
                                        .ContinueWith(
                                            task =>
                                                {
                                                    var buildInfo = CreateBuildInfo(task.Result);
                                                    if (buildInfo.CommitHashList != null)
                                                    {
                                                        observer.OnNext(buildInfo);
                                                    }
                                                },
                                            TaskContinuationOptions.ExecuteSynchronously);

                                tasks.Add(callByUrlTask);
                                --buildsLeft;

                                if (tasks.Count == tasks.Capacity || buildsLeft == 0)
                                {
                                    // TODO: Improve this code to get rid of the Wait call (potentially using IObservable.Buffer(8) to handle the breaking up into batches).
                                    var batchTasks = tasks.ToArray();
                                    tasks.Clear();

                                    Task.WaitAll(batchTasks, cancellationToken);
                                }
                            }

                            observer.OnCompleted();
                        }
                        catch (Exception ex)
                        {
                            observer.OnError(ex);
                        }

                        return Disposable.Empty;
                    }))
                .OnErrorResumeNext(Observable.Empty<BuildInfo>());
        }

        private static BuildInfo CreateBuildInfo(dynamic buildExpando)
        {
            var status = BuildInfo.BuildStatus.Unknown;
            string statusText = buildExpando.statusText;
            object[] changes = buildExpando.revisions != null
                                     ? buildExpando.revisions.revision
                                     : null;
            string[] commitHashList = changes != null
                                          ? changes.Select(change => (string)((dynamic)change).version).ToArray()
                                          : null;

            switch ((string)buildExpando.status)
            {
                case "SUCCESS":
                    status = BuildInfo.BuildStatus.Success;
                    break;
                case "FAILURE":
                    status = BuildInfo.BuildStatus.Failure;
                    break;
            }

            var buildInfo = new BuildInfo
                                {
                                    Id = buildExpando.id.ToString(),
                                    StartDate = buildExpando.startDate,
                                    Status = status,
                                    Description = statusText,
                                    CommitHashList = commitHashList,
                                    Url = buildExpando.webUrl
                                };
            return buildInfo;
        }
    }
}