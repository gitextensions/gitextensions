using System.Runtime.Serialization;

namespace GitCommands.Settings
{
    [Serializable]
    public class SaveSettingsException : Exception
    {
        public SaveSettingsException(Exception? innerException)
            : base(message: null, innerException)
        {
        }

        protected SaveSettingsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
