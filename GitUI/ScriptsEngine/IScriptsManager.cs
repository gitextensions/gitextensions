using GitCommands.Settings;

namespace GitUI.ScriptsEngine
{
    public interface IScriptsManager
    {
        /// <summary>
        ///  Adds the provided script definition at the lowest available level.
        /// </summary>
        /// <param name="script">Script definition.</param>
        void Add(ScriptInfo script);

        ScriptInfo? GetScript(int scriptId);

        /// <summary>
        ///  Loads all script definitions from all available levels.
        /// </summary>
        /// <returns>A collection of all available definitions.</returns>
        IReadOnlyList<ScriptInfo> GetScripts();

        void Initialize(DistributedSettings settings);

        /// <summary>
        ///  Removes the supplied script definition from the list.
        /// </summary>
        /// <param name="script">Script definition.</param>
        void Remove(ScriptInfo script);

        /// <summary>
        ///  Saves the currently loaded script definitions to the settings.
        /// </summary>
        void Save();

        /// <summary>
        ///  Updates the supplied script definition.
        /// </summary>
        /// <param name="script">Script definition.</param>
        void Update(ScriptInfo script);
    }
}
