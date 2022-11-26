using GitCommands.Config;
using GitExtUtils;
using Microsoft.VisualStudio.Threading;

namespace GitCommands
{
    public class CustomDiffMergeToolCache
    {
        private CustomDiffMergeToolCache(bool isDiff)
        {
            _isDiff = isDiff;
        }

        private readonly bool _isDiff;
        private readonly SemaphoreSlim _mutex = new(1);

        private IEnumerable<string>? _tools;

        public static CustomDiffMergeToolCache DiffToolCache { get; } = new(true);
        public static CustomDiffMergeToolCache MergeToolCache { get; } = new(false);

        /// <summary>
        /// Clear the existing caches (await current calculation to finsh first)
        /// </summary>
        public async Task ClearAsync()
        {
            await _mutex.WaitAsync().ConfigureAwait(false);

            try
            {
                _tools = null;
            }
            finally
            {
                _mutex.Release();
            }
        }

        /// <summary>
        /// Load the available DiffMerge tools and apply to the menus
        /// </summary>
        /// <param name="module">The Git module</param>
        /// <param name="delay">The delay before starting the operation</param>
        public async Task<IEnumerable<string>> GetToolsAsync(GitModule module, int delay, CancellationToken cancellationToken)
        {
            if (_tools is not null)
            {
                return _tools;
            }

            // The command will compete with other resources, avoid delaying startup
            // Do not bother with cancel tokens
            await Task.Delay(delay, cancellationToken);

            await _mutex.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                if (_tools is null)
                {
                    await TaskScheduler.Default;
                    cancellationToken.ThrowIfCancellationRequested();

                    var toolKey = _isDiff ? SettingKeyString.DiffToolKey : SettingKeyString.MergeToolKey;
                    var defaultTool = module.GetEffectiveSetting(toolKey);
                    string output = module.GetCustomDiffMergeTools(_isDiff, cancellationToken);
                    _tools = ParseCustomDiffMergeTool(output, defaultTool);
                }
            }
            catch
            {
                // No action.
            }
            finally
            {
                if (_tools is null)
                {
                    // Parsing has failed, just provide an empty list, no user notification
                    _tools = Array.Empty<string>();
                }

                _mutex.Release();
            }

            return _tools;
        }

        /// <summary>
        /// Parse the output from 'git difftool --tool-help'.
        /// </summary>
        /// <param name="output">The output string.</param>
        /// <returns>list with tool names.</returns>
        private IEnumerable<string> ParseCustomDiffMergeTool(string output, string defaultTool)
        {
            List<string> tools = new();

            // Simple parsing of the textual output opposite to porcelain format
            // https://github.com/git/git/blob/main/git-mergetool--lib.sh#L298
            // An alternative is to parse "git config --get-regexp difftool'\..*\.cmd'" and see show_tool_names()

            // The sections to parse in the text has a 'header', then break parsing at first non match

            foreach (var l in output.LazySplit('\n'))
            {
                if (l == "The following tools are valid, but not currently available:")
                {
                    // No more usable tools
                    break;
                }

                if (!l.StartsWith("\t\t"))
                {
                    continue;
                }

                // two tabs, then tool name, cmd (if split in 3) in second
                // cmd is unreliable for diff and not needed but could be used for mergetool special handling
                string[] delimit = { " ", ".cmd" };
                var tool = l.Substring(2).Split(delimit, 2, StringSplitOptions.None);
                if (tool.Length == 0)
                {
                    continue;
                }

                // Ignore (known) tools that must run in a terminal
                string[] ignoredTools = { "vimdiff", "vimdiff1", "vimdiff2", "vimdiff3" };
                var toolName = tool[0];
                if (!string.IsNullOrWhiteSpace(toolName) && !tools.Contains(toolName) && !ignoredTools.Contains(toolName))
                {
                    tools.Add(toolName);
                }
            }

            // Return the default tool first
            return tools.OrderBy(tool => tool != defaultTool).ThenBy(tool => tool);
        }

        internal TestAccessor GetTestAccessor() => new(this);

        internal readonly struct TestAccessor
        {
            private readonly CustomDiffMergeToolCache _cache;

            public TestAccessor(CustomDiffMergeToolCache cache)
            {
                _cache = cache;
            }

            public IEnumerable<string> ParseCustomDiffMergeTool(string output, string defaultTool)
                => _cache.ParseCustomDiffMergeTool(output, defaultTool);
        }
    }
}
