using Avalonia.Controls;
using BugReporter;
using BugReporter.Serialization;
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
    public DialogResult Show(
        Window? owner,
        Exception exception,
        string exceptionInfo,
        string environmentInfo,
        bool canIgnore,
        bool showIgnore,
        bool focusDetails)
    {
        using BugReportForm form = new();
        return form.ShowDialog(owner as IWin32Window, new SerializableException(exception), exceptionInfo,
            environmentInfo, canIgnore, showIgnore, focusDetails);
    }
}
