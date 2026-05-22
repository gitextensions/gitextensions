using System.Reflection;
using System.Text.Json.Nodes;
using CommonTestUtils;
using GitExtensions.Extensibility.BuildServerIntegration;
using GitExtensions.Extensibility.Git;
using JenkinsIntegration;

namespace JenkinsIntegrationTests;
public class JenkinsAdapterTests
{
    private JenkinsAdapter _adapter = null!;

    [SetUp]
    public void SetUp()
    {
        _adapter = new JenkinsAdapter();
    }

    [TearDown]
    public void TearDown()
    {
        _adapter.Dispose();
    }

    [Test]
    public void CreateBuildInfo_should_parse_successful_freestyle_build()
    {
        JsonNode json = LoadMockJson("JenkinsResult_freestyle_success.json");

        BuildInfo? result = _adapter.CreateBuildInfo(json);

        result.Should().NotBeNull();
        result!.Id.Should().Be("42");
        result.Status.Should().Be(BuildStatus.Success);
        result.Url.Should().Be("https://jenkins.example.com/job/my-project/42/");
        result.Duration.Should().Be(600000);
        result.CommitHashList.Should().HaveCount(1);
        result.CommitHashList[0].ToString().Should().Be("d4295d591a45c67d812aa1c4fa17b6a0f2ea3114");
    }

    [Test]
    public void CreateBuildInfo_should_parse_failed_build()
    {
        JsonNode json = LoadMockJson("JenkinsResult_freestyle_failure.json");

        BuildInfo? result = _adapter.CreateBuildInfo(json);

        result.Should().NotBeNull();
        result!.Id.Should().Be("43");
        result.Status.Should().Be(BuildStatus.Failure);
        result.Duration.Should().Be(120000);
        result.CommitHashList.Should().HaveCount(1);
        result.CommitHashList[0].ToString().Should().Be("a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0");
    }

    [Test]
    public void CreateBuildInfo_should_parse_running_build()
    {
        JsonNode json = LoadMockJson("JenkinsResult_freestyle_running.json");

        BuildInfo? result = _adapter.CreateBuildInfo(json);

        result.Should().NotBeNull();
        result!.Status.Should().Be(BuildStatus.InProgress);
        result.Duration.Should().BeNull();
    }

    [Test]
    public void CreateBuildInfo_should_parse_build_with_test_results()
    {
        JsonNode json = LoadMockJson("JenkinsResult_with_tests.json");

        BuildInfo? result = _adapter.CreateBuildInfo(json);

        result.Should().NotBeNull();
        result!.Status.Should().Be(BuildStatus.Unstable);
        result.Description.Should().Contain("100 tests");
        result.Description.Should().Contain("2 failed");
        result.Description.Should().Contain("5 skipped");
    }

    [Test]
    public void CreateBuildInfo_should_parse_build_with_multiple_branches()
    {
        JsonNode json = LoadMockJson("JenkinsResult_with_branches.json");

        BuildInfo? result = _adapter.CreateBuildInfo(json);

        result.Should().NotBeNull();
        result!.CommitHashList.Should().HaveCount(1);
        result.CommitHashList[0].ToString().Should().Be("e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4");
    }

    [Test]
    public void CreateBuildInfo_should_set_start_date_from_timestamp()
    {
        JsonNode json = LoadMockJson("JenkinsResult_freestyle_success.json");

        BuildInfo? result = _adapter.CreateBuildInfo(json);

        result.Should().NotBeNull();
        result!.StartDate.Year.Should().Be(2019);
    }

    private static JsonNode LoadMockJson(string filename)
    {
        string json = EmbeddedResourceLoader.Load(
            Assembly.GetExecutingAssembly(),
            $"JenkinsIntegrationTests.MockData.{filename}");
        return JsonNode.Parse(json)!;
    }
}
