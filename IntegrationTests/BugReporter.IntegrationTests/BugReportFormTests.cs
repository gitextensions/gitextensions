using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BugReporter;
using BugReporter.Serialization;
using NUnit.Framework;

namespace GitExtensions.UITests.NBugReports
{
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
                    Assert.AreEqual(exception.Message, form.GetTestAccessor().ExceptionMessageTextBox.Text);
                    Assert.AreEqual(exception.Type, form.GetTestAccessor().ExceptionTextBox.Text);
                    Assert.AreEqual(exception.TargetSite, form.GetTestAccessor().TargetSiteTextBox.Text);

                    ListView listView = form.GetTestAccessor().ExceptionDetails.GetTestAccessor().ExceptionDetailsListView;
                    Assert.AreEqual(10, listView.Items.Count);

                    int index = 0;
                    Assert.AreEqual("Exception", listView.Items[index].Text);
                    Assert.AreEqual(exception.Type, listView.Items[index].SubItems[1].Text);
                    index++;
                    Assert.AreEqual("Message", listView.Items[index].Text);
                    Assert.AreEqual(exception.Message, listView.Items[index].SubItems[1].Text);
                    index++;
                    Assert.AreEqual("Target Site", listView.Items[index].Text);
                    Assert.AreEqual(exception.TargetSite, listView.Items[index].SubItems[1].Text);
                    index++;
                    Assert.AreEqual("Inner Exception", listView.Items[index].Text);
                    Assert.AreEqual(exception.InnerException.Type, listView.Items[index].SubItems[1].Text);
                    index++;
                    Assert.AreEqual("Source", listView.Items[index].Text);
                    Assert.AreEqual(exception.Source, listView.Items[index].SubItems[1].Text);
                    index++;
                    Assert.AreEqual("Stack Trace", listView.Items[index].Text);
                    Assert.AreEqual(exception.StackTrace, listView.Items[index].SubItems[1].Text);

                    foreach (var info in exception.ExtendedInformation)
                    {
                        index++;
                        Assert.AreEqual(info.Key, listView.Items[index].Text);
                        Assert.AreEqual(info.Value.ToString(), listView.Items[index].SubItems[1].Text);
                    }

                    // Count=1-based, index=0-based
                    Assert.AreEqual(listView.Items.Count - 1, index);
                },
                exception,
                null);
        }

        private void RunFormTest(Action<BugReportForm> testDriver, SerializableException exception, string environmentInfo)
        {
            RunFormTest(
                form =>
                {
                    testDriver(form);
                    return Task.CompletedTask;
                },
                exception, environmentInfo,
                expected: DialogResult.Cancel);
        }

        private void RunFormTest(Func<BugReportForm, Task> testDriverAsync, SerializableException exception, string environmentInfo, DialogResult expected)
        {
            UITest.RunForm(
                () =>
                {
                    Assert.AreEqual(expected, _form.ShowDialog(owner: null, exception, environmentInfo, canIgnore: true, showIgnore: false, focusDetails: false));
                },
                testDriverAsync);
        }
    }
}
