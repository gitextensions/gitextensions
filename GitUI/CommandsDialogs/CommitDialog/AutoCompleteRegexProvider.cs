using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using GitCommands;

namespace GitUI.CommandsDialogs.CommitDialog
{
    public static class AutoCompleteRegexProvider
    {
        private static readonly Lazy<Dictionary<string, Regex>> _regexes = new Lazy<Dictionary<string, Regex>>(ParseRegexes);

        private static IEnumerable<string> ReadOrInitializeAutoCompleteRegexes ()
        {
            var path = Path.Combine(AppSettings.ApplicationDataPath.Value, "AutoCompleteRegexes.txt");

            if (!File.Exists(path))
                using (var sr = new StreamReader(Assembly.GetEntryAssembly().GetManifestResourceStream("GitExtensions.AutoCompleteRegexes.txt")))
                    File.WriteAllText(path, sr.ReadToEnd());

            return File.ReadLines(path);
        }

        private static Dictionary<string, Regex> ParseRegexes ()
        {
            var autoCompleteRegexes = ReadOrInitializeAutoCompleteRegexes();
            
            var regexes = new Dictionary<string, Regex>();

            foreach (var line in autoCompleteRegexes)
            {
                var i = line.IndexOf('=');
                var extensionStr = line.Substring(0, i);
                var regexStr = line.Substring(i + 1).Trim();

                var extensions = extensionStr.Split(',').Select(s => s.Trim()).Distinct();
                var regex = new Regex(regexStr, RegexOptions.Compiled);

                foreach (var extension in extensions)
                    regexes.Add(extension, regex);
            }

            return regexes;
        }

        public static Regex GetRegexForExtension (string extension)
        {
            return _regexes.Value.ContainsKey(extension) ? _regexes.Value[extension] : null;
        }
    }
}
