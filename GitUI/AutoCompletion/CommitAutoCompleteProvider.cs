using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GitCommands;

namespace GitUI.AutoCompletion
{
    public class CommitAutoCompleteProvider : IAutoCompleteProvider
    {
        private static readonly Lazy<Dictionary<string, Regex>> s_regexes = new Lazy<Dictionary<string, Regex>>(ParseRegexes);
        private readonly GitModule _module;

        public CommitAutoCompleteProvider (GitModule module)
        {
            _module = module;
        }

        public Task<IEnumerable<AutoCompleteWord>> GetAutoCompleteWords (CancellationTokenSource cts)
        {
            var cancellationToken = cts.Token;

            return Task.Factory.StartNew(
                    () =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var autoCompleteWords = new HashSet<string>();

                        foreach (var file in _module.GetAllChangedFiles())
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            var regex = GetRegexForExtension(Path.GetExtension(file.Name));

                            if (regex != null)
                            {
                                var text = GetChangedFileText(_module, file);
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

                        return autoCompleteWords.Select(w => new AutoCompleteWord(w));
                    }, cancellationToken);
        }

        private static Regex GetRegexForExtension (string extension)
        {
            return s_regexes.Value.ContainsKey(extension) ? s_regexes.Value[extension] : null;
        }

        private static IEnumerable<string> ReadOrInitializeAutoCompleteRegexes()
        {
            var path = Path.Combine(AppSettings.ApplicationDataPath.Value, "AutoCompleteRegexes.txt");

            if (File.Exists(path))
                return File.ReadLines(path);

            Stream s = Assembly.GetEntryAssembly().GetManifestResourceStream("GitExtensions.AutoCompleteRegexes.txt");
            if (s == null)
                {
                    throw new NotImplementedException("Please add AutoCompleteRegexes.txt file into .csproj");
            }
            using (var sr = new StreamReader (s))
            {
                return sr.ReadToEnd ().Split (new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            }
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