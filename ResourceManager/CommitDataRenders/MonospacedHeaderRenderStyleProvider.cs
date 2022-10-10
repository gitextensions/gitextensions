using GitCommands;
using GitUI;

namespace ResourceManager.CommitDataRenders
{
    /// <summary>
    /// Renders commit information in a tabular format with data columns aligned with spaces.
    /// </summary>
    public sealed class MonospacedHeaderRenderStyleProvider : IHeaderRenderStyleProvider
    {
        private readonly int _maxLength;

        public MonospacedHeaderRenderStyleProvider()
        {
            var strings = new[]
            {
                TranslatedStrings.Author,
                TranslatedStrings.AuthorDate,
                TranslatedStrings.Committer,
                TranslatedStrings.CommitDate,
                TranslatedStrings.CommitHash,
                TranslatedStrings.GetChildren(10), // assume text for plural case is longer
                TranslatedStrings.GetParents(10)
            };

            _maxLength = strings.Select(s => s.Length).Max() + 2;
        }

        public Font GetFont(Graphics g)
        {
            if (!AppSettings.Font.IsFixedWidth(g))
            {
                return new Font(FontFamily.GenericMonospace, AppSettings.Font.Size);
            }

            return AppSettings.Font;
        }

        public int GetMaxWidth() => _maxLength;

        public IEnumerable<int> GetTabStops() => Enumerable.Empty<int>();
    }
}
