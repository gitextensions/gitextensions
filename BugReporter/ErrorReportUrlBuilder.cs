using System.Text;
using BugReporter.Serialization;

namespace BugReporter
{
    public interface IErrorReportUrlBuilder
    {
        string Build(SerializableException exception, string exceptionInfo, string environmentInfo, string? additionalInfo);
        string CopyText(SerializableException exception, string exceptionInfo, string environmentInfo, string? additionalInfo);
    }

    public sealed class ErrorReportUrlBuilder : IErrorReportUrlBuilder
    {
        public string Build(SerializableException exception, string exceptionInfo, string environmentInfo, string? additionalInfo)
        {
            if (exception is null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            StringBuilder sb = new();

            sb.Append($"template=bug_report.yml");
            sb.Append($"&labels={Uri.EscapeDataString("type: NBug")}");
            sb.Append($"&about={Uri.EscapeDataString(environmentInfo)}");
            sb.Append($"&description={Uri.EscapeDataString(GetExceptionDetails(exception, exceptionInfo, additionalInfo).ToString())}");

            return sb.ToString();
        }

        public string CopyText(SerializableException exception, string exceptionInfo, string environmentInfo, string? additionalInfo)
        {
            if (exception is null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            StringBuilder sb = new();
            sb.Append(environmentInfo);
            sb.Append(GetExceptionDetails(exception, exceptionInfo, additionalInfo).ToString());

            return sb.ToString();
        }

        private static StringBuilder GetExceptionDetails(SerializableException exception, string exceptionInfo, string? additionalInfo)
        {
            StringBuilder exceptionDetails = new();
            if (!string.IsNullOrEmpty(exceptionInfo))
            {
                // Tests may be run on VMs, and AppendLine() could be either \r\n or \n
                exceptionDetails.Append('\n');
                exceptionDetails.Append(exceptionInfo.Trim());
                exceptionDetails.Append('\n');
                exceptionDetails.Append('\n');
            }

            exceptionDetails.Append("```\n");
            exceptionDetails.Append(exception.ToString().Trim());
            exceptionDetails.Append('\n');
            exceptionDetails.Append("```\n");
            exceptionDetails.Append('\n');

            if (!string.IsNullOrWhiteSpace(additionalInfo))
            {
                exceptionDetails.Append(additionalInfo.Trim());
                exceptionDetails.Append('\n');
            }

            return exceptionDetails;
        }
    }
}
