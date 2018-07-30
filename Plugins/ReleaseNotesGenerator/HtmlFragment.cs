// Sample class for Copying and Pasting HTML fragments to and from the clipboard.
//
// Mike Stall. http://blogs.msdn.com/jmstall
//

using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ReleaseNotesGenerator
{
    /// <summary>
    /// Helper class to decode HTML from the clipboard.
    /// See http://blogs.msdn.com/jmstall/archive/2007/01/21/html-clipboard.aspx for details.
    /// </summary>
    internal class HtmlFragment
    {
        #region Read and decode from clipboard

        /// <summary>
        /// Get a HTML fragment from the clipboard.
        /// </summary>
        /// <example>
        ///    string html = "<b>Hello!</b>";
        ///    HtmlFragment.CopyToClipboard(html);
        ///    HtmlFragment html2 = HtmlFragment.FromClipboard();
        ///    Debug.Assert(html2.Fragment == html);
        /// </example>
        public static HtmlFragment FromClipboard()
        {
            string rawClipboardText = Clipboard.GetText(TextDataFormat.Html);
            var h = new HtmlFragment(rawClipboardText);
            return h;
        }

        /// <summary>
        /// Create an HTML fragment decoder around raw HTML text from the clipboard.
        /// This text should have the header.
        /// </summary>
        /// <param name="rawClipboardText">raw html text, with header.</param>
        public HtmlFragment(string rawClipboardText)
        {
            // This decodes CF_HTML, which is an entirely text format using UTF-8.
            // Format of this header is described at:
            // http://msdn.microsoft.com/library/default.asp?url=/workshop/networking/clipboard/htmlclipboard.asp

            // Note the counters are byte counts in the original string, which may be Ansi. So byte counts
            // may be the same as character counts (since sizeof(char) == 1).
            // But System.String is unicode, and so byte counts are no longer the same as character counts,
            // (since sizeof(wchar) == 2).
            int startHtml = 0;
            int startFragment = 0;

            Match m;

            var r = new Regex(
                "([a-zA-Z]+):(.+?)[\r\n]",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            for (m = r.Match(rawClipboardText); m.Success; m = m.NextMatch())
            {
                string key = m.Groups[1].Value.ToLower();
                string val = m.Groups[2].Value;

                switch (key)
                {
                    // Version number of the clipboard. Starting version is 0.9.
                    case "version":
                        Version = val;
                        break;

                    // Byte count from the beginning of the clipboard to the start of the context, or -1 if no context
                    case "starthtml":
                        if (startHtml != 0)
                        {
                            throw new FormatException("StartHtml is already declared");
                        }

                        startHtml = int.Parse(val);
                        break;

                    // Byte count from the beginning of the clipboard to the end of the context, or -1 if no context.
                    case "endhtml":
                        if (startHtml == 0)
                        {
                            throw new FormatException("StartHTML must be declared before endHTML");
                        }

                        int endHtml = int.Parse(val);

                        Context = rawClipboardText.Substring(startHtml, endHtml - startHtml);
                        break;

                    // Byte count from the beginning of the clipboard to the start of the fragment.
                    case "startfragment":
                        if (startFragment != 0)
                        {
                            throw new FormatException("StartFragment is already declared");
                        }

                        startFragment = int.Parse(val);
                        break;

                    // Byte count from the beginning of the clipboard to the end of the fragment.
                    case "endfragment":
                        if (startFragment == 0)
                        {
                            throw new FormatException("StartFragment must be declared before EndFragment");
                        }

                        int endFragment = int.Parse(val);
                        Fragment = rawClipboardText.Substring(startFragment, endFragment - startFragment);
                        break;

                    // Optional Source URL, used for resolving relative links.
                    case "sourceurl":
                        SourceUrl = new Uri(val);
                        break;
                }
            }

            if (Context == null && Fragment == null)
            {
                throw new FormatException("No data specified");
            }
        }

        // Data. See properties for descriptions.

        /// <summary>
        /// Get the Version of the html. Usually something like "1.0".
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// Get the full text (context) of the HTML fragment. This includes tags that the HTML is enclosed in.
        /// May be null if context is not specified.
        /// </summary>
        public string Context { get; }

        /// <summary>
        /// Get just the fragment of HTML text.
        /// </summary>
        public string Fragment { get; }

        /// <summary>
        /// Get the Source URL of the HTML. May be null if no SourceUrl is specified. This is useful for resolving relative urls.
        /// </summary>
        public Uri SourceUrl { get; }

        #endregion

        #region Write to Clipboard

        // Helper to convert an integer into an 8 digit string.
        // String must be 8 characters, because it will be used to replace an 8 character string within a larger string.
        internal static string To8DigitString(int x) => $"{x:00000000}";

        /// <summary>
        /// Clears clipboard and copy a HTML fragment to the clipboard, providing additional meta-information.
        /// </summary>
        /// <param name="htmlFragment">a html fragment</param>
        /// <param name="title">optional title of the HTML document (can be null)</param>
        /// <param name="sourceUri">optional Source URL of the HTML document, for resolving relative links (can be null)</param>
        public static void CopyToClipboard(string htmlFragment, string title = null, Uri sourceUri = null)
        {
            var dataObject = CreateHtmlFormatClipboardDataObject(htmlFragment, title, sourceUri);

            Clipboard.Clear();
            Clipboard.SetDataObject(dataObject);

            // now the clipboard can be pasted as text (HTML code) to text editor
            // or as table to MS Word or LibreOffice Writer
        }

        internal static DataObject CreateHtmlFormatClipboardDataObject(string htmlFragment, string title = "From Clipboard", Uri sourceUri = null)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // Builds the CF_HTML header. See format specification here:
            // http://msdn.microsoft.com/library/default.asp?url=/workshop/networking/clipboard/htmlclipboard.asp

            // The string contains index references to other spots in the string, so we need placeholders so we can compute the offsets.
            // The <<<<<<<_ strings are just placeholders. We'll back patch them actual values afterwards.
            // The string layout (<<<) also ensures that it can't appear in the body of the html because the <
            // character must be escaped.
            const string header = "Version:0.9\r\n" +
                                  "StartHTML:<<<<<<<1\r\n" +
                                  "EndHTML:<<<<<<<2\r\n" +
                                  "StartFragment:<<<<<<<3\r\n" +
                                  "EndFragment:<<<<<<<4\r\n";

            sb.Append(header);

            if (sourceUri != null)
            {
                sb.AppendFormat("SourceURL:{0}", sourceUri);
            }

            int startHtml = sb.Length;

            const string pre =
                "<html><body>\r\n" +
                "<!--StartFragment-->";
            sb.Append(pre);
            int fragmentStart = sb.Length;

            sb.Append(htmlFragment);
            int fragmentEnd = sb.Length;

            const string post =
                "<!--EndFragment-->\r\n" +
                "</body></html>";
            sb.Append(post);
            int endHtml = sb.Length;

            // Back patch offsets
            sb.Replace("<<<<<<<1", To8DigitString(startHtml));
            sb.Replace("<<<<<<<2", To8DigitString(endHtml));
            sb.Replace("<<<<<<<3", To8DigitString(fragmentStart));
            sb.Replace("<<<<<<<4", To8DigitString(fragmentEnd));

            // Finally copy to clipboard.
            // http://stackoverflow.com/questions/13332377/how-to-set-html-text-in-clipboard
            string data = sb.ToString();
            var dataObject = new DataObject();
            dataObject.SetText(data, TextDataFormat.Html);
            dataObject.SetText(htmlFragment, TextDataFormat.Text);

            return dataObject;
        }

        #endregion
    }
}