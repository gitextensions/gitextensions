using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.SettingsDialog;
using Microsoft.VisualStudio.Threading;

namespace GitExtensionsTests;

[TestFixture]
public sealed class SettingsLayerRestorationTests
{
    [AvaloniaTest]
    [TestCase("LocalRB", false)]
    [TestCase("GlobalRB", false)]
    [TestCase("EffectiveRB", true)]
    public void Settings_header_should_restore_the_layer_for_its_page_type(string layer, bool readOnly)
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        SettingsPageHeader first = new(new LayerPage(), canSaveInsideRepo: true);
        first.FindControl<RadioButton>(layer)!.IsChecked = true;

        SettingsPageHeader reopened = new(new LayerPage(), canSaveInsideRepo: true);
        reopened.FindControl<RadioButton>(layer)!.IsChecked.Should().BeTrue();
        reopened.ReadOnly.Should().Be(readOnly);

        SettingsPageHeader otherType = new(new OtherLayerPage(), canSaveInsideRepo: true);
        otherType.GetTestAccessor().Effective.IsChecked.Should().BeTrue();

        SettingsPageHeader outsideRepository = new(new LayerPage(), canSaveInsideRepo: false);
        outsideRepository.GetTestAccessor().Global.IsChecked.Should().BeTrue();
        outsideRepository.GetTestAccessor().Effective.IsVisible.Should().BeFalse();
    }

    private class LayerPage() : SettingsPageWithHeader(EmptyServiceProvider.Instance), ILocalSettingsPage
    {
        public void SetLocalSettings()
        {
        }

        public void SetEffectiveSettings()
        {
        }
    }

    private sealed class OtherLayerPage : LayerPage;
}
