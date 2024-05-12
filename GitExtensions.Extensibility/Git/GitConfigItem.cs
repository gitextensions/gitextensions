namespace GitExtensions.Extensibility.Git;

/// <summary>
/// A configuration key/value pair for use in git command lines.
/// </summary>
public readonly struct GitConfigItem
{
    public string Key { get; }
    public string Value { get; }

    public GitConfigItem(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public void Deconstruct(out string key, out string value)
    {
        key = Key;
        value = Value;
    }
}
