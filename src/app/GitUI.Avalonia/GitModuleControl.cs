using Avalonia.LogicalTree;
using GitExtensions.Extensibility.Git;
using ResourceManager;

namespace GitUI;

/// <summary>Base control that obtains Git UI commands from its containing form.</summary>
public class GitModuleControl : GitExtensionsControl
{
    private IGitUICommandsSource? _uiCommandsSource;

    /// <summary>Occurs after <see cref="UICommandsSource"/> is set.</summary>
    public event EventHandler<GitUICommandsSourceEventArgs>? UICommandsSourceSet;

    /// <summary>Gets or sets the command source for this control.</summary>
    public IGitUICommandsSource UICommandsSource
    {
        get
        {
            _uiCommandsSource ??= this.GetLogicalAncestors()
                .OfType<IGitUICommandsSource>()
                .FirstOrDefault()
                ?? throw new InvalidOperationException("The UI Command Source is not available for this control. Are you calling methods before adding it to the parent control?");

            return _uiCommandsSource;
        }
        set
        {
            if (_uiCommandsSource is not null)
            {
                throw new InvalidOperationException($"{nameof(UICommandsSource)} is already set.");
            }

            ArgumentNullException.ThrowIfNull(value);
            _uiCommandsSource = value;
            UICommandsSourceSet?.Invoke(this, new GitUICommandsSourceEventArgs(value));
        }
    }

    /// <summary>Gets the commands exposed by <see cref="UICommandsSource"/>.</summary>
    public IGitUICommands UICommands => UICommandsSource.UICommands;

    /// <summary>Gets the current Git module.</summary>
    public IGitModule Module => UICommands.Module;
}
