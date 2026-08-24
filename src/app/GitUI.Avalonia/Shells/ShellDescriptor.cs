using Avalonia.Media;

namespace GitUI.Shells;

public abstract class ShellDescriptor : IShellDescriptor
{
    /// <inheritdoc/>
    public string? ExecutableCommandLine { get; protected set; }

    /// <inheritdoc/>
    public string ExecutableName { get; protected set; } = null!;

    /// <inheritdoc/>
    public string? ExecutablePath { get; protected set; }

    /// <inheritdoc/>
    public bool HasExecutable => ExecutablePath is not null;

    /// <inheritdoc/>
    public IImage Icon { get; protected set; } = null!;

    /// <inheritdoc/>
    public string Name { get; protected set; } = null!;

    /// <inheritdoc/>
    public abstract string GetChangeDirCommand(string path);

    // Avalonia constraint: ComboBox has no DisplayMember property, so expose the same visible name through ToString().
    public override string ToString() => Name;
}
