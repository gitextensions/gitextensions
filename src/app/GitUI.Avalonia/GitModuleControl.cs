using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.ScriptsEngine;
using ResourceManager;

namespace GitUI;

/// <summary>
/// Base class for a <see cref="UserControl"/> requiring <see cref="IGitModule"/> and <see cref="GitUICommands"/>.
/// </summary>
public class GitModuleControl : GitExtensionsControl, IGitModuleControl, IWin32Window
{
    private IGitUICommandsSource? _uiCommandsSource;

    /// <summary>
    /// Occurs after the <see cref="UICommandsSource"/> is set.
    /// Will only occur once, as the source cannot change after being set.
    /// </summary>
    public event EventHandler<GitUICommandsSourceEventArgs>? UICommandsSourceSet;

    protected override IServiceProvider ServiceProvider => UICommands;

    // Avalonia controls are accessed on their dispatcher thread. Use the setter,
    // as the original does, so lazy discovery also initializes subscriptions.

    /// <summary>
    /// Gets a <see cref="IGitUICommandsSource"/> for this control.
    /// </summary>
    /// <remarks>
    /// If the commands source has not yet been initialised, this property's getter attempts
    /// to find a control-tree ancestor of type <see cref="IGitUICommandsSource"/>.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Unable to initialise the source as
    /// no ancestor of type <see cref="IGitUICommandsSource"/> was found.</exception>
    public IGitUICommandsSource UICommandsSource
    {
        get
        {
            if (_uiCommandsSource is null)
            {
                // Search ancestors for an implementation of IGitUICommandsSource
                UICommandsSource = this.GetLogicalAncestors()
                    .OfType<IGitUICommandsSource>()
                    .FirstOrDefault()
                    ?? throw new InvalidOperationException("The UI Command Source is not available for this control. Are you calling methods before adding it to the parent control?");
            }

            // ReSharper disable once AssignNullToNotNullAttribute
            return _uiCommandsSource!;
        }
        set
        {
            if (_uiCommandsSource is not null)
            {
                throw new InvalidOperationException($"{nameof(UICommandsSource)} is already set.");
            }

            ArgumentNullException.ThrowIfNull(value);
            _uiCommandsSource = value;
            OnUICommandsSourceSet(value);
        }
    }

    /// <summary>
    ///  Gets the <see cref="UICommandsSource"/>'s <see cref="GitUICommands"/> reference.
    /// </summary>
    public IGitUICommands UICommands => UICommandsSource.UICommands;

    /// <summary>
    /// Gets the UI commands, if they've initialised.
    /// </summary>
    /// <remarks>
    /// <para>This method will not attempt to initialise the commands if they have not
    /// yet been initialised.</para>
    /// <para>By contrast, the <see cref="UICommands"/> property attempts to initialise
    /// the value if not previously initialised.</para>
    /// </remarks>
    internal bool TryGetUICommandsDirect([NotNullWhen(returnValue: true)] out IGitUICommands? commands)
    {
        commands = _uiCommandsSource?.UICommands;
        return commands is not null;
    }

    nint IWin32Window.Handle => TopLevel.GetTopLevel(this)?.TryGetPlatformHandle()?.Handle ?? 0;

    /// <summary>Gets the <see cref="UICommands"/>' <see cref="IGitModule"/> reference.</summary>
    public IGitModule Module => UICommands.Module;

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

    /// <summary>Raises the <see cref="UICommandsSourceSet"/> event.</summary>
    protected virtual void OnUICommandsSourceSet(IGitUICommandsSource source)
    {
        UICommandsSourceSet?.Invoke(this, new GitUICommandsSourceEventArgs(source));
    }
}
