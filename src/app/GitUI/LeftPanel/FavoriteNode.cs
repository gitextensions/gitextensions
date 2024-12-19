namespace GitUI.LeftPanel;
#nullable enable
internal class FavoriteNode : BaseRevisionNode
{
    private readonly string _imageKey;

    public FavoriteNode(FavoritesTree tree, string name, string imageKey)
        : base(tree, name, true)
    {
        _imageKey = imageKey;
    }

    public override void ApplyStyle()
    {
        base.ApplyStyle();
        TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey = _imageKey;
    }
}
