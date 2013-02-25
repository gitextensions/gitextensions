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
using TeamCitySharp.DomainEntities;
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
            Client.Authenticate();
        }

        public IObservable<BuildInfo> CreateObservable(IScheduler scheduler, DateTime? sinceDate = null)
        {
            return Observable.Create<BuildInfo>((observer, cancellationToken) =>
                Task<IDisposable>.Factory.StartNew(() =>
                    {
                        try
                        {
                            var project = Client.Projects.ByName(ProjectName);
                            var buildTypes = project.BuildTypes.BuildType;
                            var builds = buildTypes.SelectMany(x => Client.Builds.ByBuildLocator(BuildLocator.WithDimensions(BuildTypeLocator.WithId(x.Id), sinceDate: sinceDate, branch: "default:any"))).ToArray();
                            var tasks = new List<Task>(8);
                            var buildsLeft = builds.Length;

                            foreach (var build in builds)
                            {
                                string buildId = build.Id;
                                var callByUrlTask =
                                    Task.Factory
                                        .StartNew(
                                            () =>
                                                {
                                                    var buildDetails = Client.BuildDetails.ByBuildId(buildId);
                                                    buildDetails.Changes.Change = Client.Changes.ByBuildLocator(BuildLocator.WithId(Convert.ToInt64(buildId)));
                                                    return buildDetails;
                                                },
                                                cancellationToken)
                                        .ContinueWith(
                                            task =>
                                                {
                                                    var buildDetails = task.Result;
                                                    if (buildDetails.Changes.Change.Any())
                                                    {
                                                        var buildInfo = CreateBuildInfo(task.Result);
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

        private static BuildInfo CreateBuildInfo(Build build)
        {
            BuildInfo.BuildStatus status;
            string[] commitHashList = build.Changes.Change.Select(change => change.Version).ToArray();

            switch (build.Status)
            {
                case "SUCCESS":
                    status = BuildInfo.BuildStatus.Success;
                    break;
                case "FAILURE":
                    status = BuildInfo.BuildStatus.Failure;
                    break;
                default:
                    status = BuildInfo.BuildStatus.Unknown;
                    break;
            }

            var buildInfo = new BuildInfo
                                {
                                    Id = build.Id,
                                    StartDate = build.StartDate,
                                    Status = status,
                                    Description = build.StatusText,
                                    CommitHashList = commitHashList,
                                    Url = build.WebUrl
                                };
            return buildInfo;
        }
    }
}