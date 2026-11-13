using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using GitCommands;
using GitUI;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class FormFixHomeDiscoveryTests
{
    private AppSettingsTestScope _settings = null!;
    private IEnvironmentAbstraction _previousEnvironment = null!;
    private IEnvironmentAbstraction _environment = null!;
    private WinFormsShims.IMessageBoxHost? _previousMessageBoxes;
    private readonly List<Message> _messages = [];
    private Dictionary<string, string?> _previousProcessEnvironment = null!;
    private string _directory = null!;
    private string _currentHome = null!;
    private string[] _candidates = null!;

    [SetUp]
    public void SetUp()
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        _settings = new AppSettingsTestScope();
        _previousProcessEnvironment = new[] { "PATH", "HOME", "USERPROFILE", "TERM", "DISPLAY", "SSH_ASKPASS", "SSH_ASKPASS_REQUIRE" }
            .ToDictionary(name => name, Environment.GetEnvironmentVariable);
        _directory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.HomeDiscovery-{Guid.NewGuid():N}");
        _currentHome = Path.Combine(_directory, "current");
        Directory.CreateDirectory(_currentHome);
        _candidates = [.. new[] { "user", "drive", "profile", "personal" }.Select(name => Path.Combine(_directory, name))];
        _previousEnvironment = FormFixHome.TestAccessor.EnvironmentAbstraction;
        _environment = Substitute.For<IEnvironmentAbstraction>();
        _environment.GetEnvironmentVariable("HOME").Returns(_currentHome);
        _environment.GetEnvironmentVariable("HOME", EnvironmentVariableTarget.User).Returns(_candidates[0]);
        _environment.GetEnvironmentVariable("HOMEDRIVE").Returns(string.Empty);
        _environment.GetEnvironmentVariable("HOMEPATH").Returns(_candidates[1]);
        _environment.GetEnvironmentVariable("USERPROFILE").Returns(_candidates[2]);
        _environment.GetFolderPath(Environment.SpecialFolder.Personal).Returns(_candidates[3]);
        FormFixHome.TestAccessor.EnvironmentAbstraction = _environment;
        try
        {
            _previousMessageBoxes = WinFormsShims.ShimHost.MessageBoxHost;
        }
        catch (InvalidOperationException)
        {
            _previousMessageBoxes = null;
        }

        _messages.Clear();
        WinFormsShims.IMessageBoxHost messages = Substitute.For<WinFormsShims.IMessageBoxHost>();
        messages.Show(
                Arg.Any<WinFormsShims.IWin32Window?>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<WinFormsShims.MessageBoxButtons>(), Arg.Any<WinFormsShims.MessageBoxIcon>(),
                Arg.Any<WinFormsShims.MessageBoxDefaultButton>())
            .Returns(call =>
            {
                WinFormsShims.MessageBoxButtons buttons = call.ArgAt<WinFormsShims.MessageBoxButtons>(3);
                _messages.Add(new Message(call.ArgAt<string?>(1), buttons, call.ArgAt<WinFormsShims.MessageBoxIcon>(4)));
                return buttons == WinFormsShims.MessageBoxButtons.YesNo ? WinFormsShims.DialogResult.No : WinFormsShims.DialogResult.OK;
            });
        WinFormsShims.ShimHost.MessageBoxHost = messages;
    }

    [TearDown]
    public void TearDown()
    {
        FormFixHome.TestAccessor.EnvironmentAbstraction = _previousEnvironment;
        WinFormsShims.ShimHost.MessageBoxHost = _previousMessageBoxes!;
        foreach ((string name, string? value) in _previousProcessEnvironment)
        {
            Environment.SetEnvironmentVariable(name, value);
        }

        _settings.Dispose();
        TestDirectory.Delete(_directory);
    }

    [AvaloniaTest]
    public void CheckHomePath_should_not_prompt_without_a_better_candidate()
    {
        FormFixHome.CheckHomePath();

        _messages.Should().BeEmpty();
    }

    [AvaloniaTest]
    public void CheckHomePath_should_not_prompt_when_current_home_has_config()
    {
        CreateConfig(_currentHome);

        FormFixHome.CheckHomePath();

        _messages.Should().BeEmpty();
    }

    [AvaloniaTest]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("missing")]
    public void CheckHomePath_should_offer_manual_repair_for_an_unset_or_inaccessible_home(string? value)
    {
        string? home = value == "missing" ? Path.Combine(_directory, "missing") : value;
        _environment.GetEnvironmentVariable("HOME").Returns(home);

        FormFixHome.CheckHomePath();

        _messages.Should().ContainSingle().Which.Buttons.Should().Be(WinFormsShims.MessageBoxButtons.YesNo);
    }

    [AvaloniaTest]
    public void CheckHomePath_should_offer_manual_repair_when_candidate_discovery_is_denied()
    {
        _environment.GetEnvironmentVariable("HOME", EnvironmentVariableTarget.User).Returns(_ => throw new System.Security.SecurityException());

        FormFixHome.CheckHomePath();

        _messages.Should().ContainSingle().Which.Buttons.Should().Be(WinFormsShims.MessageBoxButtons.YesNo);
    }

    [AvaloniaTest]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void CheckHomePath_should_offer_repair_for_a_better_candidate(int candidate)
    {
        CreateConfig(_candidates[candidate]);

        FormFixHome.CheckHomePath();

        Message message = _messages.Should().ContainSingle().Which;
        message.Buttons.Should().Be(WinFormsShims.MessageBoxButtons.YesNo);
        message.Text.Should().Contain(_currentHome);
    }

    [AvaloniaTest]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void Opening_should_discover_and_select_a_readable_home(int candidate)
    {
        CreateConfig(_candidates[candidate]);
        AppSettings.CustomHomeDir = Path.Combine(_directory, "previous");
        using FormFixHome form = new();
        try
        {
            form.Show();

            AssertDiscovered(form, candidate);
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void Opening_should_prefer_user_home_over_later_readable_candidates()
    {
        foreach (string candidate in _candidates)
        {
            CreateConfig(candidate);
        }

        using FormFixHome form = new();
        try
        {
            form.Show();

            AssertDiscovered(form, 0);
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void LoadSettings_should_continue_after_a_candidate_read_fails(int candidate)
    {
        CreateConfig(_candidates[3]);
        switch (candidate)
        {
            case 0:
                _environment.GetEnvironmentVariable("HOME", EnvironmentVariableTarget.User).Returns(_ => throw new System.Security.SecurityException());
                break;
            case 1:
                _environment.GetEnvironmentVariable("HOMEDRIVE").Returns(_ => throw new System.Security.SecurityException());
                break;
            case 2:
                _environment.GetEnvironmentVariable("USERPROFILE").Returns(_ => throw new System.Security.SecurityException());
                break;
        }

        using FormFixHome form = new();
        form.GetTestAccessor().LoadSettings();

        AssertDiscovered(form, 3);
    }

    [AvaloniaTest]
    public void LoadSettings_should_preserve_the_configured_choice_when_all_candidate_reads_fail()
    {
        string configuredHome = Path.Combine(_directory, "configured");
        AppSettings.CustomHomeDir = configuredHome;
        _environment.GetEnvironmentVariable("HOME", EnvironmentVariableTarget.User).Returns(_ => throw new System.Security.SecurityException());
        _environment.GetEnvironmentVariable("HOMEDRIVE").Returns(_ => throw new System.Security.SecurityException());
        _environment.GetEnvironmentVariable("USERPROFILE").Returns(_ => throw new System.Security.SecurityException());
        _environment.GetFolderPath(Environment.SpecialFolder.Personal).Returns(_ => throw new System.Security.SecurityException());
        using FormFixHome form = new();

        form.GetTestAccessor().LoadSettings();

        form.GetTestAccessor().OtherHome.IsChecked.Should().BeTrue();
        form.GetTestAccessor().OtherHomeDirectory.Text.Should().Be(configuredHome);
        _messages.Should().BeEmpty();
    }

    [AvaloniaTest]
    public void Accept_should_apply_the_profile_choice_supplied_by_discovery()
    {
        CreateConfig(_candidates[2]);
        Environment.SetEnvironmentVariable("USERPROFILE", _candidates[2]);
        using FormFixHome form = new();
        try
        {
            form.Show();
            AssertDiscovered(form, 2);

            form.FindControl<Button>("ok")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            AppSettings.CustomHomeDir.Should().BeEmpty();
            AppSettings.UserProfileHomeDir.Should().BeTrue();
            EnvironmentConfiguration.GetHomeDir().Should().Be(_candidates[2]);
            form.IsVisible.Should().BeFalse();
        }
        finally
        {
            form.Close();
        }
    }

    private static void CreateConfig(string path)
    {
        Directory.CreateDirectory(path);
        File.WriteAllText(Path.Join(path, ".gitconfig"), "[user]\nname = Home Discovery Test\n");
    }

    private void AssertDiscovered(FormFixHome form, int candidate)
    {
        Message message = _messages.Should().ContainSingle().Which;
        message.Icon.Should().Be(WinFormsShims.MessageBoxIcon.Information);
        message.Text.Should().Contain(_candidates[candidate]);
        FormFixHome.TestAccessor controls = form.GetTestAccessor();
        controls.DefaultHome.IsChecked.Should().Be(candidate is 0 or 1);
        controls.UserProfileHome.IsChecked.Should().Be(candidate == 2);
        controls.OtherHome.IsChecked.Should().Be(candidate == 3);
        if (candidate == 2)
        {
            controls.UserProfileHome.IsVisible.Should().BeTrue();
        }

        if (candidate == 3)
        {
            controls.OtherHomeDirectory.Text.Should().Be(_candidates[3]);
        }
    }

    private sealed record Message(string? Text, WinFormsShims.MessageBoxButtons Buttons, WinFormsShims.MessageBoxIcon Icon);
}
