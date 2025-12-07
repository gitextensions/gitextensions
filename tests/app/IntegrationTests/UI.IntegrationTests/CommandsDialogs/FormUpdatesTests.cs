using AwesomeAssertions;
using CommonTestUtils;
using GitUI;
using GitUI.CommandsDialogs.BrowseDialog;

namespace GitExtensions.UITests.CommandsDialogs;

[Apartment(ApartmentState.STA)]
public class FormUpdatesTests
{
    // Created once for the fixture
    private ReferenceRepository _referenceRepository;

    // Created once for each test
    private GitUICommands _commands;

    [SetUp]
    public void SetUp()
    {
        _referenceRepository = new ReferenceRepository();
        _commands = new GitUICommands(GlobalServiceContainer.CreateDefaultMockServiceContainer(), _referenceRepository.Module);
    }

    [TearDown]
    public void TearDown()
    {
        _referenceRepository.Dispose();
    }

    [Test]
    public void Should_hide_NET_runtime_link_if_no_required_version()
    {
        RunFormTest(
            form =>
            {
                FormUpdates.TestAccessor accessor = form.GetTestAccessor();

                accessor.DisplayNetRuntimeLink("Required: .NET {0} Desktop Runtime {1} or later {2}.x", requiredNetRuntimeVersion: null);
                accessor.linkRequiredNetRuntime.Visible.Should().BeFalse();
            });
    }

    [TestCaseSource(nameof(NetRuntimeLinkTextTestCases))]
    public void Should_NET_runtime_link_text_be_correct(Version runtimeVersion, string format, string expectedText, LinkArea expectedLinkArea)
    {
        RunFormTest(
            form =>
            {
                FormUpdates.TestAccessor accessor = form.GetTestAccessor();

                accessor.DisplayNetRuntimeLink(format, runtimeVersion);

                accessor.linkRequiredNetRuntime.Visible.Should().BeTrue();
                accessor.linkRequiredNetRuntime.Text.Should().Be(expectedText);
                accessor.linkRequiredNetRuntime.LinkArea.Should().Be(expectedLinkArea);
            });
    }

    private static IEnumerable<TestCaseData> NetRuntimeLinkTextTestCases()
    {
        yield return new TestCaseData(
            new Version(8, 10, 134),
            "Required: .NET {0} Desktop Runtime {1} or later {2}.x",
            "Required: .NET 8.10 Desktop Runtime 8.10.134 or later 8.x",
            new LinkArea(36, 8));

        yield return new TestCaseData(
            new Version(10, 0, 2),
            "Требуется: .NET {0} Desktop Runtime {1} или более поздняя версия {2}.x",
            "Требуется: .NET 10.0 Desktop Runtime 10.0.2 или более поздняя версия 10.x",
            new LinkArea(37, 6));

        yield return new TestCaseData(
            new Version(7, 11, 10),
            "Erforderlich: .NET {0} Desktop Runtime {1} oder höher {2}.x",
            "Erforderlich: .NET 7.11 Desktop Runtime 7.11.10 oder höher 7.x",
            new LinkArea(40, 7));
    }

    [TestCaseSource(nameof(NetRuntimeLinkTestCases))]
    public void Should_NET_runtime_link_url_be_correct(Version runtimeVersion, string expectedUrl)
    {
        RunFormTest(
            form =>
            {
                FormUpdates.TestAccessor accessor = form.GetTestAccessor();
                accessor.DisplayNetRuntimeLink("Required: .NET {0} Desktop Runtime {1} or later {2}.x", runtimeVersion);

                accessor.NetRuntimeDownloadUrl.Should().Be(expectedUrl);
            });
    }

    private static IEnumerable<TestCaseData> NetRuntimeLinkTestCases()
    {
        yield return new TestCaseData(
            new Version(8, 10, 134),
            "https://aka.ms/dotnet-core-applaunch?missing_runtime=true&arch=X64&rid=win-X64&apphost_version=8.10.134&gui=true");

        yield return new TestCaseData(
            new Version(10, 0, 2),
            "https://aka.ms/dotnet-core-applaunch?missing_runtime=true&arch=X64&rid=win-X64&apphost_version=10.0.2&gui=true");

        yield return new TestCaseData(
            new Version(7, 11, 10),
            "https://aka.ms/dotnet-core-applaunch?missing_runtime=true&arch=X64&rid=win-X64&apphost_version=7.11.10&gui=true");
    }

    private void RunFormTest(Action<FormUpdates> testDriver)
    {
        RunFormTest(
            form =>
            {
                testDriver(form);
                return Task.CompletedTask;
            });
    }

    private void RunFormTest(Func<FormUpdates, Task> testDriverAsync)
    {
        UITest.RunForm(
            () =>
            {
                using FormUpdates form = new(new Version(4, 2, 0));
                form.ShowDialog(owner: null);
            },
            testDriverAsync);
    }
}
