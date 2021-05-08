using System;
using System.Collections.Generic;
using System.Text;

namespace GitUI.Avatars
{
    /// <summary>
    /// A helper that is used to process templates like "Hello {variable}!".
    /// </summary>
    public static class TemplateFormatter
    {
        /// <summary>
        /// Creates a formatter based on a <paramref name="template"/> and a <paramref name="valueMapperProvider"/>.
        /// </summary>
        /// <param name="template">The template used to generate the formatter.</param>
        /// <param name="valueMapperProvider">Supplies requested values by providing conversion functions from <typeparamref name="TInput"/> to string.</param>
        /// <remarks>
        /// The returned formatter can be reused to format different inputs. It basically generates
        /// a custom StringBuilder from the template.
        /// </remarks>
        public static Func<TInput, string> Create<TInput>(string template, Func<string, Func<TInput, string?>> valueMapperProvider)
        {
            var formatParts = new List<Action<StringBuilder, TInput>>();
            var position = 0;

            while (true)
            {
                var variableStartIndex = template.IndexOf('{', position);
                var fixIndex = variableStartIndex < 0 ? template.Length : variableStartIndex;

                var fixValue = template.Substring(position, fixIndex - position);
                formatParts.Add((sb, _) => sb.Append(fixValue));

                if (variableStartIndex < 0)
                {
                    break;
                }

                position = variableStartIndex + 1;

                var variableEndIndex = template.IndexOf('}', position);

                if (variableEndIndex < 0)
                {
                    break;
                }

                position = variableEndIndex + 1;

                var variableName = template.Substring(variableStartIndex + 1, variableEndIndex - variableStartIndex - 1);
                var inputParser = valueMapperProvider(variableName);
                formatParts.Add((sb, i) => sb.Append(inputParser(i)));
            }

            return i => FormatExecute(i, formatParts);
        }

        /// <summary>
        /// Format a given input using multiple format parts.
        /// </summary>
        private static string FormatExecute<TInput>(TInput input, IEnumerable<Action<StringBuilder, TInput>> formatParts)
        {
            var sb = new StringBuilder();

            foreach (var formatter in formatParts)
            {
                formatter(sb, input);
            }

            return sb.ToString();
        }
    }
}
