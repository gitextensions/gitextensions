using System.Text;
using Avalonia.Controls;
using GitCommands;
using GitExtensions.Shims.WinForms;

namespace GitUI.Compat;

/// <summary>
///  Defines the portable boundary between exception classification and the native report UI.
/// </summary>
internal interface IBugReportLauncher
{
    DialogResult Show(
        Window? owner,
        Exception exception,
        string exceptionInfo,
        string environmentInfo,
        bool canIgnore,
        bool showIgnore,
        bool focusDetails);
}

/// <summary>
///  Presents a native Avalonia review dialog and opens the Git Extensions issue form through
///  the platform shell or desktop portal.
/// </summary>
internal sealed class AvaloniaBugReportLauncher : IBugReportLauncher
{
    private const string NewIssueUrl = "https://github.com/gitextensions/gitextensions/issues/new";

    public DialogResult Show(
        Window? owner,
        Exception exception,
        string exceptionInfo,
        string environmentInfo,
        bool canIgnore,
        bool showIgnore,
        bool focusDetails)
    {
        DialogResult result = canIgnore ? DialogResult.Ignore : DialogResult.Abort;
        TaskDialogCommandLinkButton reportButton = new(
            TranslatedStrings.ReportIssue,
            TranslatedStrings.ReportIssueDescription);
        reportButton.Click += (_, _) =>
        {
            result = DialogResult.OK;
            OsShellUtil.OpenUrlInDefaultBrowser(BuildIssueUrl(exception, exceptionInfo, environmentInfo));
        };

        TaskDialogPage page = new()
        {
            Icon = TaskDialogIcon.Error,
            Caption = TranslatedStrings.Error,
            Heading = exception.Message,
            Text = TranslatedStrings.ReportBug,
            AllowCancel = canIgnore,
            SizeToContent = true,
            Buttons = { reportButton },
            Expander = new TaskDialogExpander
            {
                CollapsedButtonText = TranslatedStrings.ButtonViewDetails,
                ExpandedButtonText = TranslatedStrings.HideErrorMessage,
                Position = TaskDialogExpanderPosition.AfterFootnote,
                Text = BuildDetails(exception, exceptionInfo, environmentInfo),
                IsExpanded = focusDetails,
            },
        };

        if (canIgnore || showIgnore)
        {
            TaskDialogCommandLinkButton ignoreButton = new(TranslatedStrings.ButtonIgnore);
            ignoreButton.Click += (_, _) => result = DialogResult.Ignore;
            page.Buttons.Add(ignoreButton);
        }

        TaskDialog.ShowDialog(owner as IWin32Window, page);
        return result;
    }

    internal static string BuildIssueUrl(Exception exception, string exceptionInfo, string environmentInfo)
    {
        string title = $"[NBug] {exception.Message}";
        if (title.Length > 69)
        {
            title = title[..66] + "...";
        }

        string details = BuildDetails(exception, exceptionInfo, environmentInfo);
        return $"{NewIssueUrl}?template=bug_report.yml"
            + $"&labels={Uri.EscapeDataString("type: NBug")}"
            + $"&title={Uri.EscapeDataString(title)}"
            + $"&about={Uri.EscapeDataString(environmentInfo)}"
            + $"&description={Uri.EscapeDataString(details)}";
    }

    private static string BuildDetails(Exception exception, string exceptionInfo, string environmentInfo)
    {
        StringBuilder details = new();
        if (!string.IsNullOrWhiteSpace(environmentInfo))
        {
            details.AppendLine(environmentInfo.Trim());
            details.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(exceptionInfo))
        {
            details.AppendLine(exceptionInfo.Trim());
            details.AppendLine();
        }

        details.AppendLine("```");
        details.AppendLine(exception.ToString().Trim());
        details.AppendLine("```");
        return details.ToString();
    }
}
