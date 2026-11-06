using System.Xml.Linq;

namespace GitExtensions.ParityInventory;

// parity-scaffolding: Reads the existing XLIFF catalog without a product-project dependency.
internal static class EnglishCatalog
{
    public static IReadOnlySet<string> Read(string path, string translationCategory)
    {
        XDocument document = XDocument.Load(path, LoadOptions.PreserveWhitespace);
        XElement? file = document.Descendants()
            .FirstOrDefault(element =>
                element.Name.LocalName == "file"
                && string.Equals((string?)element.Attribute("original"), translationCategory, StringComparison.Ordinal));
        if (file is null)
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        return file.Descendants()
            .Where(element => element.Name.LocalName == "trans-unit")
            .Select(element => (string?)element.Attribute("id"))
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Cast<string>()
            .ToHashSet(StringComparer.Ordinal);
    }
}
