namespace GitExtensions.Extensibility.Git;

public interface IGitCommandConfiguration
{
    /// <summary>
    ///  Registers <paramref name="configItem"/> against one or more command names.
    /// </summary>
    /// <param name="configItem">The config item to register.</param>
    /// <param name="commands">One or more command names to register this config item against.</param>
    void Add(GitConfigItem configItem, params string[] commands);

    /// <summary>
    ///  Retrieves the set of default config items for the given <paramref name="command"/>.
    /// </summary>
    /// <param name="command">The command to retrieve default config items for.</param>
    /// <returns>The default config items for <paramref name="command"/>.</returns>
    IReadOnlyList<GitConfigItem> Get(string command);
}
