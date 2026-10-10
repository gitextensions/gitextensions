using System.Diagnostics;
using System.Reflection;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using GitExtensions.Extensibility.Plugins;

namespace GitUI.Compat;

internal static class PluginIconProvider
{
    private static readonly Dictionary<Type, IImage> EmbeddedIcons = [];
    private static readonly Lock SyncRoot = new();

    internal static IImage? GetIcon(IGitPlugin plugin)
    {
        if (plugin.Icon?.PlatformImage is IImage platformImage)
        {
            return platformImage;
        }

        Type pluginType = plugin.GetType();
        lock (SyncRoot)
        {
            if (EmbeddedIcons.TryGetValue(pluginType, out IImage? cached))
            {
                return cached;
            }

            try
            {
                Assembly assembly = pluginType.Assembly;
                string? resourceName = assembly.GetManifestResourceNames()
                    .Where(name => name.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    .Where(name => Path.GetFileNameWithoutExtension(name)
                        .Split('.')
                        .Last()
                        .StartsWith("Icon", StringComparison.OrdinalIgnoreCase))
                    .Order(StringComparer.Ordinal)
                    .FirstOrDefault();
                if (resourceName is null)
                {
                    return null;
                }

                using Stream? stream = assembly.GetManifestResourceStream(resourceName);
                if (stream is null)
                {
                    return null;
                }

                Bitmap bitmap = new(stream);
                EmbeddedIcons.Add(pluginType, bitmap);
                return bitmap;
            }
            catch (Exception exception)
            {
                Trace.WriteLine($"Could not load icon for plugin {plugin.Name}: {exception}");
                return null;
            }
        }
    }
}
