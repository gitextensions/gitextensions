using GitUI.CommandsDialogs.SettingsDialog.Toolbars;

namespace GitUITests.CommandsDialogs.SettingsDialog.Toolbars;

public class ToolbarItemNamesTests
{
    private const char ControlCharacter = (char)1;

    [TestCase("Commit")]
    [TestCase("a b c")]
    [TestCase("héllo wörld")]
    [TestCase("100% done")]
    [TestCase("under_score")]
    [TestCase("")]
    public void Label_should_round_trip_its_text(string text)
    {
        string name = ToolbarItemNames.Label(text, order: 3);

        ToolbarItemNames.TryParseLabel(name, out string? parsed).Should().BeTrue();
        parsed.Should().Be(text);
    }

    [Test]
    public void Label_should_truncate_a_text_longer_than_the_supported_length()
    {
        // Producing a name the validator would then have to drop would lose the label entirely,
        // so the text is bounded here instead.
        string name = ToolbarItemNames.Label(new string('a', ToolbarItemNames.MaxLabelTextLength + 10), order: 0);

        ToolbarItemNames.TryParseLabel(name, out string? parsed).Should().BeTrue();
        parsed!.Length.Should().Be(ToolbarItemNames.MaxLabelTextLength);
    }

    [Test]
    public void Label_should_round_trip_a_text_containing_the_order_separator()
    {
        // "_" is unreserved, so it is not escaped and the order suffix has to be found from the
        // last underscore rather than the first.
        string name = ToolbarItemNames.Label("a_1", order: 7);

        ToolbarItemNames.TryParseLabel(name, out string? parsed).Should().BeTrue();
        parsed.Should().Be("a_1");
    }

    [TestCase("_LABEL_abc", TestName = "no order suffix")]
    [TestCase("_LABEL_abc_", TestName = "empty order suffix")]
    [TestCase("_LABEL_abc_x", TestName = "non-numeric order suffix")]
    [TestCase("_LABEL_a b_0", TestName = "unescaped space in the text")]
    [TestCase("_LABEL_a%zz_0", TestName = "malformed percent escape")]
    [TestCase("_LABEL_abc_12345678901", TestName = "absurd order suffix")]
    public void TryParseLabel_should_reject_a_malformed_name(string name)
    {
        ToolbarItemNames.TryParseLabel(name, out string? parsed).Should().BeFalse();
        parsed.Should().BeNull();
        ToolbarItemNames.IsValid(name).Should().BeFalse();
    }

    [Test]
    public void TryParseLabel_should_reject_a_text_hiding_control_characters()
    {
        // Percent-escaping lets a control character through a plain inspection of the name, so the
        // decoded text is what has to be checked.
        string name = $"{ToolbarItemNames.LabelPrefix}{Uri.EscapeDataString($"a{ControlCharacter}b")}_0";

        ToolbarItemNames.TryParseLabel(name, out _).Should().BeFalse();
        ToolbarItemNames.IsValid(name).Should().BeFalse();
    }

    [Test]
    public void TryParseLabel_should_reject_a_text_longer_than_the_supported_length()
    {
        string name = $"{ToolbarItemNames.LabelPrefix}{new string('a', ToolbarItemNames.MaxLabelTextLength + 1)}_0";

        ToolbarItemNames.TryParseLabel(name, out _).Should().BeFalse();
        ToolbarItemNames.IsValid(name).Should().BeFalse();
    }

    [TestCase("toolStripButtonPush")]
    [TestCase("btn_navigateToolStripMenuItem")]
    [TestCase("pull_shortcut_fetchToolStripMenuItem")]
    [TestCase("_NO_TRANSLATE_WorkingDir")]
    [TestCase("_viewPullRequestsToolStripMenuItem")]
    [TestCase("toolStripSeparator1")]
    [TestCase("_SEPARATOR_0")]
    [TestCase("_SPACER_12")]
    [TestCase("_LABEL_Commit_2")]
    public void IsValid_should_accept_a_name_this_application_writes(string name)
    {
        ToolbarItemNames.IsValid(name).Should().BeTrue();
    }

    [TestCase(null, TestName = "null")]
    [TestCase("", TestName = "empty")]
    [TestCase("   ", TestName = "blank")]
    [TestCase("with space", TestName = "space")]
    [TestCase("_SEPARATOR_", TestName = "separator without index")]
    [TestCase("_SEPARATOR_x", TestName = "separator with a non-numeric index")]
    [TestCase("_SPACER_1x", TestName = "spacer with a trailing character")]
    [TestCase("1leadingDigit", TestName = "leading digit")]
    public void IsValid_should_reject_a_name_this_application_could_not_write(string? name)
    {
        ToolbarItemNames.IsValid(name).Should().BeFalse();
    }

    [Test]
    public void IsValid_should_reject_a_name_carrying_a_control_character()
    {
        ToolbarItemNames.IsValid($"with{ControlCharacter}control").Should().BeFalse();
    }

    [Test]
    public void IsValid_should_reject_an_absurdly_long_name()
    {
        ToolbarItemNames.IsValid(new string('a', 2049)).Should().BeFalse();
    }

    [TestCase("_SEPARATOR_0", true)]
    [TestCase("_SPACER_0", true)]
    [TestCase("_LABEL_a_0", true)]
    [TestCase("_viewPullRequestsToolStripMenuItem", false)]
    [TestCase("toolStripButtonPush", false)]
    public void IsPlaceholder_should_not_treat_a_leading_underscore_as_a_placeholder(string name, bool expected)
    {
        // Several real menu items are named with a leading underscore; taking them for placeholders
        // would drop them from a rebuilt toolbar.
        ToolbarItemNames.IsPlaceholder(name).Should().Be(expected);
    }

    [Test]
    public void Separator_and_Spacer_should_produce_valid_names()
    {
        ToolbarItemNames.IsValid(ToolbarItemNames.Separator(0)).Should().BeTrue();
        ToolbarItemNames.IsValid(ToolbarItemNames.Spacer(41)).Should().BeTrue();
    }
}
