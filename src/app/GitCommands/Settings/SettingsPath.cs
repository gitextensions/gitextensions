﻿#nullable enable

using GitExtensions.Extensibility.Configurations;
using GitExtensions.Extensibility.Settings;
using Microsoft;

namespace GitCommands.Settings;

public class SettingsPath : SettingsSource
{
    private readonly IConfigValueStore? _parent;
    private readonly string _pathNameWithSeparator;

    public SettingsPath(IConfigValueStore? parent, string pathName)
    {
        _parent = parent;
        _pathNameWithSeparator = string.IsNullOrWhiteSpace(pathName) ? "" : $"{pathName}.";
    }

    public string PathFor(string subPath)
    {
        return $"{_pathNameWithSeparator}{subPath}";
    }

    public override string? GetValue(string name)
    {
        Validates.NotNull(_parent);
        return _parent.GetValue(PathFor(name));
    }

    public override void SetValue(string name, string? value)
    {
        Validates.NotNull(_parent);
        _parent.SetValue(PathFor(name), value);
    }
}
