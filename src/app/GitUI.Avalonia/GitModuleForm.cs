using System.Diagnostics.CodeAnalysis;
using GitExtensions.Extensibility.Git;

namespace GitUI;

// Twin of GitUI/GitModuleForm.cs (reduced): access to IGitUICommands and the GitModule,
// including the command-service bridge used by the shared hotkey dispatcher. Scripts remain deferred.

/// <summary>Base window that provides access to the module and <see cref="IGitUICommands"/>.</summary>
public class GitModuleForm : GitExtensionsForm, IGitUICommandsSource
{
    private IGitUICommands? _uiCommands;

    public event EventHandler<GitUICommandsChangedEventArgs>? UICommandsChanged;

    /// <summary>For the visual designer and construction tests only, like WinForms.</summary>
    protected GitModuleForm()
    {
    }

    protected GitModuleForm(IGitUICommands? commands, bool enablePositionRestore)
    {
        _uiCommands = commands;
    }

    public IGitUICommands UICommands
    {
        get => _uiCommands
            ?? throw new InvalidOperationException($"{nameof(UICommands)} is null. {GetType().FullName} was constructed incorrectly.");
        protected set
        {
            ArgumentNullException.ThrowIfNull(value);
            IGitUICommands? oldCommands = _uiCommands;
            _uiCommands = value;
            UICommandsChanged?.Invoke(this, new GitUICommandsChangedEventArgs(oldCommands));
        }
    }

    /// <summary>Gets the module of the currently set <see cref="UICommands"/>.</summary>
    public IGitModule Module => UICommands.Module;

    public override bool TryGetUICommands([NotNullWhen(true)] out IGitUICommands? commands)
    {
        commands = _uiCommands;
        return commands is not null;
    }
}
