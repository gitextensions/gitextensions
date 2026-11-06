using WinFormsTaskDialogs = GitExtensions.Shims.WinForms.TaskDialogs;

namespace GitUI.Compat;

/// <summary>
///  Adapts the headless task-dialog contract used by portable plugins to the existing
///  Avalonia task-dialog renderer.
/// </summary>
public sealed class AvaloniaTaskDialogHost : WinFormsTaskDialogs.ITaskDialogHost
{
    public Task<WinFormsTaskDialogs.TaskDialogButton> ShowDialogAsync(WinFormsTaskDialogs.TaskDialogPage page)
    {
        Dictionary<TaskDialogButton, WinFormsTaskDialogs.TaskDialogButton> buttons = [];
        TaskDialogPage avaloniaPage = new()
        {
            Heading = page.Heading,
            Caption = page.Caption,
            Icon = page.Icon switch
            {
                WinFormsTaskDialogs.TaskDialogIcon.Information => TaskDialogIcon.Information,
                WinFormsTaskDialogs.TaskDialogIcon.Warning => TaskDialogIcon.Warning,
                WinFormsTaskDialogs.TaskDialogIcon.Error => TaskDialogIcon.Error,
                _ => TaskDialogIcon.None,
            },
            AllowCancel = page.AllowCancel,
        };

        foreach (WinFormsTaskDialogs.TaskDialogButton button in page.Buttons)
        {
            TaskDialogButton avaloniaButton = new(button.Text);
            buttons.Add(avaloniaButton, button);
            avaloniaPage.Buttons.Add(avaloniaButton);
        }

        TaskDialogButton result = TaskDialog.ShowDialog(owner: null, avaloniaPage);
        return Task.FromResult(buttons.GetValueOrDefault(result, WinFormsTaskDialogs.TaskDialogButton.Cancel));
    }
}
