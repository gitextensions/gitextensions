using GitUI.Properties;

namespace GitUI.LeftPanel
{
    // Top-level nodes used to group SubmoduleNodes
    internal sealed class SubmoduleFolderNode : Node
    {
        private string _name;

        public SubmoduleFolderNode(Tree tree, string name)
            : base(tree)
        {
            _name = name;
        }

        protected override string DisplayText()
        {
            return string.Format(_name);
        }

        public override void ApplyStyle()
        {
            base.ApplyStyle();
            TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey = nameof(Images.FolderClosed);
        }

        protected override FontStyle GetFontStyle()
            => base.GetFontStyle() | FontStyle.Italic;
    }
}
