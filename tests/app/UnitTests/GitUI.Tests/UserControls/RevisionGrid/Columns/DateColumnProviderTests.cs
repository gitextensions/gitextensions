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

    [SetUp]
    public void SetUp()
    {
        _originalRelativeDate = AppSettings.RelativeDate;
        AppSettings.RelativeDate = false;
    }

    [TearDown]
    public void TearDown()
    {
        AppSettings.RelativeDate = _originalRelativeDate;
    }

    [Test]
    public void FormatDate_should_omit_seconds()
    {
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
