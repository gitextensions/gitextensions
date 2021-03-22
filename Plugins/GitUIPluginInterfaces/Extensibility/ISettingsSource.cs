namespace GitUIPluginInterfaces.Extensibility
{
    public interface ISettingsSource
    {
        /// <summary>
        /// Extracts the string value with the specified key as is.
        /// </summary>
        /// <param name="key">The key of the setting value.</param>
        /// <returns>The string value.</returns>
        string GetValue(string key);

        /// <summary>
        /// Extracts the string value with the specified key as is or use default value.
        /// </summary>
        /// <param name="key">The key of the setting value.</param>
        /// <param name="defaultValue">The default value to use if no value is found.</param>
        /// <returns>The string value.</returns>
        string GetValue(string key, string defaultValue);

        /// <summary>
        /// Extracts the value with the specified key and converts it to type T or create new.
        /// </summary>
        /// <typeparam name="T">The type to convert the value to.</typeparam>
        /// <param name="key">The key of the setting value.</param>
        /// <returns>The converted value to T or new T().</returns>
        T GetValue<T>(string key)
            where T : new();

        /// <summary>
        /// Extracts the value with the specified key and converts it to type T or use default value.
        /// </summary>
        /// <typeparam name="T">The type to convert the value to.</typeparam>
        /// <param name="key">The key of the setting value.</param>
        /// <param name="defaultValue">The default value to use if no value is found.</param>
        /// <returns>The converted value to T.</returns>
        T GetValue<T>(string key, T defaultValue);

        /// <summary>
        /// Stores the value with the specified key.
        /// </summary>
        /// <typeparam name="T">The type to convert the value from.</typeparam>
        /// <param name="key">The key of the setting value.</param>
        /// <param name="value">The value to save.</param>
        void SetValue<T>(string key, T value);
    }
}
