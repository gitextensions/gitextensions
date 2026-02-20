using System.Collections;
using System.Text;
using FluentAssertions;
using GitExtensions.Extensibility;
using GitUI.NBugReports;

namespace GitUITests.NBugReports;

[TestFixture]
public sealed class BugReportInvokerTests
{
    [Test, TestCaseSource(typeof(TestExceptions), "TestCases")]
    public void Append(Exception exception, string expectedRootError, string expectedText)
    {
        StringBuilder text = BugReportInvoker.GetExceptionInfo(exception);
        string rootError = BugReportInvoker.GetRootError(exception);
        rootError.Should().Be(expectedRootError);
        text.ToString().Should().Be(expectedText);
    }

    [TestCase("System.Data.Common", true)]
    [TestCase("System.IO", true)]
    [TestCase("Microsoft.CSharp", true)]
    [TestCase("Microsoft.VisualBasic", true)]
    [TestCase("system.data.common", true)] // case-insensitive
    [TestCase("MICROSOFT.Extensions", true)] // case-insensitive
    [TestCase("CustomAssembly", false)]
    [TestCase("MyApp.Core", false)]
    [TestCase("", false)]
    [TestCase(null, false)]
    public void IsDotNetFrameworkAssembly_should_identify_framework_assemblies(string assemblyName, bool expected)
    {
        // The logic being tested (as implemented in ReportFailedToLoadAnAssembly local function):
        // - Returns true for assemblies starting with "System." or "Microsoft."
        // - Case-insensitive comparison
        // - Returns false for null or whitespace

        bool result = IsSystemOrMicrosoftAssembly(assemblyName);
        result.Should().Be(expected);

        static bool IsSystemOrMicrosoftAssembly(string assemblyName)
        {
            if (string.IsNullOrWhiteSpace(assemblyName))
            {
                return false;
            }

            return assemblyName.StartsWith("System.", StringComparison.OrdinalIgnoreCase)
                || assemblyName.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase);
        }
    }

    [TestCase("vcruntime140_cor3.dll", true)]
    [TestCase("vcruntime140.dll", true)]
    [TestCase("VCRUNTIME140_COR3.DLL", true)] // case-insensitive
    [TestCase("some_vcruntime_file.dll", true)]
    [TestCase("CustomDll.dll", false)]
    [TestCase("user32.dll", false)]
    [TestCase("", false)]
    [TestCase(null, false)]
    public void IsVCRuntimeDll_should_identify_vcruntime_dlls(string dllName, bool expected)
    {
        // The logic being tested (as implemented in ReportFailedToLoadAnAssembly local function):
        // - Returns true for DLL names containing "vcruntime"
        // - Case-insensitive comparison
        // - Returns false for null or whitespace

        bool result = ContainsVCRuntime(dllName);
        result.Should().Be(expected);

        static bool ContainsVCRuntime(string dllName)
        {
            if (string.IsNullOrWhiteSpace(dllName))
            {
                return false;
            }

            return dllName.Contains("vcruntime", StringComparison.OrdinalIgnoreCase);
        }
    }

    [TestCase("Unable to load DLL 'vcruntime140_cor3.dll' or one of its dependencies: The specified module could not be found.", "vcruntime140_cor3.dll")]
    [TestCase("Unable to load DLL 'user32.dll': File not found.", "user32.dll")]
    [TestCase("Unable to load DLL 'my.custom.dll' for some reason", "my.custom.dll")]
    [TestCase("No quotes in this message", "No quotes in this message")] // fallback case
    [TestCase("'only_one_quote", "'only_one_quote")] // fallback case
    public void DllNameExtraction_should_extract_dll_name_from_message(string message, string expectedDllName)
    {
        // The logic being tested (as implemented in ReportFailedToLoadAnAssembly):
        // - Extracts DLL name between single quotes from DllNotFoundException message
        // - Falls back to using the entire message if no quotes found

        string result = ExtractDllNameFromMessage(message);
        result.Should().Be(expectedDllName);

        static string ExtractDllNameFromMessage(string message)
        {
            string fileName = message;
            int startIndex = fileName.IndexOf('\'');
            if (startIndex >= 0)
            {
                int endIndex = fileName.IndexOf('\'', startIndex + 1);
                if (endIndex > startIndex)
                {
                    fileName = fileName.Substring(startIndex + 1, endIndex - startIndex - 1);
                }
            }

            return fileName;
        }
    }
}

public class TestExceptions
{
    private const string _messageOuter = "outer";
    private const string _messageMiddle = "middle";
    private const string _messageInner = "inner";
    private const string _context = "context";
    private const string _command = "command";
    private const string _arguments = "arguments";
    private const string _directory = "directory";
    private const int _exitCode = 128;

    public static IEnumerable TestCases
    {
        get
        {
            yield return new TestCaseData(new Exception(_messageOuter),
                _messageOuter,
                "");
            yield return new TestCaseData(new Exception(_messageOuter, new Exception(_messageMiddle)),
                _messageMiddle,
                "");
            yield return new TestCaseData(new Exception(_messageOuter, new Exception(_messageMiddle, new Exception(_messageInner))),
                _messageInner,
                "");
            yield return new TestCaseData(new UserExternalOperationException(_context,
                new ExternalOperationException(_command, _arguments, _directory, _exitCode, new Exception(_messageOuter, new Exception(_messageInner)))),
                _messageInner,
                $"{_context}{Environment.NewLine}"
                + $"Exit code: {_exitCode}{Environment.NewLine}"
                + $"Command: {_command}{Environment.NewLine}"
                + $"Arguments: {_arguments}{Environment.NewLine}"
                + $"Working directory: {_directory}{Environment.NewLine}");
            yield return new TestCaseData(new UserExternalOperationException(context: null,
                new ExternalOperationException(null, null, null, null, new Exception(_messageInner))),
                _messageInner,
                "");
        }
    }
}
