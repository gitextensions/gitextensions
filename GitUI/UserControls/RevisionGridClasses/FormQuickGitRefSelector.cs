using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.UserControls.RevisionGridClasses
{
    public partial class FormQuickGitRefSelector : GitModuleForm
    {
        private readonly TranslationString _actionRename = new TranslationString("Rename");
        private readonly TranslationString _actionDelete = new TranslationString("Delete");
        private readonly TranslationString _tag = new TranslationString("tag");
        private const short MaxVisibleItemsWithoutScroll = 8;
        private const short MaxRefLength = 100;

        public FormQuickGitRefSelector()
        {
            InitializeComponent();
            Translate();

            lbxRefs.DisplayMember = nameof(DisplyGitRef.Label);
        }

        /// <summary>
        /// Gets the ref selected by the user.
        /// </summary>
        public IGitRef SelectedRef => (lbxRefs.SelectedItem as DisplyGitRef)?.Item;

        public void Init(Action action, IGitRef[] refs)
        {
            switch (action)
            {
                case Action.Delete:
                    {
                        btnAction.Text = _actionDelete.Text;
                    }

                    break;
                case Action.Rename:
                    {
                        btnAction.Text = _actionRename.Text;
                    }

                    break;
            }

            lbxRefs.Items.Clear();
            if (refs == null || refs.Length < 1)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            // constrain the listbox min width, otherwise the button may not have enough space
            var longestItemWidth = 150f;
            try
            {
                lbxRefs.BeginUpdate();

                using (Graphics graphics = lbxRefs.CreateGraphics())
                {
                    foreach (var gitRef in refs.OrderBy(r => r.IsTag).ThenBy(r => r.Name))
                    {
                        var suffix = gitRef.IsTag ? $" ({_tag.Text})" : string.Empty;
                        var item = new DisplyGitRef
                        {
                            Label = $"{gitRef.Name}{suffix}",
                            Item = gitRef
                        };
                        lbxRefs.Items.Add(item);

                        // assume that the branch names or tags are never longer than MaxRefLength symbols long
                        // if they are (sanity!) - don't resize past beyond certain limit
                        var label = item.Label.Length > MaxRefLength ? item.Label.Substring(0, MaxRefLength) : item.Label;
                        longestItemWidth = Math.Max(longestItemWidth, graphics.MeasureString(label, lbxRefs.Font).Width);
                    }
                }
            }
            finally
            {
                // what is this magic number "MaxVisibleItemsWithoutScroll + 0.5"?
                // we are limiting number of visible items in the listbox before enabling vscroll, this is to reduce the risk of not fitting on user's screen.
                // when listbox.IntergralHeight=true, the listbox automatically calculates its size and only show full items (i.e. you can't render it
                // at 20px high if the ItemHeight=13px, it can only be 13, 26, 39 etc (NB: slightly more if you account for the furniture)).
                // listbox.PreferredHeight returns the height of the listbox showing all items.
                // for some strange reason, MaxVisibleItemsWithoutScroll*listbox.ItemHeight renders the listbox showing MaxVisibleItemsWithoutScroll-1 items.
                // to fix this, trick the listbox and set its height to show MaxVisibleItemsWithoutScroll+0.5
                // this internally forces it to re-calculates its bounds and then to calculate its preferred height
                lbxRefs.Height = Math.Min(lbxRefs.PreferredHeight, (int)((MaxVisibleItemsWithoutScroll + 0.5) * lbxRefs.ItemHeight));
                lbxRefs.Width = (int)longestItemWidth;
                lbxRefs.EndUpdate();
                lbxRefs.SelectedIndex = 0;
                lbxRefs.Focus();

                // resize the form to look nice
                Size = new Size(lbxRefs.Width + Padding.Left + Padding.Right,
                                lbxRefs.Height + Padding.Top + flowLayoutPanel1.Height);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void lbxRefs_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            AcceptButton.PerformClick();
        }

        public enum Action
        {
            Rename = 0,
            Delete
        }

        private class DisplyGitRef
        {
            public string Label { get; set; }
            public IGitRef Item { get; set; }
        }
    }
}
