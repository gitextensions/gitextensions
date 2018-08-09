using System.Text;
using JetBrains.Annotations;

namespace GitCommands
{
    public static class StringBuilderExtensions
    {
        private static readonly char[] _whiteSpaceChars = { ' ', '\r', '\n', '\t' };

        [NotNull]
        public static StringBuilder AppendQuoted([NotNull] this StringBuilder builder, [NotNull] string s)
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