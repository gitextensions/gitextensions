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
            return FillToLength(WebUtility.HtmlEncode(label) + ":", desiredLength);
        }

        public string FormatLabelPlain(string label, int desiredLength)
        {
            return FillToLength(label + ":", desiredLength);
        }


        private static string FillToLength(string input, int length)
        {
            // length
            const int tabsize = 4;
            if (input.Length < length)
            {
                int l = length - input.Length;
                return input + new string('\t', l / tabsize + (l % tabsize == 0 ? 0 : 1));
            }

            return input;
        }
    }
}