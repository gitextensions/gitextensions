using GitCommands;
using GitUI;
using GitUI.UserControls.RevisionGrid.Columns;

namespace GitUITests.UserControls.RevisionGrid;

[SetCulture("en-US")]
[SetUICulture("en-US")]
[NonParallelizable]
public class DateColumnProviderTests
{
    private bool _originalRelativeDate;
    private bool _originalOmitSeconds;

    [SetUp]
    public void SetUp()
    {
        _originalRelativeDate = AppSettings.RelativeDate;
        _originalOmitSeconds = AppSettings.OmitSecondsInRevisionGridDate;
        AppSettings.RelativeDate = false;
        AppSettings.OmitSecondsInRevisionGridDate.Value = false;
    }

    [TearDown]
    public void TearDown()
    {
        AppSettings.RelativeDate = _originalRelativeDate;
        AppSettings.OmitSecondsInRevisionGridDate.Value = _originalOmitSeconds;
    }

    [Test]
    public void FormatDate_should_show_seconds_when_setting_is_off()
    {
        DateTime dt = new(2024, 5, 17, 14, 3, 42);

        // "G" is the culture-aware format used before the seconds-omission feature
        DateColumnProvider.FormatDate(dt).Should().Be(dt.ToString("G"));
        DateColumnProvider.FormatDate(dt).Should().Contain("42");
    }

    [Test]
    public void FormatDate_should_omit_seconds_when_setting_is_on()
    {
        AppSettings.OmitSecondsInRevisionGridDate.Value = true;
        DateTime dt = new(2024, 5, 17, 14, 3, 42);

        DateColumnProvider.FormatDate(dt).Should().Be("2024-05-17 14:03");
    }

    [Test]
    public void FormatDate_should_return_empty_for_min_value()
    {
        DateColumnProvider.FormatDate(DateTime.MinValue).Should().BeEmpty();
    }

    [Test]
    public void FormatDate_should_return_empty_for_max_value()
    {
        DateColumnProvider.FormatDate(DateTime.MaxValue).Should().BeEmpty();
    }
}
