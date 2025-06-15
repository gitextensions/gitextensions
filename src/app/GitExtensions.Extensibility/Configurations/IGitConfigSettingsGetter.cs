#nullable enable

using GitExtensions.Extensibility.Settings;

namespace GitExtensions.Extensibility.Configurations;

public interface IGitConfigSettingsGetter : ISettingsValueGetter
{
    /// <summary>
    ///  Enumerates all configured settings.
    /// </summary>
    IEnumerable<(string Setting, string Value)> GetAllValues();

    /// <summary>
    ///  Retrieves all configured values of a git setting that accepts multiple values for the same key.
    /// </summary>
    /// <param name="setting">The git setting key.</param>
    /// <returns>The collection of all the <see cref="string"/> values.</returns>
    IReadOnlyList<string> GetValues(string setting);

    /// <summary>
    ///  Splits the passed <paramref name="setting"/> identifier into section, optional subsection and value name.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="setting"/> does not contain any '.'</exception>
    static (string Section, string? Subsection, string Name) SplitSetting(string setting)
    {
        int sectionEnd = setting.IndexOf('.');
        int subsectionStart = sectionEnd + 1;
        int subsectionEnd = setting.LastIndexOf('.');
        int nameStart = subsectionEnd + 1;
        return sectionEnd < 0 ? throw new ArgumentException(@$"Invalid git setting ""{setting}"".")
            : subsectionStart <= subsectionEnd
                ? (setting[..sectionEnd], setting[subsectionStart..subsectionEnd], setting[nameStart..])
                : (setting[..sectionEnd], null, setting[nameStart..]);
    }
}
