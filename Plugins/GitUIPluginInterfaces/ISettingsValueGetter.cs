namespace GitUIPluginInterfaces
{
    public interface ISettingsValueGetter
    {
        string GetValue(string setting);

        /// <summary>
        /// Get the config setting from git converted in an expected C# value type (bool, int, ...)
        /// </summary>
        /// <typeparam name="T">the expected type of the git setting.</typeparam>
        /// <param name="setting">the git setting key</param>
        /// <returns>
        /// null if the settings is not set
        /// the value converted in the <typeparamref name="T" /> type otherwise.
        /// </returns>
        T? GetValue<T>(string setting) where T : struct;
    }
}
