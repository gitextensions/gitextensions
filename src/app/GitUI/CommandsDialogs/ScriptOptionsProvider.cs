using GitExtensions.Extensibility;
using GitUI.ScriptsEngine;

namespace GitUI.CommandsDialogs;

internal class ScriptOptionsProvider : IScriptOptionsProvider
{
    private const string _selectedRelativePaths = "SelectedRelativePaths";
    private const string _lineNumber = "LineNumber";

    private Func<IEnumerable<string>> _getSelectedRelativePaths;
    private Func<int?> _getCurrentLineNumber;

    public ScriptOptionsProvider(Func<IEnumerable<string>> getSelectedRelativePaths, Func<int?> getCurrentLineNumber)
    {
        _getSelectedRelativePaths = getSelectedRelativePaths;
        _getCurrentLineNumber = getCurrentLineNumber;
    }

    public ScriptOptionsProvider(FileStatusList fileStatusList, Func<int?> getCurrentLineNumber)
        : this(() => fileStatusList.SelectedItems.Select(item => item.Item.Name), getCurrentLineNumber)
    {
    }

    IReadOnlyList<string> IScriptOptionsProvider.Options { get; } = new[] { _selectedRelativePaths, _lineNumber };

    IEnumerable<string> IScriptOptionsProvider.GetValues(string option)
    {
        switch (option)
        {
            case _selectedRelativePaths:
                return _getSelectedRelativePaths().Select(item => item.EscapeForCommandLine());
            case _lineNumber:
                return _getCurrentLineNumber() is int lineNumber ? [lineNumber.ToString()] : [];
            default:
                throw new NotImplementedException(option);
        }
    }
}
