namespace GitUI.ScriptsEngine;

public interface IScriptOptionsForm
{
    /// <summary>
    ///  Gets an <see cref="IScriptOptionsProvider"/> instance supported by the form or its active control.
    /// </summary>
    IScriptOptionsProvider GetScriptOptionsProvider();
}
