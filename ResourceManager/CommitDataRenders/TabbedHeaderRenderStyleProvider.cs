using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;

namespace ResourceManager.CommitDataRenders
{
    public sealed class TabbedHeaderRenderStyleProvider : IHeaderRenderStyleProvider
    {
        private readonly IReadOnlyList<int> _tabStops;

        public TabbedHeaderRenderStyleProvider()
        {
            var strings = new[]
            {
                Strings.Author,
                Strings.AuthorDate,
                Strings.Committer,
                Strings.CommitDate,
                Strings.CommitHash,
                Strings.GetChildren(10), // assume text for plural case is longer
                Strings.GetParents(10)
            };

            var tabStop = strings
                .Select(s => TextRenderer.MeasureText(s + "  ", AppSettings.Font).Width)
                .Max();

            // simulate a two column layout even when there's more then one tab used
            _tabStops = new[] { tabStop, tabStop + 1, tabStop + 2, tabStop + 3 };
        }

        public Font GetFont(Graphics g) => AppSettings.Font;

        public int GetMaxWidth() => 16;

        public IEnumerable<int> GetTabStops() => _tabStops;
    }
}