using System.Reflection;
using System.Text.RegularExpressions;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using Microsoft;
using Microsoft.VisualStudio.Threading;

namespace GitUI.AutoCompletion
{
    public class CommitAutoCompleteProvider : IAutoCompleteProvider
    {
        private static readonly Lazy<Dictionary<string, Regex>> _regexes = new(ParseRegexes);
        private readonly Func<IGitModule> _getModule;
        private readonly GetAllChangedFilesOutputParser _getAllChangedFilesOutputParser;

        public CommitAutoCompleteProvider(Func<IGitModule> getModule)
        {
            _getModule = getModule;
            _getAllChangedFilesOutputParser = new GetAllChangedFilesOutputParser(getModule);
        }

        public async Task<IEnumerable<AutoCompleteWord>> GetAutoCompleteWordsAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            HashSet<string> autoCompleteWords = [];

            IGitModule module = GetModule();
            ArgumentString cmd = Commands.GetAllChangedFiles(true, UntrackedFilesMode.Default, noLocks: true);
            ExecutionResult result = await module.GitExecutable.ExecuteAsync(cmd, throwOnErrorExit: false, cancellationToken: cancellationToken);
            if (!result.ExitedSuccessfully)
            {
                // Try again
                result = await module.GitExecutable.ExecuteAsync(cmd, throwOnErrorExit: false, cancellationToken: cancellationToken);
            }

            if (!result.ExitedSuccessfully)
            {
                // Failed after retry, do not bother
                return Enumerable.Empty<AutoCompleteWord>();
            }

            string output = result.StandardOutput;
            IReadOnlyList<GitItemStatus> changedFiles = _getAllChangedFilesOutputParser.Parse(output);
            foreach (GitItemStatus file in changedFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                autoCompleteWords.Add(Path.GetFileName(file.Name));

                if (file.IsSubmodule)
                {
                    continue;
                }

                Regex regex = GetRegexForExtension(Path.GetExtension(file.Name) ?? "");

                if (regex is not null)
                {
                    // HACK: need to expose require methods at IGitModule level
                    string text = await GetChangedFileTextAsync((GitModule)module, file);
                    MatchCollection matches = regex.Matches(text);
                    foreach (Match match in matches)
                    {
                        // Skip first group since it always contains the entire matched string (regardless of capture groups)
                        foreach (Group group in match.Groups.OfType<Group>().Skip(1))
                        {
                            foreach (Capture capture in group.Captures)
                            {
                                autoCompleteWords.Add(capture.Value);
                            }
                        }
                    }
                }

                autoCompleteWords.Add(Path.GetFileNameWithoutExtension(file.Name));
                if (!string.IsNullOrWhiteSpace(file.OldName))
                {
                    autoCompleteWords.Add(Path.GetFileNameWithoutExtension(file.OldName));
                    autoCompleteWords.Add(Path.GetFileName(file.OldName));
                }
            }

            return autoCompleteWords.Select(w => new AutoCompleteWord(w));
        }

        private IGitModule GetModule()
        {
            IGitModule module = _getModule();

            if (module is null)
            {
                throw new ArgumentException($"Require a valid instance of {nameof(IGitModule)}");
            }

            return module;
        }

        private static Regex? GetRegexForExtension(string extension)
        {
            return _regexes.Value.TryGetValue(extension, out Regex? value) ? value : null;
        }

        private static IEnumerable<string> ReadOrInitializeAutoCompleteRegexes()
        {
            string? appDataPath = AppSettings.ApplicationDataPath.Value;

            Validates.NotNull(appDataPath);

            string path = Path.Combine(appDataPath, "AutoCompleteRegexes.txt");

            if (File.Exists(path))
            {
                return File.ReadLines(path);
            }

            Assembly entryAssembly = Assembly.GetEntryAssembly();

            if (entryAssembly is null)
            {
                // This will be null during integration tests, for example
                return Enumerable.Empty<string>();
            }

            Stream? s = entryAssembly.GetManifestResourceStream("GitExtensions.AutoCompleteRegexes.txt");

            if (s is null)
            {
                throw new NotImplementedException("Please add AutoCompleteRegexes.txt file into .csproj");
            }

            using StreamReader sr = new(s);
            return sr.ReadToEnd().Split(Delimiters.LineFeedAndCarriageReturn, StringSplitOptions.RemoveEmptyEntries);
        }

        private static Dictionary<string, Regex> ParseRegexes()
        {
            IEnumerable<string> autoCompleteRegexes = ReadOrInitializeAutoCompleteRegexes();

            Dictionary<string, Regex> regexes = [];

            foreach (string line in autoCompleteRegexes)
            {
                int i = line.IndexOf('=');
                string extensionStr = line[..i];
                string regexStr = line[(i + 1)..].Trim();

                IEnumerable<string> extensions = extensionStr.LazySplit(',').Select(s => s.Trim()).Distinct();
                Regex regex = new(regexStr, RegexOptions.Compiled);

                foreach (string extension in extensions)
                {
                    regexes.Add(extension, regex);
                }
            }

            return regexes;
        }

        private static async Task<string?> GetChangedFileTextAsync(IGitModule module, GitItemStatus file)
        {
            if (file.IsTracked)
            {
                Patch? changes = null;
                try
                {
                    changes = await module.GetCurrentChangesAsync(file.Name, file.OldName, file.Staged == StagedStatus.Index, "-U5", noLocks: true)
                        .ConfigureAwait(false);
                }
                catch
                {
                    // Ignore errors, this can be very repetitive
                }

                if (changes is not null)
                {
                    return changes.Text;
                }

                string? content = await module.GetFileContentsAsync(file).ConfigureAwaitRunInline();
                if (content is not null)
                {
                    return content;
                }
            }

            // Try to read the contents of the file: if it cannot be read, skip the operation silently.
            try
            {
                using StreamReader reader = File.OpenText(Path.Combine(module.WorkingDir, file.Name));
                return await reader.ReadToEndAsync();
            }
            catch
            {
                return "";
            }
        }
    }
}
