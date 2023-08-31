using System.Reactive.Concurrency;
using FluentAssertions;
using GitExtensions.Plugins.GitlabIntegration;
using GitExtensions.Plugins.GitlabIntegration.ApiClient;
using GitExtensions.Plugins.GitlabIntegration.ApiClient.Models;
using GitUIPluginInterfaces.BuildServerIntegration;
using GitUITests;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace GitlabIntegrationTests
{
    [TestFixture]
    internal class GitlabAdapterTests
    {
        private GitlabAdapter _target;

        private IScheduler _scheduler;
        private IGitlabApiClient _apiClient;

        [SetUp]
        public void SetUp()
        {
            _apiClient = Substitute.For<IGitlabApiClient>();

            IGitlabApiClientFactory apiClientFactory = Substitute.For<IGitlabApiClientFactory>();
            apiClientFactory.CreateGitlabApiClient(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).Returns(_apiClient);

            _scheduler = Substitute.For<IScheduler>();

            _target = new GitlabAdapter(apiClientFactory);
            _target.Initialize(Substitute.For<IBuildServerWatcher>(), new MemorySettings(), () => { });
        }

        [Test]
        public void Should_dispose_api_client()
        {
            _apiClient.Dispose();

            _apiClient.Received(1).Dispose();
        }

        [Test]
        public void Should_load_empty_collection()
        {
            PagedResponse<GitlabPipeline> pagedResponse = new()
            {
                TotalPages = 1,
                PageSize = 1,
                PageNumber = 1,
                Total = 0,
                Items = Array.Empty<GitlabPipeline>()
            };

            List<BuildInfo> expected = new();

            _apiClient.GetPipelinesAsync(Arg.Any<DateTime?>(), false, 1, Arg.Any<CancellationToken>())
                .Returns(pagedResponse);

            List<BuildInfo> result = ProcessGetFinishedBuildsRequest();

            _apiClient.Received(1).GetPipelinesAsync(Arg.Any<DateTime?>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
            result.OrderBy(x => x.Id).SequenceEqual(expected.OrderBy(x => x.Id), new BuildInfoEqualityComparer()).Should().BeTrue();
        }

        [Test]
        public void Should_load_single_element_with_correct_attributes()
        {
            PagedResponse<GitlabPipeline> pagedResponse = new()
            {
                TotalPages = 1,
                PageSize = 1,
                PageNumber = 1,
                Total = 1,
                Items = new[]
                {
                    GitlabAdapterTestData.Builds[1].Item2,
                }
            };

            List<BuildInfo> expected = new()
            {
                GitlabAdapterTestData.Builds[1].Item1,
            };

            _apiClient.GetPipelinesAsync(Arg.Any<DateTime?>(), false, 1, Arg.Any<CancellationToken>())
                .Returns(pagedResponse);

            List<BuildInfo> result = ProcessGetFinishedBuildsRequest();

            _apiClient.Received(1).GetPipelinesAsync(Arg.Any<DateTime?>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
            result.OrderBy(x => x.Id).SequenceEqual(expected.OrderBy(x => x.Id), new BuildInfoEqualityComparer()).Should().BeTrue();
        }

        [Test]
        public void Should_load_single_running_build()
        {
            PagedResponse<GitlabPipeline> pagedResponse = new()
            {
                TotalPages = 1,
                PageSize = 1,
                PageNumber = 1,
                Total = 1,
                Items = new[]
                {
                    GitlabAdapterTestData.Builds[-7].Item2,
                }
            };

            List<BuildInfo> expected = new()
            {
                GitlabAdapterTestData.Builds[-7].Item1,
            };

            _apiClient.GetPipelinesAsync(Arg.Any<DateTime?>(), true, 1, Arg.Any<CancellationToken>())
                .Returns(pagedResponse);

            List<BuildInfo> result = ProcessGetRunningBuildsRequest();

            _apiClient.Received(1).GetPipelinesAsync(Arg.Any<DateTime?>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
            result.OrderBy(x => x.Id).SequenceEqual(expected.OrderBy(x => x.Id), new BuildInfoEqualityComparer()).Should().BeTrue();
        }

        [Test]
        public void Should_load_two_pages_with_three_elements()
        {
            PagedResponse<GitlabPipeline> firstPagedResponse = new()
            {
                TotalPages = 2,
                PageSize = 3,
                PageNumber = 1,
                Total = 6,
                Items = new[]
                {
                    GitlabAdapterTestData.Builds[6].Item2,
                    GitlabAdapterTestData.Builds[5].Item2,
                    GitlabAdapterTestData.Builds[4].Item2,
                }
            };

            PagedResponse<GitlabPipeline> secondPagedResponse = new()
            {
                TotalPages = 2,
                PageSize = 3,
                PageNumber = 2,
                Total = 6,
                Items = new[]
                {
                    GitlabAdapterTestData.Builds[3].Item2,
                    GitlabAdapterTestData.Builds[2].Item2,
                    GitlabAdapterTestData.Builds[1].Item2,
                }
            };

            List<BuildInfo> expected = new()
            {
                GitlabAdapterTestData.Builds[6].Item1,
                GitlabAdapterTestData.Builds[5].Item1,
                GitlabAdapterTestData.Builds[4].Item1,
                GitlabAdapterTestData.Builds[3].Item1,
                GitlabAdapterTestData.Builds[2].Item1,
                GitlabAdapterTestData.Builds[1].Item1
            };

            _apiClient.GetPipelinesAsync(Arg.Any<DateTime?>(), false, 1, Arg.Any<CancellationToken>())
                .Returns(firstPagedResponse);
            _apiClient.GetPipelinesAsync(Arg.Any<DateTime?>(), false, 2, Arg.Any<CancellationToken>())
                .Returns(secondPagedResponse);

            List<BuildInfo> result = ProcessGetFinishedBuildsRequest();

            _apiClient.Received(2).GetPipelinesAsync(Arg.Any<DateTime?>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
            result.OrderBy(x => x.Id).SequenceEqual(expected.OrderBy(x => x.Id), new BuildInfoEqualityComparer()).Should().BeTrue();
        }

        [Test]
        public void Should_not_pass_duplicating_builds()
        {
            PagedResponse<GitlabPipeline> firstPagedResponse = new()
            {
                TotalPages = 2,
                PageSize = 3,
                PageNumber = 1,
                Total = 5,
                Items = new[]
                {
                    GitlabAdapterTestData.Builds[5].Item2,
                    GitlabAdapterTestData.Builds[4].Item2,
                    GitlabAdapterTestData.Builds[3].Item2
                }
            };

            PagedResponse<GitlabPipeline> secondPagedResponse = new()
            {
                TotalPages = 2,
                PageSize = 3,
                PageNumber = 2,
                Total = 6,
                Items = new[]
                {
                    GitlabAdapterTestData.Builds[3].Item2,
                    GitlabAdapterTestData.Builds[2].Item2,
                    GitlabAdapterTestData.Builds[1].Item2
                }
            };

            List<BuildInfo> expected = new()
            {
                GitlabAdapterTestData.Builds[5].Item1,
                GitlabAdapterTestData.Builds[4].Item1,
                GitlabAdapterTestData.Builds[3].Item1,
                GitlabAdapterTestData.Builds[2].Item1,
                GitlabAdapterTestData.Builds[1].Item1,
            };

            _apiClient.GetPipelinesAsync(Arg.Any<DateTime?>(), false, 1, Arg.Any<CancellationToken>())
                .Returns(firstPagedResponse);
            _apiClient.GetPipelinesAsync(Arg.Any<DateTime?>(), false, 2, Arg.Any<CancellationToken>())
                .Returns(secondPagedResponse);

            List<BuildInfo> result = ProcessGetFinishedBuildsRequest();

            _apiClient.Received(2).GetPipelinesAsync(Arg.Any<DateTime?>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
            result.OrderBy(x => x.Id).SequenceEqual(expected.OrderBy(x => x.Id), new BuildInfoEqualityComparer()).Should().BeTrue();
        }

        [Test]
        public void Should_load_finished_running_and_finished_builds()
        {
            // Request finished builds
            PagedResponse<GitlabPipeline> firstResponse = new()
            {
                TotalPages = 1,
                PageSize = 1,
                PageNumber = 1,
                Total = 1,
                Items = new[]
                {
                    GitlabAdapterTestData.Builds[1].Item2,
                }
            };

            List<BuildInfo> firstExpected = new()
            {
                GitlabAdapterTestData.Builds[1].Item1,
            };

            _apiClient.GetPipelinesAsync(Arg.Any<DateTime?>(), false, 1, Arg.Any<CancellationToken>())
                .Returns(firstResponse);

            List<BuildInfo> result = ProcessGetFinishedBuildsRequest();

            _apiClient.Received(1).GetPipelinesAsync(Arg.Any<DateTime?>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
            result.OrderBy(x => x.Id).SequenceEqual(firstExpected.OrderBy(x => x.Id), new BuildInfoEqualityComparer()).Should().BeTrue();

            // Request running build
            PagedResponse<GitlabPipeline> runningResponse = new()
            {
                TotalPages = 1,
                PageSize = 1,
                PageNumber = 1,
                Total = 1,
                Items = new[]
                {
                    GitlabAdapterTestData.Builds[-7].Item2,
                }
            };

            List<BuildInfo> runningExpected = new()
            {
                GitlabAdapterTestData.Builds[-7].Item1,
            };

            _apiClient.GetPipelinesAsync(Arg.Any<DateTime?>(), true, 1, Arg.Any<CancellationToken>())
                .Returns(runningResponse);

            result = ProcessGetRunningBuildsRequest();

            _apiClient.Received(2).GetPipelinesAsync(Arg.Any<DateTime?>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
            result.OrderBy(x => x.Id).SequenceEqual(runningExpected.OrderBy(x => x.Id), new BuildInfoEqualityComparer()).Should().BeTrue();

            // Simulate running build to be finished
            PagedResponse<GitlabPipeline> secondResponse = new()
            {
                TotalPages = 1,
                PageSize = 2,
                PageNumber = 1,
                Total = 2,
                Items = new[]
                {
                    GitlabAdapterTestData.Builds[1].Item2,
                    GitlabAdapterTestData.Builds[7].Item2,
                }
            };

            // should receive only updated BuildInfo
            List<BuildInfo> secondExpected = new()
            {
                GitlabAdapterTestData.Builds[7].Item1,
            };

            _apiClient.GetPipelinesAsync(Arg.Any<DateTime?>(), false, 1, Arg.Any<CancellationToken>())
                .Returns(secondResponse);

            result = ProcessGetFinishedBuildsRequest();

            _apiClient.Received(3).GetPipelinesAsync(Arg.Any<DateTime?>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
            result.OrderBy(x => x.Id).SequenceEqual(secondExpected.OrderBy(x => x.Id), new BuildInfoEqualityComparer()).Should().BeTrue();
        }

        [TestCase(3, null, 3)]
        [TestCase(3, 2, 2)]
        [TestCase(null, null, 3)]
        [TestCase(null, 2, 2)]
        public void Should_respect_page_limit(int? totalPages, int? pagesLimit, int expectedPages)
        {
            MemorySettings settings = new();
            settings.SetInt("PagesLimit", pagesLimit);

            _target.Initialize(Substitute.For<IBuildServerWatcher>(), settings, () => { });

            PagedResponse<GitlabPipeline> firstPagedResponse = new()
            {
                TotalPages = totalPages,
                PageSize = 1,
                PageNumber = 1,
                NextPage = 2,
                Total = totalPages,
                Items = new[]
                {
                    GitlabAdapterTestData.Builds[3].Item2,
                }
            };

            PagedResponse<GitlabPipeline> secondPagedResponse = new()
            {
                TotalPages = totalPages,
                PageSize = 1,
                PageNumber = 2,
                NextPage = 3,
                Total = totalPages,
                Items = new[]
                {
                    GitlabAdapterTestData.Builds[2].Item2,
                }
            };

            PagedResponse<GitlabPipeline> thirdPagedResponse = new()
            {
                TotalPages = totalPages,
                PageSize = 1,
                PageNumber = 3,
                NextPage = null,
                Total = totalPages,
                Items = new[]
                {
                    GitlabAdapterTestData.Builds[1].Item2,
                }
            };

            _apiClient.GetPipelinesAsync(Arg.Any<DateTime?>(), false, 1, Arg.Any<CancellationToken>())
                .Returns(firstPagedResponse);
            _apiClient.GetPipelinesAsync(Arg.Any<DateTime?>(), false, 2, Arg.Any<CancellationToken>())
                .Returns(secondPagedResponse);
            _apiClient.GetPipelinesAsync(Arg.Any<DateTime?>(), false, 3, Arg.Any<CancellationToken>())
                .Returns(thirdPagedResponse);

            List<BuildInfo> result = ProcessGetFinishedBuildsRequest();

            result.Count.Should().Be(expectedPages);
        }

        [Test]
        [Ignore("Settings check should be added first")]
        public void Should_handle_unauthorized_access_exception()
        {
            _apiClient.GetPipelinesAsync(Arg.Any<DateTime?>(), false, 1, Arg.Any<CancellationToken>())
                .Throws(new UnauthorizedAccessException());

            List<BuildInfo> result = ProcessGetFinishedBuildsRequest();

            result.Should().BeEmpty();
        }

        private List<BuildInfo> ProcessGetFinishedBuildsRequest()
        {
            ManualResetEvent observableEvent = new(false);
            List<BuildInfo> result = new();

            IObservable<BuildInfo> observable = _target.GetFinishedBuildsSince(_scheduler);
            observable.Subscribe(e =>
                {
                    result.Add(e);
                },
                () =>
                {
                    observableEvent.Set();
                });

            observableEvent.WaitOne();

            return result;
        }

        private List<BuildInfo> ProcessGetRunningBuildsRequest()
        {
            ManualResetEvent observableEvent = new(false);
            List<BuildInfo> result = new();

            IObservable<BuildInfo> observable = _target.GetRunningBuilds(_scheduler);
            observable.Subscribe(e =>
                {
                    result.Add(e);
                },
                () =>
                {
                    observableEvent.Set();
                });

            observableEvent.WaitOne();

            return result;
        }
    }
}
