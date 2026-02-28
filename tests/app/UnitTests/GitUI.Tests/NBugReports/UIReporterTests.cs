using System.Text;
using FluentAssertions;
using GitExtensions.Extensibility;
using GitUI.NBugReports;

namespace GitUITests.NBugReports;

[TestFixture]
public sealed class UIReporterTests
{
    private const string RootError = "root error";

    [Test, TestCaseSource(typeof(UIReporterTests), nameof(GetFileNameTestCases))]
    public void GetFileName_should_extract_file_name_from_exception(Exception exception, string expectedDllName)
    {
        string result = UIReporter.TestAccessor.GetFileName(exception);
        result.Should().Be(expectedDllName);
    }

    private static IEnumerable<TestCaseData> GetFileNameTestCases
    {
        get
        {
            // DllNotFoundException
            yield return new TestCaseData(
                new DllNotFoundException("Unable to load DLL 'vcruntime140_cor3.dll' or one of its dependencies: The specified module could not be found."),
                "vcruntime140_cor3.dll");
            yield return new TestCaseData(
                new DllNotFoundException("Unable to load DLL 'user32.dll': File not found."),
                "user32.dll");
            yield return new TestCaseData(
                new DllNotFoundException("Unable to load DLL 'my.custom.dll' for some reason"),
                "my.custom.dll");
            yield return new TestCaseData(
                new DllNotFoundException("No quotes in this message"),
                "No quotes in this message");
            yield return new TestCaseData(
                new DllNotFoundException("'only_one_quote"),
                "'only_one_quote");
            yield return new TestCaseData(
                new DllNotFoundException(""),
                "");
            yield return new TestCaseData(
                new DllNotFoundException("Unable to load DLL ''"),
                "");

            // FileNotFoundException
            yield return new TestCaseData(
                new FileNotFoundException("Could not load file or assembly Microsoft.CSharp", "Microsoft.CSharp"),
                "Microsoft.CSharp");
            yield return new TestCaseData(
                new FileNotFoundException("Could not load file or assembly 'System.Data.Common, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'", fileName: "System.Data.Common, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"),
                "System.Data.Common");
            yield return new TestCaseData(
                new FileNotFoundException("Could not load file or assembly", fileName: null),
                "");
            yield return new TestCaseData(
                new FileNotFoundException("Could not load file or assembly MyLib", "MyLib"),
                "MyLib");
            yield return new TestCaseData(
                new FileNotFoundException("Could not load file or assembly 'SomeAssembly, version=1.0.0.0'", fileName: "SomeAssembly, version=1.0.0.0"),
                "SomeAssembly");
            yield return new TestCaseData(
                new FileNotFoundException("Could not load file or assembly", fileName: ""),
                "");
            yield return new TestCaseData(
                new FileNotFoundException("Assembly not found", fileName: ", version=1.0.0.0"),
                ", version=1.0.0.0");

            // Other exceptions
            yield return new TestCaseData(
                new Exception("No quotes in this message"),
                "unknown");
            yield return new TestCaseData(
                new Exception("'only_one_quote"),
                "unknown");
            yield return new TestCaseData(
                new InvalidOperationException("some error"),
                "unknown");
        }
    }

    [TestCase("System.Data.Common", true)]
    [TestCase("System.IO", true)]
    [TestCase("System.", true)]
    [TestCase("Microsoft.CSharp", true)]
    [TestCase("Microsoft.VisualBasic", true)]
    [TestCase("Microsoft.", true)]
    [TestCase("system.data.common", true)] // case-insensitive
    [TestCase("MICROSOFT.Extensions", true)] // case-insensitive
    [TestCase("System", false)]
    [TestCase("Microsoft", false)]
    [TestCase("CustomAssembly", false)]
    [TestCase("MyApp.Core", false)]
    [TestCase("", false)]
    [TestCase("   ", false)]
    [TestCase(null, false)]
    public void IsDotNetFrameworkAssembly_should_identify_framework_assemblies(string assemblyName, bool expected)
    {
        bool result = UIReporter.TestAccessor.IsDotNetFrameworkAssembly(assemblyName);
        result.Should().Be(expected);
    }

    [TestCase("vcruntime140_cor3.dll", true)]
    [TestCase("vcruntime140.dll", true)]
    [TestCase("VCRUNTIME140_COR3.DLL", true)] // case-insensitive
    [TestCase("some_vcruntime_file.dll", true)]
    [TestCase("vcruntime", true)]
    [TestCase("CustomDll.dll", false)]
    [TestCase("user32.dll", false)]
    [TestCase("", false)]
    [TestCase("   ", false)]
    [TestCase(null, false)]
    public void IsVCRuntimeDll_should_identify_vcruntime_dlls(string dllName, bool expected)
    {
        bool result = UIReporter.TestAccessor.IsVCRuntimeDll(dllName);
        result.Should().Be(expected);
    }

    [Test]
    public void CreateFailedToLoadAnAssemblyReport_should_include_report_button_for_custom_assembly()
    {
        FileNotFoundException exception = new("Could not load file or assembly CustomLib", "CustomLib");

        TaskDialogPage page = UIReporter.TestAccessor.CreateFailedToLoadAnAssemblyReport(exception, isTerminating: false);

        page.Icon.Should().Be(TaskDialogIcon.Warning);
        page.AllowCancel.Should().BeFalse();
        page.Heading.Should().Contain("CustomLib");
        page.Buttons.Should().HaveCount(2);
        page.Expander.Should().NotBeNull();
        page.Expander!.Text.Should().Be(exception.Message);
    }

