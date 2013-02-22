using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
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
        }

        public IObservable<BuildInfo> CreateObservable(IScheduler scheduler, DateTime? sinceDate = null)
        {
            return Observable.Create<BuildInfo>(observer =>
                                                    {
                                                        var cancellationTokenSource = new CancellationTokenSource();

                                                        scheduler.Schedule(() =>
                                                                               {
                                                                                   try
                                                                                   {
                                                                                       var project = Client.ProjectByName(ProjectName);
                                                                                       var buildTypes = project.BuildTypes.BuildType;
                                                                                       var builds = buildTypes.SelectMany(x => Client.BuildsByBuildLocator(TeamCityBuildLocator2.WithDimensions(BuildTypeLocator.WithId(x.Id), sinceDate: sinceDate, defaultBranch: "any")));
                                                                                       var tasks = new List<Task>(builds.Count());

                                                                                       foreach (var build in builds)
                                                                                       {
                                                                                           var buildHref = build.Href.Replace("guestAuth/", string.Empty);
                                                                                           var callByUrlTask =
                                                                                               Task.Factory
                                                                                                   .StartNew(() => Client.CallByUrl<dynamic>(buildHref), cancellationTokenSource.Token)
                                                                                                   .ContinueWith(
                                                                                                       task => { observer.OnNext(CreateBuildInfo(task.Result)); },
                                                                                                       TaskContinuationOptions.ExecuteSynchronously);

                                                                                           tasks.Add(callByUrlTask);

                                                                                           if (tasks.Count == 8)
                                                                                           {
                                                                                               // TODO: Improve this code to get rid of the Wait call (potentially using IObservable.Buffer(8) to handle the breaking up into batches).
                                                                                               Task.Factory
                                                                                                   .ContinueWhenAll(tasks.ToArray(), completedTasks => tasks.Clear())
                                                                                                   .Wait(cancellationTokenSource.Token);
                                                                                           }
                                                                                       }

                                                                                       Task.Factory.ContinueWhenAll(
                                                                                               tasks.ToArray(),
                                                                                               completedTasks =>
                                                                                                   {
                                                                                                       var firstFaultedTask = completedTasks.FirstOrDefault(task => task.IsFaulted);
                                                                                                       if (firstFaultedTask != null)
                                                                                                       {
                                                                                                           observer.OnError(firstFaultedTask.Exception);
                                                                                                       }
                                                                                                       else
                                                                                                       {
                                                                                                           observer.OnCompleted();
                                                                                                       }
                                                                                                   });
                                                                                   }
                                                                                   catch (Exception ex)
                                                                                   {
                                                                                       observer.OnError(ex);
                                                                                   }
                                                                               });

                                                        var finallyCallback = Disposable.Create(cancellationTokenSource.Cancel);

                                                        return finallyCallback;
                                                    })
                .OnErrorResumeNext(Observable.Empty<BuildInfo>());
        }

        private static BuildInfo CreateBuildInfo(dynamic buildExpando)
        {
            var status = BuildInfo.BuildStatus.Unknown;
            string statusText = buildExpando.statusText;
            object[] revisions = buildExpando.revisions != null
                                     ? buildExpando.revisions.revision
                                     : null;
            string revisionVersion = revisions != null
                                         ? ((dynamic) revisions.Single()).version
                                         : null;

            switch ((string) buildExpando.status)
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
                                    Id = buildExpando.id,
                                    StartDate =
                                        buildExpando.startDate,
                                    Status = status,
                                    Description = statusText,
                                    CommitHash = revisionVersion,
                                    Url = buildExpando.webUrl
                                };
            return buildInfo;
        }
    }
}