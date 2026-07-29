using AwesomeAssertions;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Category("P0_1")]
public sealed class CapturePlanTests
{
    [Test]
    public void Load_should_define_acceptance_matrix()
    {
        string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "capture-plan.json");

        CapturePlan plan = CapturePlan.Load(path);

        plan.Scales.Should().Equal(100, 125, 150, 200);
        plan.Themes.Select(theme => theme.Id).Should().Equal("light", "dark", "parity-custom");
        plan.Components.Select(component => component.TypeName).Should().Equal(
            "GitUI.CommandsDialogs.FormBrowse",
            "GitUI.CommandsDialogs.FormCommit",
            "GitUI.CommandsDialogs.FormSettings");
    }

    [Test]
    public void Load_should_include_interaction_states()
    {
        string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "capture-plan.json");

        CapturePlan plan = CapturePlan.Load(path);

        CaptureStateKind[] actual = plan.Components.SelectMany(component => component.States).Select(state => state.Kind).ToArray();
        CaptureStateKind[] expected =
        [
            CaptureStateKind.Focus,
            CaptureStateKind.Disabled,
            CaptureStateKind.Checked,
            CaptureStateKind.Expanded,
            CaptureStateKind.Hover,
            CaptureStateKind.Pressed,
            CaptureStateKind.MenuOpen
        ];

        foreach (CaptureStateKind state in expected)
        {
            actual.Should().Contain(state);
        }
    }
}
