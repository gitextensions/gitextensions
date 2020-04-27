namespace GitCommands.Settings
{
    public enum SettingsKind
    {
        /// <summary>
        /// Global for all repositories
        /// </summary>
        Global,

        /// <summary>
        /// Version-controlled, distributed with current repository
        /// </summary>
        Distributed,

        /// <summary>
        /// Local for current repository
        /// </summary>
        Local,

        /// <summary>
        /// Effective - first assigned value in the following order: Local, Distributable, Global
        /// </summary>
        Effective
    }
}
