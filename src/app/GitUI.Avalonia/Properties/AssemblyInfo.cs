using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("GitUI.Avalonia.Tests")]

// Framework constraint: the apphost starts the independent reporter whose source-shaped program lives with the UI twin.
[assembly: InternalsVisibleTo("GitExtensions.Avalonia")]
