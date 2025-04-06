#nullable enable

using GitCommands.Settings;

namespace GitUI.ScriptsEngine;

/// <summary>
/// Provides methods to manage and manipulate script definitions.
/// </summary>
public interface IScriptsManager
{
    /// <summary>
    ///  Adds the provided script definition at the lowest available level.
    /// </summary>
    /// <param name="script">The script to add.</param>
    void Add(ScriptInfo script);

    /// <summary>
    ///  Gets the script definition with the specified ID.
    /// </summary>
    /// <param name="scriptId">The ID of the script to retrieve.</param>
    /// <returns>The script definition, if found; otherwise, <see langword="null"/>.</returns>
    ScriptInfo? GetScript(int scriptId);

    /// <summary>
    ///  Loads all script definitions from all available levels.
    /// </summary>
    /// <returns>A collection of all available scripts.</returns>
    IReadOnlyList<ScriptInfo> GetScripts();

    /// <summary>
    ///  Initializes the script manager with the provided settings.
    /// </summary>
    /// <param name="settings">The settings to initialize with.</param>
    void Initialize(DistributedSettings settings);

    /// <summary>
    ///  Removes the supplied script definition from the list.
    /// </summary>
    /// <param name="script">The script to remove.</param>
    void Remove(ScriptInfo script);

    /// <summary>
    ///  Saves the currently loaded script definitions to the settings.
    /// </summary>
    void Save();

    /// <summary>
    ///  Updates the supplied script definition.
    /// </summary>
    /// <param name="script">The script to update.</param>
    void Update(ScriptInfo script);
}
