using GitCommands;

namespace GitUI.LeftPanel
{
    /// <summary>A common base class for both <see cref="Node"/> and <see cref="Tree"/>.</summary>
    internal abstract class NodeBase
    {
        /// <summary>The child nodes.</summary>
        protected internal Nodes Nodes { get; protected set; }

        internal bool HasChildren => Nodes.Count > 0;

        /// <summary>The corresponding tree node.</summary>
        protected internal virtual TreeNode TreeViewNode { get; set; }

        /// <summary>
        /// Marks this node to be included in multi-selection. See <see cref="Select(bool, bool)"/>.
        /// This is remembered here instead of relying on the status of <see cref="TreeViewNode"/>
        /// because <see cref="Nodes.FillTreeViewNode(TreeNode)"/> recycles <see cref="TreeNode"/>s
        /// and may change the association between <see cref="Node"/> and <see cref="TreeNode"/>.
        /// </summary>
        protected internal bool IsSelected { get; set; }

        /// <summary>
        /// Gets whether the commit that the node represents is currently visible in the revision grid.
        /// </summary>
        public bool Visible { get; set; }

        protected internal void Select(bool select, bool includingDescendants = false)
        {
            IsSelected = select;
            ApplyStyle(); // toggle multi-selected node style

            // recursively process descendants if required
            if (includingDescendants && HasChildren)
            {
                foreach (Node child in Nodes)
                {
                    child.Select(select, includingDescendants);
                }
            }
        }

        #region style / appearance
        public virtual void ApplyStyle()
        {
            SetFont(GetFontStyle());
            TreeViewNode.ToolTipText = string.Empty;
        }

        protected virtual FontStyle GetFontStyle()
            => IsSelected ? FontStyle.Underline : FontStyle.Regular;

        private void SetFont(FontStyle style)
        {
            if (style == FontStyle.Regular)
            {
                // For regular, set to null to use the NativeTreeView font
                if (TreeViewNode.NodeFont is not null)
                {
                    ResetFont();
                }
            }
            else
            {
                // If current font doesn't have the input style, get rid of it
                if (TreeViewNode.NodeFont is not null && TreeViewNode.NodeFont.Style != style)
                {
                    ResetFont();
                }

                // If non-null, our font is already valid, otherwise create a new one
                TreeViewNode.NodeFont ??= new Font(AppSettings.Font, style);
            }
        }

        private void ResetFont()
        {
            TreeViewNode.NodeFont.Dispose();
            TreeViewNode.NodeFont = null;
        }
        #endregion
    }
}
