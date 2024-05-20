using System.Diagnostics;

namespace GitExtensions.Extensibility;

[DebuggerDisplay("{" + nameof(Arguments) + "}")]
public readonly struct ArgumentString
{
    public string? Arguments { get; }
    public int Length { get => Arguments?.Length ?? 0; }

    private ArgumentString(string arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        Arguments = arguments;
    }

    public static implicit operator ArgumentString(string? args) => new(args ?? "");
    public static implicit operator ArgumentString(ArgumentBuilder args) => new(args.ToString());
    public static implicit operator string(ArgumentString args) => args.Arguments ?? "";
    public override string ToString() => Arguments ?? "";
}