    [Test, TestCaseSource(nameof(FrameworkAssemblyAndVCRuntimeTestCases))]
    public void CreateFailedToLoadAnAssemblyReport_should_not_include_report_button(Exception exception, string expectedHeadingPart)
    {
        TaskDialogPage page = UIReporter.TestAccessor.CreateFailedToLoadAnAssemblyReport(exception, isTerminating: false);

        page.Heading.Should().Contain(expectedHeadingPart);
        page.Buttons.Should().HaveCount(1);
    }

    private static IEnumerable<TestCaseData> FrameworkAssemblyAndVCRuntimeTestCases
    {
        get
        {
            yield return new TestCaseData(
                new FileNotFoundException(
                    "Could not load file or assembly 'System.Data.Common, Version=9.0.0.0'",
                    fileName: "System.Data.Common, Version=9.0.0.0"),
                "System.Data.Common");

            yield return new TestCaseData(
                new FileNotFoundException(
                    "Could not load file or assembly 'Microsoft.CSharp, Version=9.0.0.0'",
                    fileName: "Microsoft.CSharp, Version=9.0.0.0"),
                "Microsoft.CSharp");

            yield return new TestCaseData(
                new DllNotFoundException(
                    "Unable to load DLL 'vcruntime140_cor3.dll' or one of its dependencies: The specified module could not be found."),
                "vcruntime140_cor3.dll");
        }
    }

    [Test]
    public void CreateDubiousOwnershipReport_should_create_page_with_expected_buttons()
    {
        const string errorMessage = $"fatal: detected dubious ownership in repository at 'd:/repo'\nTo add an exception for this directory, call:\n\n\tgit config --global --add safe.directory d:/repo\n";

        ExternalOperationException exception = new(
            command: "git",
            workingDirectory: "d:/repo",
            innerException: new Exception(errorMessage));

        TaskDialogPage page = UIReporter.TestAccessor.CreateDubiousOwnershipReport(exception);

        page.Icon.Should().Be(TaskDialogIcon.Error);
        page.AllowCancel.Should().BeTrue();
        page.Buttons.Should().HaveCount(4);
        page.Expander.Should().NotBeNull();
        page.Expander!.Text.Should().Be(errorMessage);
    }

    [TearDown]
    public void TearDown()
    {
        UIReporter.IgnoreFailedToLoadAnAssembly = false;
    }

    [Test]
    public void CreateErrorReport_should_show_report_bug_and_ignore_buttons_for_internal_operation()
    {
        Exception exception = new("something broke");
        StringBuilder text = new("error details");
        OperationInfo operationInfo = new() { IsExternalOperation = false, IsUserExternalOperation = false };

        TaskDialogPage page = UIReporter.TestAccessor.CreateErrorReport(exception, RootError, text, operationInfo);

        page.Icon.Should().Be(TaskDialogIcon.Error);
        page.Heading.Should().Be(RootError);
        page.AllowCancel.Should().BeTrue();
        page.Buttons.Should().HaveCount(2);
    }

    [Test]
    public void CreateErrorReport_should_show_report_issue_and_ignore_buttons_for_external_operation()
    {
        Exception exception = new("git failed");
        StringBuilder text = new("error details");
        OperationInfo operationInfo = new() { IsExternalOperation = true, IsUserExternalOperation = false };

        TaskDialogPage page = UIReporter.TestAccessor.CreateErrorReport(exception, RootError, text, operationInfo);

        page.Icon.Should().Be(TaskDialogIcon.Warning);
        page.Buttons.Should().HaveCount(2);
    }

    [Test]
    public void CreateErrorReport_should_show_view_details_and_ignore_buttons_for_user_external_operation()
    {
        Exception exception = new("user script failed");
        StringBuilder text = new("error details");
        OperationInfo operationInfo = new() { IsExternalOperation = true, IsUserExternalOperation = true };

        TaskDialogPage page = UIReporter.TestAccessor.CreateErrorReport(exception, RootError, text, operationInfo);

        page.Icon.Should().Be(TaskDialogIcon.Warning);
        page.Buttons.Should().HaveCount(2);
    }

    [Test]
    public void CreateFailedToLoadAnAssemblyReport_should_show_expander_with_exception_message()
    {
        DllNotFoundException exception = new("Unable to load DLL 'vcruntime140_cor3.dll'");

        TaskDialogPage page = UIReporter.TestAccessor.CreateFailedToLoadAnAssemblyReport(exception, isTerminating: false);

        page.Expander.Should().NotBeNull();
        page.Expander!.Text.Should().Be(exception.Message);
    }

    [Test]
    public void ReportFailedToLoadAnAssembly_should_return_false_when_IgnoreFailedToLoadAnAssembly_is_set()
    {
        UIReporter.IgnoreFailedToLoadAnAssembly = true;
        UIReporter reporter = new();

        bool result = reporter.ReportFailedToLoadAnAssembly(
            new FileNotFoundException("Could not load file or assembly CustomLib", "CustomLib"),
            isTerminating: false);

        result.Should().BeFalse();
    }

    [Test, TestCaseSource(nameof(NonAssemblyLoadExceptionTestCases))]
    public void ReportFailedToLoadAnAssembly_should_return_false_for_non_matching_exception(Exception exception)
    {
        UIReporter reporter = new();

        bool result = reporter.ReportFailedToLoadAnAssembly(exception, isTerminating: false);

        result.Should().BeFalse();
    }

    private static IEnumerable<TestCaseData> NonAssemblyLoadExceptionTestCases
    {
        get
        {
            yield return new TestCaseData(new Exception("some error"));
            yield return new TestCaseData(new InvalidOperationException("something went wrong"));
            yield return new TestCaseData(new FileNotFoundException("File not found"));
            yield return new TestCaseData(new DllNotFoundException("Unable to load DLL 'user32.dll'"));
            yield return new TestCaseData(new Exception("outer", new Exception("inner")));
        }
    }
}
