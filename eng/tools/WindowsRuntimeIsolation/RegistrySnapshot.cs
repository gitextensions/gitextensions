using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Win32;

namespace GitExtensions.WindowsRuntimeIsolation;

internal sealed class RegistrySnapshot
{
    private readonly SortedDictionary<string, string> _entries = new(StringComparer.OrdinalIgnoreCase);

    internal string Hash => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(_entries))));

    internal static RegistrySnapshot Capture(string path)
    {
        RegistrySnapshot snapshot = new();
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(path, writable: false);
        if (key is not null)
        {
            snapshot.CaptureKey(key, string.Empty);
        }

        return snapshot;
    }

    internal void AssertUnchanged(RegistrySnapshot after)
    {
        string[] changed = _entries.Keys.Union(after._entries.Keys, StringComparer.OrdinalIgnoreCase)
            .Where(key => !_entries.TryGetValue(key, out string? beforeValue)
                || !after._entries.TryGetValue(key, out string? afterValue)
                || beforeValue != afterValue)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (changed.Length != 0)
        {
            throw new InvalidOperationException($"Owner registry changed: {string.Join(", ", changed)}. It has not been restored or written by the guard.");
        }
    }

    private void CaptureKey(RegistryKey key, string relativePath)
    {
        _entries.Add($"key:{relativePath}", string.Empty);
        foreach (string name in key.GetValueNames())
        {
            object? value = key.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
            _entries.Add($"value:{relativePath}\\{name}", $"{key.GetValueKind(name)}:{JsonSerializer.Serialize(value)}");
        }

        foreach (string name in key.GetSubKeyNames())
        {
            using RegistryKey child = key.OpenSubKey(name, writable: false)!;
            CaptureKey(child, $"{relativePath}\\{name}");
        }
    }
}
