using FluentAssertions;
using GitUI.NBugReports;

namespace GitUITests.NBugReports;

[TestFixture]
public sealed class UIReporterTests
{
    [Test, TestCaseSource(typeof(UIReporterTests), nameof(GetFileNameTestCases))]
    public void DllNameExtraction_should_extract_dll_name_from_message(Exception exception, string expectedDllName)
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

            // FileNotFoundException
            yield return new TestCaseData(
                new FileNotFoundException("Could not load file or assembly Microsoft.CSharp", "Microsoft.CSharp"),
                "Microsoft.CSharp");
            yield return new TestCaseData(
                new FileNotFoundException("Could not load file or assembly 'System.Data.Common, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'", fileName: "System.Data.Common, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"),
                "System.Data.Common");

            // Other exceptions
            yield return new TestCaseData(
                new Exception("No quotes in this message"),
                "unknown");
            yield return new TestCaseData(
                new Exception("'only_one_quote"),
                "unknown");
        }
    }
}
