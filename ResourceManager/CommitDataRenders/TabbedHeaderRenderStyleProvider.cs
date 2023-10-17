using GitCommands;

namespace ResourceManager.CommitDataRenders
{
    public sealed class TabbedHeaderRenderStyleProvider : IHeaderRenderStyleProvider
    {
        private readonly IReadOnlyList<int> _tabStops;

        public TabbedHeaderRenderStyleProvider()
        {
            string[] strings = new[]
            {
                TranslatedStrings.Author,
                TranslatedStrings.AuthorDate,
                TranslatedStrings.Committer,
                TranslatedStrings.CommitDate,
                TranslatedStrings.CommitHash,
                TranslatedStrings.GetChildren(10), // assume text for plural case is longer
                TranslatedStrings.GetParents(10)
            };

            int tabStop = strings
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
