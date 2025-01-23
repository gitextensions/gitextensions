using StrongOf;

namespace GitExtensions.Extensibility.Git;

/// <summary>
///  Contains a path relative to the root of a repository (with POSIX path separators).
/// </summary>
public sealed class RelativePath(string value) : StrongString<RelativePath>(value)
{
}
