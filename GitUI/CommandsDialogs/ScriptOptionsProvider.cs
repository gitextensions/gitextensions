using GitExtensions.Extensibility;
using GitUI.ScriptsEngine;

namespace GitUI.CommandsDialogs;

internal class ScriptOptionsProvider : IScriptOptionsProvider
{
    private const string _selectedFiles = "SelectedFiles";
    private const string _lineNumber = "LineNumber";

    private Func<IEnumerable<string>> _getSelectedFiles;
    private Func<IFullPathResolver> _getFullPathResolver;
    private Func<int?> _getCurrentLineNumber;

    public ScriptOptionsProvider(Func<IEnumerable<string>> getSelectedFiles, Func<IFullPathResolver> getFullPathResolver, Func<int?> getCurrentLineNumber)
    {
        _getSelectedFiles = getSelectedFiles;
        _getFullPathResolver = getFullPathResolver;
        _getCurrentLineNumber = getCurrentLineNumber;
    }

    public ScriptOptionsProvider(FileStatusList fileStatusList, Func<IFullPathResolver> getFullPathResolver, Func<int?> getCurrentLineNumber)
        : this(() => fileStatusList.SelectedItems.Select(item => item.Item.Name), getFullPathResolver, getCurrentLineNumber)
    {
    }

    IReadOnlyList<string> IScriptOptionsProvider.Options { get; } = new[] { _selectedFiles, _lineNumber };

    string? IScriptOptionsProvider.GetValue(string option)
    {
        switch (option)
        {
            case _selectedFiles:
                IEnumerable<string> selectedFiles = _getSelectedFiles();
                if (!selectedFiles.Any())
                {
                    return null;
                }

                IFullPathResolver fullPathResolver = _getFullPathResolver();
                return string.Join(" ", selectedFiles.Select(item => fullPathResolver.Resolve(item).QuoteForCommandLine()));
            case _lineNumber:
                int? line = _getCurrentLineNumber();
                return line?.ToString();
            default:
                throw new NotImplementedException(option);
        }
    }
}
