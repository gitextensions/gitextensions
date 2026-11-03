using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Shims.WinForms;

namespace GitUI.HelperDialogs;

public class FormRemoteProcess : FormProcess
{
    public FormRemoteProcess(IGitUICommands commands, ArgumentString arguments)
        : this(commands, arguments, useDialogSettings: true)
    {
    }

    // OpenSSH handles host-key confirmation and authentication through the existing process input.
    internal FormRemoteProcess(IGitUICommands commands, ArgumentString arguments, bool useDialogSettings)
        : base(commands, arguments, commands.Module.WorkingDir, input: null, useDialogSettings)
    {
    }

    public static bool ShowDialog(IWin32Window? owner, IGitUICommands commands, ArgumentString arguments)
    {
        using FormRemoteProcess formRemoteProcess = new(commands, arguments);
        formRemoteProcess.ShowDialog(owner);
        return !formRemoteProcess.ErrorOccurred();
    }
}
