using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Shims.WinForms;

namespace GitUI.HelperDialogs;

// Twin of GitUI/HelperDialogs/FormRemoteProcess.cs. Remote commands retain their own
// semantic process boundary and the inherited retry callback. OpenSSH handles host-key
// confirmation and authentication through the existing process input; PuTTY-only registry
// and key-agent recovery is intentionally not part of the cross-platform application.
public class FormRemoteProcess : FormProcess
{
    public FormRemoteProcess(IGitUICommands commands, ArgumentString arguments)
        : this(commands, arguments, useDialogSettings: true)
    {
    }

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
