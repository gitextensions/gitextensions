namespace GitUI;

public sealed class NestedSplitterManager
{
    private readonly SplitterManager _parent;
    private readonly string _prefix;

    public NestedSplitterManager(SplitterManager parent, string name)
    {
        _parent = parent;
        _prefix = $"{name}.";
    }

    public NestedSplitterManager(NestedSplitterManager parent, string name)
        : this(parent._parent, parent._prefix + name)
    {
    }

    public void AddSplitter(Avalonia.Controls.Grid splitter, int? defaultDistance = null)
        => _parent.AddSplitter(splitter, $"{_prefix}{splitter.Name}", defaultDistance);

    internal void AddSplitter(IPersistedSplitter splitter, string name, int? defaultDistance = null)
        => _parent.AddSplitter(splitter, $"{_prefix}{name}", defaultDistance);
}
