using System;
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
                                                            foreach (var build in builds)
                                                            {
                                                                cancellationToken.ThrowIfCancellationRequested();

                                                                dynamic buildExpando =
                                                                    Client.CallByUrl<object>(
                                                                        build.Href.Replace("guestAuth/", string.Empty));
                                                                var status = BuildInfo.BuildStatus.Unknown;
                                                                string statusText = buildExpando.statusText;
                                                                object[] revisions = buildExpando.revisions != null
                                                                                         ? buildExpando.revisions.revision
                                                                                         : null;
                                                                string revisionVersion = revisions != null
                                                                                             ? ((dynamic)revisions.Single()).version
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
                                                                                        CommitHash = revisionVersion
                                                                                    };

                                                                observer.OnNext(buildInfo);
                                                            }

                                                            observer.OnCompleted();
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