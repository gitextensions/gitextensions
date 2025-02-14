#nullable enable

using GitExtensions.Extensibility.Settings;

namespace GitExtensions.Extensibility.Configurations;

public interface IGitConfigSettingsGetter : ISettingsValueGetter
{
    /// <summary>
    ///  Retrieves all configured values of a git setting that accepts multiple values for the same key.
    /// </summary>
    /// <param name="setting">The git setting key.</param>
    /// <returns>The collection of all the <see cref="string"/> values.</returns>
    IReadOnlyList<string> GetValues(string setting);
}
