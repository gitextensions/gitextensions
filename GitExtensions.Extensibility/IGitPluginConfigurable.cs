using System.Collections.Generic;
using GitExtensions.Extensibility.Settings;

namespace GitExtensions.Extensibility
{
    /// <summary>
    /// Interface to implement if you wish to have plugin settings.
    /// </summary>
    public interface IGitPluginConfigurable
    {
        IEnumerable<ISetting> GetSettings();
    }
}
