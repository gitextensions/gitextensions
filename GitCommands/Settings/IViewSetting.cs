using System.Data.SqlTypes;

namespace GitCommands
{
    public interface IViewSetting
    {
        /// <summary>
        /// Reloads the value from the settings file.
        /// </summary>
        void Reload();

        /// <summary>
        /// Resets the value to hard-coded default value.
        /// </summary>
        void ResetToDefault();

        /// <summary>
        /// Saves the value to the settings file.
        /// </summary>
        void Save();
    }
}
