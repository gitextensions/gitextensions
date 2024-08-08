namespace GitUI.ScriptsEngine;

public interface IScriptOptionsProvider
{
    /// <summary>
    ///  The list of script argument options which are supported by this provider.
    /// </summary>
    IReadOnlyList<string> Options { get; }

    /// <summary>
    ///  Retrieves the information for placeholders in script arguments.
    /// </summary>
    /// <param name="option">The option identifier which is to be replaced.</param>
    /// <returns>The value(s) to be used for the script argument.</returns>
    IEnumerable<string> GetValues(string option);
}
