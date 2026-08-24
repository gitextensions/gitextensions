namespace GitUI.Shells;

/// <summary>
///  Provides access to the available shell descriptors.
/// </summary>
public interface IShellProvider
{
    /// <summary>
    ///  Gets all available shell descriptors.
    /// </summary>
    /// <returns>A read-only list of shell descriptors.</returns>
    IReadOnlyList<IShellDescriptor> GetShells();

    /// <summary>
    ///  Gets the shell descriptor for the specified shell name, or the default shell if not found.
    /// </summary>
    /// <param name="name">The name of the shell to retrieve.</param>
    /// <returns>The matching <see cref="IShellDescriptor"/>, or the default shell if not found.</returns>
    IShellDescriptor GetShell(string? name);

    /// <summary>
    ///  Gets the command line string for the specified shell type.
    /// </summary>
    /// <param name="shellType">The name of the shell type.</param>
    /// <returns>The command line string for the shell, or the default console command line as fallback.</returns>
    string GetShellCommandLine(string? shellType);
}
