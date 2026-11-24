using System.Windows.Forms;
using GitUI.UserControls;

namespace GitUITests.UserControls;

public class TemporaryFileLabelsTests
{
    [Test]
    public void TryParseHotkey_should_map_digit_keys_to_labels()
    {
        TemporaryFileLabels.TryParseHotkey(Keys.D1, out int label1).Should().BeTrue();
        label1.Should().Be(1);
        TemporaryFileLabels.TryParseHotkey(Keys.D9, out int label9).Should().BeTrue();
        label9.Should().Be(9);
        TemporaryFileLabels.TryParseHotkey(Keys.NumPad1, out int numPadLabel1).Should().BeTrue();
        numPadLabel1.Should().Be(1);
        TemporaryFileLabels.TryParseHotkey(Keys.NumPad9, out int numPadLabel9).Should().BeTrue();
        numPadLabel9.Should().Be(9);
    }

    [Test]
    public void TryParseHotkey_should_map_zero_to_clearing()
    {
        TemporaryFileLabels.TryParseHotkey(Keys.D0, out int label).Should().BeTrue();
        label.Should().Be(TemporaryFileLabels.NoLabel);
        TemporaryFileLabels.TryParseHotkey(Keys.NumPad0, out int numPadLabel).Should().BeTrue();
        numPadLabel.Should().Be(TemporaryFileLabels.NoLabel);
    }

    [Test]
    public void TryParseHotkey_should_ignore_other_keys()
    {
        TemporaryFileLabels.TryParseHotkey(Keys.A, out _).Should().BeFalse();
        TemporaryFileLabels.TryParseHotkey(Keys.Control | Keys.D1, out _).Should().BeFalse();
        TemporaryFileLabels.TryParseHotkey(Keys.Alt | Keys.D1, out _).Should().BeFalse();
        TemporaryFileLabels.TryParseHotkey(Keys.Shift | Keys.D1, out _).Should().BeFalse();
    }

    [Test]
    public void TryAssign_should_store_label_for_file()
    {
        TemporaryFileLabels labels = new();

        labels.TryAssign("src/file.cs", 3).Should().BeTrue();
        labels.GetLabel("src/file.cs").Should().Be(3);
        labels.Count.Should().Be(1);
    }

    [Test]
    public void TryAssign_should_replace_label_and_report_no_change_for_same_label()
    {
        TemporaryFileLabels labels = new();

        labels.TryAssign("src/file.cs", 3);
        labels.TryAssign("src/file.cs", 5).Should().BeTrue();
        labels.GetLabel("src/file.cs").Should().Be(5);
        labels.TryAssign("src/file.cs", 5).Should().BeFalse();
    }

    [Test]
    public void GetLabel_should_return_null_for_unlabelled_file()
    {
        TemporaryFileLabels labels = new();

        labels.GetLabel("src/file.cs").Should().BeNull();
    }

    [Test]
    public void Clear_should_remove_label()
    {
        TemporaryFileLabels labels = new();
        labels.TryAssign("src/file.cs", 2);

        labels.Clear("src/file.cs").Should().BeTrue();
        labels.GetLabel("src/file.cs").Should().BeNull();
        labels.Count.Should().Be(0);
        labels.Clear("src/file.cs").Should().BeFalse();
    }

    [Test]
    public void ClearAll_should_remove_all_labels()
    {
        TemporaryFileLabels labels = new();
        labels.TryAssign("src/file.cs", 2);
        labels.TryAssign("src/other.cs", 7);

        labels.ClearAll();

        labels.Count.Should().Be(0);
        labels.GetLabel("src/file.cs").Should().BeNull();
        labels.GetLabel("src/other.cs").Should().BeNull();
    }

    [Test]
    public void Format_should_prefix_label_number()
    {
        TemporaryFileLabels.Format(1).Should().Be("[1] ");
        TemporaryFileLabels.Format(9).Should().Be("[9] ");
    }

    [Test]
    public void GetColor_should_return_distinct_colors_for_all_labels()
    {
        HashSet<System.Drawing.Color> colors = [];

        for (int label = 1; label <= TemporaryFileLabels.MaxLabel; label++)
        {
            colors.Add(TemporaryFileLabels.GetColor(label)).Should().BeTrue();
        }

        colors.Should().HaveCount(TemporaryFileLabels.MaxLabel);
    }
}
