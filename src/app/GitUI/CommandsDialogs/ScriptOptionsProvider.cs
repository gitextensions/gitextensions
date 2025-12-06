using GitExtensions.Extensibility;
using GitUI.ScriptsEngine;

namespace GitUI.CommandsDialogs;

internal class ScriptOptionsProvider : IScriptOptionsProvider
{
    private const string _selectedRelativePaths = "SelectedRelativePaths";
    internal const string _lineNumber = "LineNumber";
    private const string _fileLine = "FileLine";
    private const string _fileColumn = "FileColumn";

    private Func<IEnumerable<string>> _getSelectedRelativePaths;
    private Func<int?> _getCurrentLineNumber;
    private Func<int?> _getCurrentColumn;

    public ScriptOptionsProvider(Func<IEnumerable<string>> getSelectedRelativePaths, Func<int?> getCurrentLineNumber, Func<int?> getCurrentColumn)
    {
        _getSelectedRelativePaths = getSelectedRelativePaths;
        _getCurrentLineNumber = getCurrentLineNumber;
        _getCurrentColumn = getCurrentColumn;
    }

    public ScriptOptionsProvider(FileStatusList fileStatusList, Func<int?> getCurrentLineNumber, Func<int?> getCurrentColumn)
        : this(() => fileStatusList.SelectedItems.Select(item => item.Item.Name), getCurrentLineNumber, getCurrentColumn)
    {
    }

    IReadOnlyList<string> IScriptOptionsProvider.Options { get; } = new[] { _selectedRelativePaths, _lineNumber, _fileLine, _fileColumn };

    IEnumerable<string> IScriptOptionsProvider.GetValues(string option)
    {
        switch (option)
        {
            case _selectedRelativePaths:
                return _getSelectedRelativePaths().Select(item => item.EscapeForCommandLine());
            case _lineNumber:
            case _fileLine:
                return _getCurrentLineNumber() is int lineNumber ? [lineNumber.ToString()] : [];
            case _fileColumn:
                return _getCurrentColumn() is int column ? [column.ToString()] : [];
            default:
                throw new NotImplementedException(option);
        }
    }
}
