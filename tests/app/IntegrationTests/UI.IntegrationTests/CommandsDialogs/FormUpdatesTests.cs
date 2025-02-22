using CommonTestUtils;
using FluentAssertions;
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
        ReferenceRepository.ResetRepo(ref _referenceRepository);
        _commands = new GitUICommands(GlobalServiceContainer.CreateDefaultMockServiceContainer(), _referenceRepository.Module);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
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

                accessor.DisplayNetRuntimeLink("Required: .NET {0} Desktop Runtime {1} or later", requiredNetRuntimeVersion: null);
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
            "Required: .NET {0} Desktop Runtime {1} or later",
            "Required: .NET 8.10 Desktop Runtime 8.10.134 or later",
            new LinkArea(36, 8));

        yield return new TestCaseData(
            new Version(10, 0, 2),
            "Требуется: .NET {0} Desktop Runtime {1} или более поздняя версия",
            "Требуется: .NET 10.0 Desktop Runtime 10.0.2 или более поздняя версия",
            new LinkArea(37, 6));

        yield return new TestCaseData(
            new Version(7, 11, 10),
            "Erforderlich: .NET {0} Desktop Runtime {1} oder höher",
            "Erforderlich: .NET 7.11 Desktop Runtime 7.11.10 oder höher",
            new LinkArea(40, 7));
    }

    [TestCaseSource(nameof(NetRuntimeLinkTestCases))]
    public void Should_NET_runtime_link_url_be_correct(Version runtimeVersion, string expectedUrl)
    {
        RunFormTest(
            form =>
            {
                FormUpdates.TestAccessor accessor = form.GetTestAccessor();
                accessor.DisplayNetRuntimeLink("Required: .NET {0} Desktop Runtime {1} or later", runtimeVersion);

                accessor.NetRuntimeDownloadUrl.Should().Be(expectedUrl);
            });
    }

    private static IEnumerable<TestCaseData> NetRuntimeLinkTestCases()
    {
        yield return new TestCaseData(
            new Version(8, 10, 134),
            "https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-desktop-8.10.134-windows-x64-installer");

        yield return new TestCaseData(
            new Version(10, 0, 2),
            "https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-desktop-10.0.2-windows-x64-installer");

        yield return new TestCaseData(
            new Version(7, 11, 10),
            "https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-desktop-7.11.10-windows-x64-installer");
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
