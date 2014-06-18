using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GitCommands;
using GitUI.SpellChecker;

namespace GitUI.CommandsDialogs.CommitDialog
{
    public static class AutoCompleteProvider
    {
        private static readonly Lazy<Dictionary<string, Regex>> _regexes = new Lazy<Dictionary<string, Regex>>(ParseRegexes);

        public static Task<List<AutoCompleteWord>> GetAutoCompleteList (GitModule module, CancellationTokenSource cts, Action<IEnumerable<string>> addAutoCompleteWordsToDictionary)
        {
            var cancellationToken = cts.Token;

            return Task.Factory.StartNew(
                    () =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var autoCompleteWords = new HashSet<string>();

                        foreach (var file in module.GetAllChangedFiles())
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            var regex = GetRegexForExtension(Path.GetExtension(file.Name));

                            if (regex != null)
                            {
                                var text = GetChangedFileText(module, file);
                                var matches = regex.Matches(text);
                                foreach (Match match in matches)
                                        // Skip first group since it always contains the entire matched string (regardless of capture groups)
                                    foreach (Group @group in match.Groups.OfType<Group>().Skip(1))
                                        foreach (Capture capture in @group.Captures)
                                            autoCompleteWords.Add(capture.Value);
                            }

                            autoCompleteWords.Add(Path.GetFileNameWithoutExtension(file.Name));
                            autoCompleteWords.Add(Path.GetFileName(file.Name));
                            if (!string.IsNullOrWhiteSpace(file.OldName))
                            {
                                autoCompleteWords.Add(Path.GetFileNameWithoutExtension(file.OldName));
                                autoCompleteWords.Add(Path.GetFileName(file.OldName));
                            }
                        }

                        var autoCompleteList = autoCompleteWords.Select(w => new AutoCompleteWord(w)).ToList();
                        addAutoCompleteWordsToDictionary(autoCompleteList.Select(w => w.Word));
                        return autoCompleteList;
                    }, cancellationToken);
        }

        private static Regex GetRegexForExtension (string extension)
        {
            return _regexes.Value.ContainsKey(extension) ? _regexes.Value[extension] : null;
        }

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

        private static string GetChangedFileText (GitModule module, GitItemStatus file)
        {
            var changes = module.GetCurrentChanges(file.Name, file.OldName, file.IsStaged, "-U1000000", module.FilesEncoding);

            if (changes != null)
                return changes.Text;

            var content = module.GetFileContents(file);

            if (content != null)
                return content;

            return File.ReadAllText(Path.Combine(module.WorkingDir, file.Name));
        }
    }
}
