using GitExtensions.Extensibility;
using GitUI.ScriptsEngine;

namespace GitUI.CommandsDialogs;

internal class ScriptOptionsProvider : IScriptOptionsProvider
{
    private const string _selectedRelativePaths = "SelectedRelativePaths";
    internal const string _lineNumber = "LineNumber";
    private const string _columnNumber = "ColumnNumber";

    private Func<IEnumerable<string>> _getSelectedRelativePaths;
    private Func<int?> _getCurrentLineNumber;
    private Func<int?> _getCurrentColumnNumber;

    public ScriptOptionsProvider(Func<IEnumerable<string>> getSelectedRelativePaths, Func<int?> getCurrentLineNumber, Func<int?> getCurrentColumnNumber)
    {
        _getSelectedRelativePaths = getSelectedRelativePaths;
        _getCurrentLineNumber = getCurrentLineNumber;
        _getCurrentColumnNumber = getCurrentColumnNumber;
    }

    public ScriptOptionsProvider(FileStatusList fileStatusList, Func<int?> getCurrentLineNumber, Func<int?> getCurrentColumn)
        : this(() => fileStatusList.SelectedItems.Select(item => item.Item.Name), getCurrentLineNumber, getCurrentColumn)
    {
    }

    IReadOnlyList<string> IScriptOptionsProvider.Options { get; } = new[] { _selectedRelativePaths, _lineNumber, _columnNumber };

    IEnumerable<string> IScriptOptionsProvider.GetValues(string option)
    {
        switch (option)
        {
            case _selectedRelativePaths:
                return _getSelectedRelativePaths().Select(item => item.EscapeForCommandLine());
            case _lineNumber:
                return _getCurrentLineNumber() is int lineNumber ? [lineNumber.ToString()] : [];
            case _columnNumber:
                return _getCurrentColumnNumber() is int columnNumber ? [columnNumber.ToString()] : [];
            default:
                throw new NotImplementedException(option);
        }
    }
}
