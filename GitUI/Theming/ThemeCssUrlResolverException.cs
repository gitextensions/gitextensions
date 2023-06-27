using System.Runtime.Serialization;

namespace GitUI.Theming
{
    [Serializable]
    public class ThemeCssUrlResolverException : ThemeException
    {
        public ThemeCssUrlResolverException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ThemeCssUrlResolverException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
