using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GitCommands;
using GitUI;

namespace ResourceManager.CommitDataRenders
{
    /// <summary>
    /// Renders commit information in a tabular format with data columns aligned with spaces.
    /// </summary>
    public sealed class MonospacedHeaderRenderStyleProvider : IHeaderRenderStyleProvider
    {
        public Font GetFont(Graphics g)
        {
            if (!AppSettings.Font.IsFixedWidth(g))
            {
                return new Font(FontFamily.GenericMonospace, AppSettings.Font.Size);
            }

            return AppSettings.Font;
        }

        public int GetMaxWidth()
        {
            var strings = new[]
            {
                Strings.GetAuthorText(),
                Strings.GetAuthorDateText(),
                Strings.GetCommitterText(),
                Strings.GetCommitDateText(),
                Strings.GetCommitHashText(),
                Strings.GetChildrenText(),
                Strings.GetParentsText()
            };

            var maxLegnth = strings.Select(s => s.Length).Max();
            return maxLegnth + 2;
        }

        public IEnumerable<int> GetTabStops()
        {
            return Enumerable.Empty<int>();
        }
    }
}