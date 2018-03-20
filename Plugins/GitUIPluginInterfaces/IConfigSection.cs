using System.Collections.Generic;

namespace GitUIPluginInterfaces
{
    public interface IConfigSection
    {
        string SectionName { get; set; }
        string SubSection { get; set; }

        IDictionary<string, IReadOnlyList<string>> AsDictionary();
        void AddValue(string key, string value);
        bool Equals(IConfigSection other);
        string GetValue(string key, string defaultValue = "");
        IReadOnlyList<string> GetValues(string key);
        bool HasValue(string key);
        void SetValue(string key, string value);
    }
}