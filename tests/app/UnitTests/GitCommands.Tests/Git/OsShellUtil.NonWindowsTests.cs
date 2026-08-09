using GitCommands;
using GitExtensions.Shims.WinForms;

namespace GitCommandsTests.Git;

[TestFixture]
public sealed class OsShellUtilNonWindowsTests
{
    private RecordingOsShell _osShell = null!;
    private RecordingMessageBoxHost _messageBox = null!;

    [SetUp]
    public void SetUp()
    {
        _osShell = new RecordingOsShell();
        _messageBox = new RecordingMessageBoxHost();
        ShimHost.OsShell = _osShell;
        ShimHost.MessageBoxHost = _messageBox;
    }

    [TestCase(nameof(OsShellUtil.Open), "/tmp/file.txt", OsShellLaunchKind.Open)]
    [TestCase(nameof(OsShellUtil.OpenAs), "/tmp/file.txt", OsShellLaunchKind.OpenAs)]
    [TestCase(nameof(OsShellUtil.OpenWithFileExplorer), "/tmp/folder", OsShellLaunchKind.OpenDirectory)]
    [TestCase(nameof(OsShellUtil.SelectPathInFileExplorer), "/tmp/folder/file.txt", OsShellLaunchKind.ShowInDirectory)]
    [TestCase(nameof(OsShellUtil.OpenUrlInDefaultBrowser), "https://example.com", OsShellLaunchKind.OpenUri)]
    public void Shell_methods_route_to_installed_service(string method, string target, OsShellLaunchKind expectedKind)
    {
        Invoke(method, target);

        _osShell.Requests.Should().BeEquivalentTo([(target, expectedKind)]);
        _messageBox.Messages.Should().BeEmpty();
    }

    [Test]
    public void OpenUrlInDefaultBrowser_ignores_blank_values()
    {
        OsShellUtil.OpenUrlInDefaultBrowser("   ");

        _osShell.Requests.Should().BeEmpty();
    }

    [Test]
    public void Failed_launch_reports_portal_unavailability()
    {
        _osShell.Result = false;

        OsShellUtil.Open("/tmp/file.txt");

        _messageBox.Messages.Should().ContainSingle()
            .Which.Should().Contain("XDG desktop portal");
    }

    private static void Invoke(string method, string target)
    {
        switch (method)
        {
            case nameof(OsShellUtil.Open):
                OsShellUtil.Open(target);
                break;
            case nameof(OsShellUtil.OpenAs):
                OsShellUtil.OpenAs(target);
                break;
            case nameof(OsShellUtil.OpenWithFileExplorer):
                OsShellUtil.OpenWithFileExplorer(target);
                break;
            case nameof(OsShellUtil.SelectPathInFileExplorer):
                OsShellUtil.SelectPathInFileExplorer(target);
                break;
            case nameof(OsShellUtil.OpenUrlInDefaultBrowser):
                OsShellUtil.OpenUrlInDefaultBrowser(target);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(method));
        }
    }

    private sealed class RecordingOsShell : IOsShell
    {
        public List<(string Target, OsShellLaunchKind Kind)> Requests { get; } = [];

        public bool Result { get; set; } = true;

        public bool TryLaunch(string target, OsShellLaunchKind kind)
        {
            Requests.Add((target, kind));
            return Result;
        }
    }

    private sealed class RecordingMessageBoxHost : IMessageBoxHost
    {
        public List<string> Messages { get; } = [];

        public DialogResult Show(
            IWin32Window? owner,
            string? text,
            string? caption,
            MessageBoxButtons buttons,
            MessageBoxIcon icon,
            MessageBoxDefaultButton defaultButton)
        {
            Messages.Add(text ?? string.Empty);
            return DialogResult.OK;
        }
    }
}
