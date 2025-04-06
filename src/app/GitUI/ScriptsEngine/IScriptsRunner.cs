using GitExtensions.Extensibility.Git;
using ResourceManager;

namespace GitUI.ScriptsEngine;

/// <summary>
/// Provides methods to run scripts.
/// </summary>
public interface IScriptsRunner
{
    /// <summary>
    ///  Runs the scripts associated with a specific event.
    /// </summary>
    /// <typeparam name="THostForm">The type of the host form.</typeparam>
    /// <param name="scriptEvent">The event that triggers the scripts.</param>
    /// <param name="form">The host form.</param>
    /// <returns><see langword="true"/> if the scripts were run successfully; otherwise, <see langword="false"/>.</returns>
    bool RunEventScripts<THostForm>(ScriptEvent scriptEvent, THostForm form)
        where THostForm : IGitModuleForm, IWin32Window;

    /// <summary>
    ///  Runs a specific script.
    /// </summary>
    /// <param name="scriptInfo">The script information.</param>
    /// <param name="owner">The owner window.</param>
    /// <param name="commands">The Git UI commands.</param>
    /// <param name="scriptOptionsProvider">The script options provider (optional).</param>
    /// <returns><see langword="true"/> if the script was run successfully; otherwise, <see langword="false"/>.</returns>
    bool RunScript(ScriptInfo scriptInfo, IWin32Window owner, IGitUICommands commands, IScriptOptionsProvider? scriptOptionsProvider = null);
}
