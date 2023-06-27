using System.Runtime.Serialization;

namespace GitCommands.Git
{
    [Serializable]
    public class RefsWarningException : GitException
    {
        public RefsWarningException(string message)
            : base(message)
        {
        }

        public RefsWarningException(string message, Exception? inner)
            : base(message, inner)
        {
        }

        protected RefsWarningException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
