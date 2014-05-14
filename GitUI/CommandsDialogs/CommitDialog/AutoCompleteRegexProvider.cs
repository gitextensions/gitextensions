using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace GitUI.CommandsDialogs.CommitDialog
{
    public static class AutoCompleteRegexProvider
    {
        private static readonly Dictionary<string, Regex> _regexes = new Dictionary<string, Regex>(); 

        public static void Initialize ()
        {
            foreach (var line in File.ReadLines (Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "AutoCompleteRegexes.txt")))
            {
                var i = line.IndexOf('=');
                var extensionStr = line.Substring(0, i);
                var regexStr = line.Substring(i + 1).Trim();

                var extensions = extensionStr.Split(',').Select(s => s.Trim()).Distinct();
                var regex = new Regex(regexStr, RegexOptions.Compiled);

                foreach (var extension in extensions)
                    _regexes.Add(extension, regex);
            }
        }

        public static Regex GetRegexForExtension (string extension)
        {
            return _regexes.ContainsKey(extension) ? _regexes[extension] : null;
        }
    }
}
