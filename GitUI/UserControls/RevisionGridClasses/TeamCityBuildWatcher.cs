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

namespace GitUI.UserControls.RevisionGridClasses
{
    internal class TeamCityBuildWatcher : IBuildServerWatcher
    {
        private TeamCityClient Client { get; set; }

        private string ProjectName { get; set; }

        public TeamCityBuildWatcher(string projectName, IConfig config)
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

        public IObservable<BuildInfo> CreateObservable(DateTime? sinceDate = new DateTime?())
        {
            return Observable.Create<BuildInfo>((observer, cancellationToken) =>
                                                    {
                                                        try
                                                        {
                                                            var project = Client.ProjectByName(ProjectName);
                                                            var buildTypes = project.BuildTypes.BuildType;
                                                            var builds =
                                                                buildTypes.SelectMany(
                                                                    x =>
                                                                    Client.BuildsByBuildLocator(
                                                                        TeamCityBuildLocator2.WithDimensions(
                                                                            BuildTypeLocator.WithId(x.Id),
                                                                            sinceDate: sinceDate, defaultBranch: "any")));
                                                            var tasks = new List<Task>(builds.Count());
                                                            foreach (var build in builds)
                                                            {
                                                                cancellationToken.ThrowIfCancellationRequested();

                                                                var buildHref = build.Href.Replace("guestAuth/", string.Empty);
                                                                var callByUrlTask =
                                                                    Task.Factory
                                                                        .StartNew(() => Client.CallByUrl<object>(buildHref), cancellationToken)
                                                                        .ContinueWith(
                                                                            task =>
                                                                                {
                                                                                    dynamic buildExpando = task.Result;

                                                                                    var status = BuildInfo.BuildStatus.Unknown;
                                                                                    string statusText = buildExpando.statusText;
                                                                                    object[] revisions = buildExpando.revisions != null
                                                                                                            ? buildExpando.revisions.revision
                                                                                                            : null;
                                                                                    string revisionVersion = revisions != null
                                                                                                                ? ((dynamic)revisions.Single()).version
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
                                                                                                            Id = buildExpando.id,
                                                                                                            StartDate =
                                                                                                                buildExpando.startDate,
                                                                                                            Status = status,
                                                                                                            Description = statusText,
                                                                                                            CommitHash = revisionVersion,
                                                                                                            Url = build.WebUrl
                                                                                                        };

                                                                                    observer.OnNext(buildInfo);
                                                                                },
                                                                                TaskContinuationOptions.ExecuteSynchronously);

                                                                tasks.Add(callByUrlTask);

                                                                if (tasks.Count == 8)
                                                                {
                                                                    Task.Factory.ContinueWhenAll(tasks.ToArray(), completedTasks => tasks.Clear()).Wait(cancellationToken);
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

                                                        return new Task<IDisposable>(() => Disposable.Empty);
                                                    })
                .SubscribeOn(NewThreadScheduler.Default)
                .OnErrorResumeNext(Observable.Empty<BuildInfo>());
        }
    }
}