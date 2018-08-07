using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using GitUIPluginInterfaces.RepositoryHosts;

namespace GitUI.CommandsDialogs.RepoHosting
{
    internal static class DiscussionHtmlCreator
    {
        public static string CreateFor(IPullRequestInformation currentPullRequestInfo, List<IDiscussionEntry> entries = null)
        {
            var html = new StringBuilder();
            AddLine(html, "<html><body><style type='text/css'>");
            html.Append(CssData);
            AddLine(html, "</style>");

            if (entries != null)
            {
                foreach (var entry in entries)
                {
                    var cde = entry as ICommitDiscussionEntry;

                    AddLine(html, "<div class='entry {0}'>", cde == null ? "commentEntry" : " commitEntry");

                    AddLine(html, "<div class='heading'>");
                    AddLine(html, "<span class='created'>{0}</span>\r\n", entry.Created);
                    AddLine(html, "<span class='author'>{0}</span>\r\n", entry.Author);
                    if (cde != null)
                    {
                        AddLine(html, "<span class='commit'>Commit:  {0}</span>\r\n", cde.Sha);
                    }

                    AddLine(html, "</div>");
                    AddLine(html, "<div class='commentBody'>{0}</div>\r\n", entry.Body);

                    AddLine(html, "</div>");
                }
            }

            AddLine(html, "</body></html>");

            return html.ToString();
        }

        private static void AddLine(StringBuilder html, string input, params object[] p)
        {
            html.AppendFormat(input + "\r\n", (from el in p select (el == null) ? "[UNKNOWN]" : el.ToString().Replace("\r", "").Replace("\n", "<br/>\n").Replace("\"", "&quot;")).ToArray());
        }

        private static string CssData
        {
            get
            {
                if (_cssData == null)
                {
                    _cssData = _cssDataRaw;
                    foreach (var elem in SystemInfoReplacement)
                    {
                        _cssData = _cssData.Replace(elem.Key, elem.Value);
                    }
                }

                return _cssData;
            }
        }

        private static List<KeyValuePair<string, string>> _systemInfoReplacement;

        private static IEnumerable<KeyValuePair<string, string>> SystemInfoReplacement
        {
            get
            {
                if (_systemInfoReplacement == null)
                {
                    var props = typeof(SystemColors).GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty).ToList();

                    var kvps = from prop in props
                               where prop.PropertyType == typeof(Color)
                               let c = (Color)prop.GetValue(null, null)
                               select new KeyValuePair<string, string>("SC." + prop.Name, string.Format("#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B));

                    _systemInfoReplacement = kvps.ToList();

                    // TODO: is it safe to rename the keys ('SF.DialogFont', 'SF.DialogFontSize') to 'SF.MessageBoxFont' or not?
                    _systemInfoReplacement.Add(new KeyValuePair<string, string>("SF.DialogFont", SystemFonts.MessageBoxFont.Name));
                    _systemInfoReplacement.Add(new KeyValuePair<string, string>("SF.DialogFontSize", string.Format("{0}pt", SystemFonts.MessageBoxFont.SizeInPoints)));

                    _systemInfoReplacement.Sort((p1, p2) => p2.Key.CompareTo(p1.Key)); // Required.
                }

                return _systemInfoReplacement;
            }
        }

        private static string _cssData;
        private const string _cssDataRaw = @"
body {
    background: SC.Control;
    color: SC.ControlText;
    font-family: SF.DialogFont;
    font-size: SF.DialogFontSize;
    padding: 0px;
    margin: 0px;
}

div
{
    margin: 0px;
    padding: 0px;
}

.entry 
{
    margin-bottom: 1em;
}

.heading
{
    background-color: SC.ControlLight;
    border-bottom: 1px solid SC.ControlText;
}

.commentEntry .author
{
    font-weight: bold;
    color: SC.Highlight;
}

.commitEntry .author
{
    font-weight: bold;
    color: red;
}

.created
{
    float: right;
}


.commit
{
    font-size: 7pt;
    display: block;
}

.commentBody
{
}
";
    }
}
