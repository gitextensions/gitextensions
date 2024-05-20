namespace GitUI.ScriptsEngine;

/// <summary>
/// Display a script dialog where the user enter a value.
/// </summary>
public interface IUserInputPrompt : IDisposable
{
    /// <summary>
    ///  Shows this form as a modal dialog with the specified owner.
    /// </summary>
    DialogResult ShowDialog(IWin32Window owner);

    /// <summary>
    /// Contains the value entered by the user.
    /// </summary>
    string UserInput { get; }
}
