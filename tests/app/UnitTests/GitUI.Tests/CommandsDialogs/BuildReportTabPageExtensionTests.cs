using System.Reflection;
using GitCommands.Settings;
using GitExtensions.Extensibility.BuildServerIntegration;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitUI.CommandsDialogs;
using GitUIPluginInterfaces;
using NSubstitute;

namespace GitUITests.CommandsDialogs;

[Apartment(ApartmentState.STA)]
public class BuildReportTabPageExtensionTests
{
    private const string _buildReportUrl = "about:blank";

    private TabControl _tabControl;
    private BuildReportTabPageExtension _extension;

    [SetUp]
    public void Setup()
    {
        SettingsSource settings = Substitute.For<SettingsSource>();
        settings.GetValue(BuildServerSettings.ShowBuildResultPage.Name).Returns("true");

        IGitModule module = Substitute.For<IGitModule>();
        module.GetEffectiveSettings().Returns(settings);

        _tabControl = new TabControl { ImageList = new ImageList() };
        _extension = new BuildReportTabPageExtension(() => module, _tabControl, "Build Report");
    }

    [TearDown]
    public void TearDown()
    {
        BuildReportTabPageExtension.TestAccessor accessor = _extension.GetTestAccessor();
        accessor.BuildReportWebBrowser?.Dispose();
        accessor.BuildReportTabPage?.Dispose();
        _tabControl.ImageList?.Dispose();
        _tabControl.Dispose();
    }

    [Test]
    public void FillBuildReport_should_not_create_web_browser_if_report_is_only_linked()
    {
        for (int i = 0; i < 3; ++i)
        {
            _extension.FillBuildReport(CreateRevision(showInBuildReportTab: false));
        }

        BuildReportTabPageExtension.TestAccessor accessor = _extension.GetTestAccessor();
        accessor.BuildReportTabPage.Should().NotBeNull();
        _tabControl.TabPages.Contains(accessor.BuildReportTabPage).Should().BeTrue();
        accessor.BuildReportWebBrowser.Should().BeNull();
        _extension.Control.Should().BeOfType<LinkLabel>();
    }

    [Test]
    public void FillBuildReport_should_remove_tab_page_without_web_browser_if_revision_has_no_build_report()
    {
        _extension.FillBuildReport(CreateRevision(showInBuildReportTab: false));

        _extension.FillBuildReport(new GitRevision(ObjectId.Random()));

        BuildReportTabPageExtension.TestAccessor accessor = _extension.GetTestAccessor();
        accessor.BuildReportTabPage.Should().NotBeNull();
        _tabControl.TabPages.Contains(accessor.BuildReportTabPage).Should().BeFalse();
        accessor.BuildReportWebBrowser.Should().BeNull();
    }

    [Test]
    public void FillBuildReport_should_create_web_browser_if_report_is_shown_in_tab()
    {
        _extension.FillBuildReport(CreateRevision(showInBuildReportTab: false));

        _extension.FillBuildReport(CreateRevision(showInBuildReportTab: true));

        BuildReportTabPageExtension.TestAccessor accessor = _extension.GetTestAccessor();
        accessor.BuildReportWebBrowser.Should().NotBeNull();
        _extension.Control.Should().BeSameAs(accessor.BuildReportWebBrowser);
        accessor.BuildReportTabPage!.Controls.Contains(accessor.BuildReportWebBrowser).Should().BeTrue();
    }

    [Test]
    public void FillBuildReport_should_subscribe_to_Navigated_only_once_until_navigation_completes()
    {
        for (int i = 0; i < 3; ++i)
        {
            _extension.FillBuildReport(CreateRevision(showInBuildReportTab: true));
            _extension.FillBuildReport(CreateRevision(showInBuildReportTab: false));
        }

        WebBrowser? webBrowser = _extension.GetTestAccessor().BuildReportWebBrowser;
        webBrowser.Should().NotBeNull();
        GetNavigatedHandlerCount(webBrowser!).Should().Be(1);
    }

    private static GitRevision CreateRevision(bool showInBuildReportTab)
        => new(ObjectId.Random())
        {
            BuildStatus = new BuildInfo
            {
                Url = _buildReportUrl,
                ShowInBuildReportTab = showInBuildReportTab
            }
        };

    private static int GetNavigatedHandlerCount(WebBrowser webBrowser)
    {
        FieldInfo? field = typeof(WebBrowser).GetField(nameof(WebBrowser.Navigated), BindingFlags.Instance | BindingFlags.NonPublic);
        field.Should().NotBeNull($"{nameof(WebBrowser)}.{nameof(WebBrowser.Navigated)} is expected to be a field-like event");

        return (field!.GetValue(webBrowser) as Delegate)?.GetInvocationList().Length ?? 0;
    }
}
