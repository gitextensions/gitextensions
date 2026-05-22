using BugReporter;
using BugReporter.Serialization;

namespace GitExtensions.UITests.NBugReports;

[Apartment(ApartmentState.STA)]
public class BugReportFormTests
{
    private BugReportForm _form = null!;

    [SetUp]
    public void Setup()
    {
        _form = new BugReportForm();
    }

    [TearDown]
    public void TearDown()
    {
        _form.Dispose();
    }

    [Test]
    public void Should_show_load_exception_info_correctly()
    {
        string path = Path.Combine(TestContext.CurrentContext.TestDirectory, @"MockData/SimpleException.txt");
        string content = File.ReadAllText(path);

        SerializableException exception = SerializableException.FromXmlString(content);

        RunFormTest(
            form =>
            {
                form.GetTestAccessor().ExceptionMessageTextBox.Text.Should().Be(exception.Message);
                form.GetTestAccessor().ExceptionTextBox.Text.Should().Be(exception.Type);
                form.GetTestAccessor().TargetSiteTextBox.Text.Should().Be(exception.TargetSite);

                ListView listView = form.GetTestAccessor().ExceptionDetails.GetTestAccessor().ExceptionDetailsListView;
                listView.Items.Count.Should().Be(10);

                int index = 0;
                listView.Items[index].Text.Should().Be("Exception");
                listView.Items[index].SubItems[1].Text.Should().Be(exception.Type);
                index++;
                listView.Items[index].Text.Should().Be("Message");
                listView.Items[index].SubItems[1].Text.Should().Be(exception.Message);
                index++;
                listView.Items[index].Text.Should().Be("Target Site");
                listView.Items[index].SubItems[1].Text.Should().Be(exception.TargetSite);
                index++;
                listView.Items[index].Text.Should().Be("Inner Exception");
                listView.Items[index].SubItems[1].Text.Should().Be(exception.InnerException!.Type);
                index++;
                listView.Items[index].Text.Should().Be("Source");
                listView.Items[index].SubItems[1].Text.Should().Be(exception.Source);
                index++;
                listView.Items[index].Text.Should().Be("Stack Trace");
                listView.Items[index].SubItems[1].Text.Should().Be(exception.StackTrace);

                foreach (KeyValuePair<string, object> info in exception.ExtendedInformation!)
                {
                    index++;
                    listView.Items[index].Text.Should().Be(info.Key);
                    listView.Items[index].SubItems[1].Text.Should().Be(info.Value.ToString());
                }

                // Count=1-based, index=0-based
                index.Should().Be(listView.Items.Count - 1);
            },
            exception,
            exceptionInfo: "",
            environmentInfo: "");
    }

    private void RunFormTest(Action<BugReportForm> testDriver, SerializableException exception, string exceptionInfo, string environmentInfo)
    {
        RunFormTest(
            form =>
            {
                testDriver(form);
                return Task.CompletedTask;
            },
            exception, exceptionInfo, environmentInfo,
            expected: DialogResult.Cancel);
    }

    private void RunFormTest(Func<BugReportForm, Task> testDriverAsync, SerializableException exception, string exceptionInfo, string environmentInfo, DialogResult expected)
    {
        UITest.RunForm(
            () =>
            {
                _form.ShowDialog(owner: null, exception, exceptionInfo, environmentInfo, canIgnore: true, showIgnore: false, focusDetails: false).Should().Be(expected);
            },
            testDriverAsync);
    }
}
