namespace GitUI.CommandsDialogs.GitIgnoreDialog
{
    public interface IGitIgnoreDialogModel
    {
        string FormCaption { get; }
        string ExcludeFile { get; }
        string FileOnlyInWorkingDirSupported { get; }
        string CannotAccessFile { get; }
        string CannotAccessFileCaption { get; }
        string SaveFileQuestion { get; }
    }
}