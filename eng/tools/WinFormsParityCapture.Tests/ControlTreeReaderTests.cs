using AwesomeAssertions;
using GitExtensions.ParityCapture;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[Category("P0_1")]
public sealed class ControlTreeReaderTests
{
    [Test]
    public void ReadPrimary_should_preserve_field_name_and_resolve_colors()
    {
        using TestForm form = new();
        form.CreateControl();
        form.TestButton.CreateControl();
        ControlTreeReader reader = new(form, dpi: 96);

        CaptureSurface surface = reader.ReadPrimary(form, new Rectangle(10, 20, 300, 200));

        CaptureNode button = FindNode(surface.Root, "_btnAction");
        button.FieldName.Should().Be("_btnAction");
        button.Colors.Foreground.Should().MatchRegex("^#[0-9A-F]{8}$");
        button.Colors.Background.Should().MatchRegex("^#[0-9A-F]{8}$");
    }

    [Test]
    public void ReadPrimary_should_record_pixels_and_dips()
    {
        using TestForm form = new();
        form.CreateControl();
        ControlTreeReader reader = new(form, dpi: 192);

        CaptureSurface surface = reader.ReadPrimary(form, new Rectangle(0, 0, 300, 200));

        surface.Root.BoundsDip.Width.Should().Be(surface.Root.BoundsPx.Width / 2m);
        surface.Root.BoundsDip.Height.Should().Be(surface.Root.BoundsPx.Height / 2m);
    }

    [Test]
    [Category("P1_7")]
    public void ReadPrimary_should_emit_framework_neutral_resolved_color_roles()
    {
        using TestForm form = new();
        form.CreateControl();
        ControlTreeReader reader = new(form, dpi: 96);

        CaptureSurface surface = reader.ReadPrimary(form, new Rectangle(0, 0, 300, 200));
        IReadOnlyDictionary<string, string> roles = surface.Root.Colors.Additional
            .Where(pair => pair.Key.StartsWith("semantic.", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

        roles.Should().HaveCount(18);
        roles.Keys.Should().Contain(
            "semantic.app.panel.background",
            "semantic.app.selection.background",
            "semantic.system.control.background",
            "semantic.system.highlight.background",
            "semantic.system.inactiveSelection.background",
            "semantic.system.tooltip.background",
            "semantic.app.reset.hard.background");
        roles.Values.Should().OnlyContain(color => System.Text.RegularExpressions.Regex.IsMatch(color, "^#[0-9A-F]{8}$"));
    }

    [Test]
    [Category("P1_7")]
    public void Dark_system_roles_should_use_the_resolved_WinForms_dark_palette()
    {
        ControlTreeReader.TestAccessor.ResolveSystemColor(KnownColor.Control, isDark: true)
            .Should().Be(Color.FromArgb(32, 32, 32));
        ControlTreeReader.TestAccessor.ResolveSystemColor(KnownColor.WindowText, isDark: true)
            .Should().Be(Color.FromArgb(240, 240, 240));
        ControlTreeReader.TestAccessor.ResolveSystemColor(KnownColor.Info, isDark: true)
            .Should().Be(Color.FromArgb(80, 80, 60));
    }

    private static CaptureNode FindNode(CaptureNode root, string fieldName)
    {
        if (root.FieldName == fieldName)
        {
            return root;
        }

        foreach (CaptureNode child in root.Children)
        {
            CaptureNode? match = FindNodeOrDefault(child, fieldName);
            if (match is not null)
            {
                return match;
            }
        }

        throw new InvalidOperationException($"Node '{fieldName}' was not found.");
    }

    private static CaptureNode? FindNodeOrDefault(CaptureNode root, string fieldName)
    {
        if (root.FieldName == fieldName)
        {
            return root;
        }

        return root.Children.Select(child => FindNodeOrDefault(child, fieldName)).FirstOrDefault(match => match is not null);
    }

    private sealed class TestForm : Form
    {
        private readonly Button _btnAction = new()
        {
            Name = "btnAction",
            Text = "Action",
            ForeColor = Color.FromArgb(255, 1, 2, 3),
            BackColor = Color.FromArgb(255, 4, 5, 6)
        };

        public TestForm()
        {
            Controls.Add(_btnAction);
            ClientSize = new Size(300, 200);
        }

        public Button TestButton => _btnAction;
    }
}
