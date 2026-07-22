using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.ScriptsEngine;
using ResourceManager;

namespace GitUI;

/// <summary>Base control that obtains Git UI commands from its containing form.</summary>
public class GitModuleControl : GitExtensionsControl, IWin32Window
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

    /// <summary>Gets the commands only if their source has already been set.</summary>
    internal bool TryGetUICommandsDirect([NotNullWhen(returnValue: true)] out IGitUICommands? commands)
    {
        commands = _uiCommandsSource?.UICommands;
        return commands is not null;
    }

    /// <summary>Gets the current Git module.</summary>
    public IGitModule Module => UICommands.Module;

    nint IWin32Window.Handle => TopLevel.GetTopLevel(this)?.TryGetPlatformHandle()?.Handle ?? 0;

    protected override IServiceProvider ServiceProvider => UICommands;

    protected override bool ExecuteCommand(int command)
    {
        if (TryGetUICommandsDirect(out IGitUICommands? commands)
            && commands.GetService(typeof(IScriptsManager)) is IScriptsManager scriptsManager
            && scriptsManager.GetScript(command) is ScriptInfo script)
        {
            IScriptsRunner scriptsRunner = commands.GetRequiredService<IScriptsRunner>();
            _ = scriptsRunner.RunScript(script, this, commands, FindScriptOptionsProvider());
            return true;
        }

        return base.ExecuteCommand(command);
    }

    internal IScriptOptionsProvider FindScriptOptionsProvider()
    {
        if (GetScriptOptionsProvider() is IScriptOptionsProvider ownProvider)
        {
            return ownProvider;
        }

        foreach (object ancestor in this.GetLogicalAncestors())
        {
            if (ancestor is GitModuleControl gitModuleControl
                && gitModuleControl.GetScriptOptionsProvider() is IScriptOptionsProvider controlProvider)
            {
                return controlProvider;
            }

            if (ancestor is GitModuleForm gitModuleForm)
            {
                return gitModuleForm.GetScriptOptionsProvider();
            }
        }

        return ScriptOptionsProviderBase.Default;
    }

    protected virtual IScriptOptionsProvider? GetScriptOptionsProvider()
        => null;
}
