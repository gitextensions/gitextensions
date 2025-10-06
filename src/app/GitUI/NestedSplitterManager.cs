namespace GitUI;

public sealed class NestedSplitterManager
{
    private SplitterManager _parent;
    private string _prefix;

    public NestedSplitterManager(SplitterManager parent, string name)
    {
        _parent = parent;
        _prefix = $"{name}.";
    }

    public NestedSplitterManager(NestedSplitterManager parent, string name)
        : this(parent._parent, parent._prefix + name)
    {
    }

    public void AddSplitter(SplitContainer splitter, int? defaultDistance = null)
    {
        _parent.AddSplitter(splitter, $"{_prefix}{splitter.Name}", defaultDistance);
    }
}
