#nullable enable

using System;

namespace GitCommands.Settings
{
    public class SaveSettingsException : Exception
    {
        public SaveSettingsException(Exception? innerException)
            : base(message: null, innerException)
        {
        }
    }
}
