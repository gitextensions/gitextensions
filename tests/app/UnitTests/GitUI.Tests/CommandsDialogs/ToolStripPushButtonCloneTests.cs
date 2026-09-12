using GitCommands;
using GitCommands.Git;
using GitUI.CommandsDialogs;

namespace GitUITests.CommandsDialogs;

// A push button placed on a custom toolbar is a ToolStripPushButtonClone rather than a plain copy,
// so that each toolbar can show or hide the "Push" label independently while both keep displaying
// the same ahead/behind counter. That text composition is what these tests pin down.
public class ToolStripPushButtonCloneTests
{
    private const string BranchName = "my-branch";

    private bool _originalShowAheadBehindData;
    private ToolStripPushButton _original = null!;

    [SetUp]
    public void Setup()
    {
        _originalShowAheadBehindData = AppSettings.ShowAheadBehindData;
        AppSettings.ShowAheadBehindData = true;

        _original = new ToolStripPushButton();
        _original.ResetToDefaultState();
    }

    [TearDown]
    public void TearDown()
    {
        _original.Dispose();
        AppSettings.ShowAheadBehindData = _originalShowAheadBehindData;
    }

    private void DisplayAheadBehind(string aheadCount = "9", string behindCount = "")
    {
        Dictionary<string, AheadBehindData> data = new()
        {
            { BranchName, new AheadBehindData { AheadCount = aheadCount, BehindCount = behindCount, Branch = BranchName } }
        };

        _original.DisplayAheadBehindInformation(data, BranchName, string.Empty);
    }

    [Test]
    public void Clone_should_show_no_text_while_idle_when_it_does_not_want_a_label()
    {
        using ToolStripPushButtonClone clone = new(_original, showLabel: false);

        clone.Text.Should().BeEmpty();
        clone.DisplayStyle.Should().Be(ToolStripItemDisplayStyle.Image);
    }

    [Test]
    public void Clone_should_show_the_label_alone_while_idle_when_it_wants_one()
    {
        using ToolStripPushButtonClone clone = new(_original, showLabel: true);

        clone.Text.Should().Be("Push");
        clone.DisplayStyle.Should().Be(ToolStripItemDisplayStyle.ImageAndText);
    }

    [Test]
    public void Clone_should_show_the_counter_alone_when_it_does_not_want_a_label()
    {
        using ToolStripPushButtonClone clone = new(_original, showLabel: false);

        DisplayAheadBehind();

        clone.Text.Should().Be(_original.Text);
        clone.Text.Should().NotBeEmpty();
        clone.DisplayStyle.Should().Be(ToolStripItemDisplayStyle.ImageAndText);
    }

    [Test]
    public void Clone_should_append_its_own_label_after_the_counter()
    {
        using ToolStripPushButtonClone clone = new(_original, showLabel: true);

        DisplayAheadBehind();

        clone.Text.Should().Be($"{_original.Text} Push");
    }

    [Test]
    public void Clone_should_strip_a_label_the_original_shows_but_it_does_not()
    {
        // This is the whole point of the class: the Standard toolbar can show "9↑ Push" while a
        // custom toolbar shows just "9↑".
        _original.ShowLabel = true;
        using ToolStripPushButtonClone clone = new(_original, showLabel: false);

        DisplayAheadBehind();

        _original.Text.Should().EndWith(" Push");
        clone.Text.Should().Be(_original.Text[..^" Push".Length]);
    }

    [Test]
    public void Clone_should_keep_its_label_when_the_original_hides_its_own()
    {
        _original.ShowLabel = false;
        using ToolStripPushButtonClone clone = new(_original, showLabel: true);

        DisplayAheadBehind();

        clone.Text.Should().Be($"{_original.Text} Push");
        _original.Text.Should().NotEndWith(" Push");
    }

    [Test]
    public void Clone_should_refresh_its_text_when_its_own_label_preference_changes()
    {
        using ToolStripPushButtonClone clone = new(_original, showLabel: false);
        DisplayAheadBehind();
        string? counter = clone.Text;

        clone.ShowLabel = true;

        clone.Text.Should().Be($"{counter} Push");
    }

    [Test]
    public void Clone_should_use_its_own_label_text()
    {
        using ToolStripPushButtonClone clone = new(_original, showLabel: true) { LabelText = "Envoyer" };

        clone.Text.Should().Be("Envoyer");
    }

    [Test]
    public void Clone_should_forward_a_click_to_the_original()
    {
        int clicks = 0;
        _original.Click += (s, e) => clicks++;
        using ToolStripPushButtonClone clone = new(_original, showLabel: false);

        clone.PerformClick();

        clicks.Should().Be(1);
    }

    [Test]
    public void Clone_should_stop_following_the_original_once_disposed()
    {
        // A clone is thrown away and rebuilt whenever the toolbars are reloaded, so a subscription
        // left on the long-lived original would pile up and keep updating a dead button.
        ToolStripPushButtonClone clone = new(_original, showLabel: true);
        clone.Dispose();

        DisplayAheadBehind();

        clone.Text.Should().Be("Push");
    }
}
