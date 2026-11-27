using System.ComponentModel.Design;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using NSubstitute;

namespace GitUITests.CommandsDialogs;

[Apartment(ApartmentState.STA)]
public sealed class FormRebaseTests
{
    [Test]
    public void OnShown_should_check_autosquash_when_interactive_and_setting_unset()
    {
        using FormRebase form = CreateForm(interactive: true, autosquashSetting: null);

        form.GetTestAccessor().RaiseShown();

        form.GetTestAccessor().chkAutosquash.Checked.Should().BeTrue();
    }

    [Test]
    public void OnShown_should_not_check_autosquash_when_not_interactive_and_setting_unset()
    {
        using FormRebase form = CreateForm(interactive: false, autosquashSetting: null);

        form.GetTestAccessor().RaiseShown();

        form.GetTestAccessor().chkAutosquash.Checked.Should().BeFalse();
    }

    [Test]
    public void OnShown_should_respect_explicit_false_setting()
    {
        using FormRebase form = CreateForm(interactive: true, autosquashSetting: false);

        form.GetTestAccessor().RaiseShown();

        form.GetTestAccessor().chkAutosquash.Checked.Should().BeFalse();
    }

    [Test]
    public void OnShown_should_respect_explicit_true_setting()
    {
        using FormRebase form = CreateForm(interactive: false, autosquashSetting: true);

        form.GetTestAccessor().RaiseShown();

        form.GetTestAccessor().chkAutosquash.Checked.Should().BeTrue();
    }

    [Test]
    public void chkInteractive_CheckedChanged_should_check_autosquash_when_interactive_enabled_and_setting_unset()
    {
        using FormRebase form = CreateForm(interactive: false, autosquashSetting: null);
        FormRebase.TestAccessor accessor = form.GetTestAccessor();

        accessor.chkInteractive.Checked = true;

        accessor.chkAutosquash.Checked.Should().BeTrue();
    }

    [Test]
    public void chkInteractive_CheckedChanged_should_not_check_autosquash_when_setting_explicitly_false()
    {
        using FormRebase form = CreateForm(interactive: false, autosquashSetting: false);
        FormRebase.TestAccessor accessor = form.GetTestAccessor();

        accessor.chkInteractive.Checked = true;

        accessor.chkAutosquash.Checked.Should().BeFalse();
    }

    private static FormRebase CreateForm(bool interactive, bool? autosquashSetting)
    {
        IGitVersion gitVersion = Substitute.For<IGitVersion>();
        gitVersion.SupportUpdateRefs.Returns(true);

        IGitModule gitModule = Substitute.For<IGitModule>();
        gitModule.GitVersion.Returns(gitVersion);
        gitModule.GetSelectedBranch(Arg.Any<bool>()).Returns(string.Empty);
        gitModule.GetRefs(Arg.Any<RefsFilter>()).Returns(Array.Empty<IGitRef>());
        gitModule.GetEffectiveSetting<bool>("rebase.autosquash").Returns(autosquashSetting);

        ServiceContainer serviceContainer = new();
        serviceContainer.AddService(Substitute.For<GitUI.Hotkey.IHotkeySettingsManager>());
        serviceContainer.AddService(Substitute.For<ResourceManager.IHotkeySettingsLoader>());
        serviceContainer.AddService(Substitute.For<GitUI.ScriptsEngine.IScriptsRunner>());
        GitUICommands commands = new(serviceContainer, gitModule);
        return new FormRebase(commands, from: "", to: null, defaultBranch: null, interactive: interactive, startRebaseImmediately: false);
    }
}
