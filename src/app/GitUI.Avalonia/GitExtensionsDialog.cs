using GitExtensions.Extensibility.Git;

namespace GitUI;

// Twin of GitUI/GitExtensionsDialog.cs. The MainPanel/ControlsPanel chrome that the WinForms
// base contributes through its Designer file is declared in each dialog's .axaml instead
// (a bottom-docked ControlsPanel button row and a filling MainPanel), because Avalonia
// windows do not inherit visual trees.

/// <summary>Base class for a Git Extensions dialog.</summary>
public class GitExtensionsDialog : GitModuleForm
{
    /// <summary>For the visual designer and construction tests only, like WinForms.</summary>
    protected GitExtensionsDialog()
    {
    }

    /// <summary>Creates a new <see cref="GitExtensionsForm"/> indicating position restore.</summary>
    /// <param name="enablePositionRestore">Indicates whether the <see cref="Avalonia.Controls.Window"/>'s position
    /// will be restored upon being re-opened.</param>
    protected GitExtensionsDialog(IGitUICommands? commands, bool enablePositionRestore)
        : base(commands, enablePositionRestore)
    {
    }

    /// <summary>
    ///  Gets or sets the anchor pointing to a section in the manual pertaining to this dialog.
    /// </summary>
    public string? ManualSectionAnchorName { get; set; }

    /// <summary>
    ///  Gets or sets the name of a document pertaining to this dialog.
    /// </summary>
    public string? ManualSectionSubfolder { get; set; }
}
