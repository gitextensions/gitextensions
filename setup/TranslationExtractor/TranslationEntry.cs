namespace TranslationExtractor;

/// <summary>
///  Represents a single translatable entry to be written to an XLIFF file.
/// </summary>
/// <param name="Category">The category name (class name), maps to the XLIFF file/@original attribute.</param>
/// <param name="Name">The item name (field/control name), first part of the trans-unit/@id.</param>
/// <param name="Property">The property name (e.g. "Text", "ToolTipText"), second part of the trans-unit/@id.</param>
/// <param name="Source">The English source text.</param>
internal sealed record TranslationEntry(string Category, string Name, string Property, string Source)
{
    /// <summary>The trans-unit id, formatted as "Name.Property".</summary>
    public string Id => $"{Name}.{Property}";
}
