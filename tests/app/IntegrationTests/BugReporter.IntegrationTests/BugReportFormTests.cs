using BugReporter;
using BugReporter.Serialization;

namespace GitExtensions.UITests.NBugReports;

[Apartment(ApartmentState.STA)]
[TestFixture]
public class BugReportFormTests
{
    private BugReportForm _form;

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
                ClassicAssert.AreEqual(exception.Message, form.GetTestAccessor().ExceptionMessageTextBox.Text);
                ClassicAssert.AreEqual(exception.Type, form.GetTestAccessor().ExceptionTextBox.Text);
                ClassicAssert.AreEqual(exception.TargetSite, form.GetTestAccessor().TargetSiteTextBox.Text);

                ListView listView = form.GetTestAccessor().ExceptionDetails.GetTestAccessor().ExceptionDetailsListView;
                ClassicAssert.AreEqual(10, listView.Items.Count);

                int index = 0;
                ClassicAssert.AreEqual("Exception", listView.Items[index].Text);
                ClassicAssert.AreEqual(exception.Type, listView.Items[index].SubItems[1].Text);
                index++;
                ClassicAssert.AreEqual("Message", listView.Items[index].Text);
                ClassicAssert.AreEqual(exception.Message, listView.Items[index].SubItems[1].Text);
                index++;
                ClassicAssert.AreEqual("Target Site", listView.Items[index].Text);
                ClassicAssert.AreEqual(exception.TargetSite, listView.Items[index].SubItems[1].Text);
                index++;
                ClassicAssert.AreEqual("Inner Exception", listView.Items[index].Text);
                ClassicAssert.AreEqual(exception.InnerException.Type, listView.Items[index].SubItems[1].Text);
                index++;
                ClassicAssert.AreEqual("Source", listView.Items[index].Text);
                ClassicAssert.AreEqual(exception.Source, listView.Items[index].SubItems[1].Text);
                index++;
                ClassicAssert.AreEqual("Stack Trace", listView.Items[index].Text);
                ClassicAssert.AreEqual(exception.StackTrace, listView.Items[index].SubItems[1].Text);

                foreach (KeyValuePair<string, object> info in exception.ExtendedInformation)
                {
                    index++;
                    ClassicAssert.AreEqual(info.Key, listView.Items[index].Text);
                    ClassicAssert.AreEqual(info.Value.ToString(), listView.Items[index].SubItems[1].Text);
                }

                // Count=1-based, index=0-based
                ClassicAssert.AreEqual(listView.Items.Count - 1, index);
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
                ClassicAssert.AreEqual(expected, _form.ShowDialog(owner: null, exception, exceptionInfo, environmentInfo, canIgnore: true, showIgnore: false, focusDetails: false));
            },
            testDriverAsync);
    }
}
