using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using MediaColor = Avalonia.Media.Color;

namespace GitUI.Editor.Diff;

internal static class DiffBrushes
{
    public static IBrush Get(Control owner, string key, MediaColor fallback)
        => Application.Current?.TryGetResource(key, owner.ActualThemeVariant, out object? resource) == true
            && resource is IBrush brush
                ? brush
                : new SolidColorBrush(fallback).ToImmutable();
}
