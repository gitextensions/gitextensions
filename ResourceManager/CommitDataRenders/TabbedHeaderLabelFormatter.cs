using System.Net;

namespace ResourceManager.CommitDataRenders
{
    /// <summary>
    /// Formats the commit information heading labels with tabs.
    /// </summary>
    public sealed class TabbedHeaderLabelFormatter : IHeaderLabelFormatter
    {
        public string FormatLabel(string label, int desiredLength)
        {
            return FillToLength(WebUtility.HtmlEncode(label) + ":");

            string FillToLength(string input)
            {
                const int tabSize = 4;

                if (input.Length < desiredLength)
                {
                    int l = desiredLength - input.Length;
                    return input + new string('\t', (l / tabSize) + (l % tabSize == 0 ? 0 : 1));
                }

                return input;
            }
        }
    }
}