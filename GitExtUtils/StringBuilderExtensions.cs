using System.Text;

namespace GitExtUtils
{
    public static class StringBuilderExtensions
    {
        private static readonly char[] _whiteSpaceChars = { ' ', '\r', '\n', '\t' };

        public static StringBuilder AppendQuoted(this StringBuilder builder, string s)
        {
            if (NeedsEscaping())
            {
                builder.Append('"').Append(s).Append('"');
            }
            else
            {
                builder.Append(s);
            }

            return builder;

            bool NeedsEscaping()
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    // Quote empty or white space strings
                    return true;
                }

                if (s.IndexOfAny(_whiteSpaceChars) == -1)
                {
                    // Doesn't contain any white space
                    return false;
                }

                if (s.Length > 1 && s[0] == '"' && s[s.Length - 1] == '"')
                {
                    // String is already quoted
                    return false;
                }

                return true;
            }
        }
    }
}
