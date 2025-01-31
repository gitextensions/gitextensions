#nullable enable

namespace GitExtensions.Extensibility.Configurations;

public interface IPersistentConfigValueStore : IConfigValueStore
{
    /// <summary>
    ///  Saves pending changes.
    /// </summary>
    void Save();
}
