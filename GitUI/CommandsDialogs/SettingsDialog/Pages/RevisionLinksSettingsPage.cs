using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitCommands.GitExtLinks;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class RevisionLinksSettingsPage : RepoDistSettingsPage
    {
        private GitExtLinksParser parser;

        public RevisionLinksSettingsPage()
        {
            InitializeComponent();
            Text = "Revision links";
            Translate();
            LinksGrid.AutoGenerateColumns = false;
        }

        protected override void  SettingsToPage() 	
        {
            parser = new GitExtLinksParser(CurrentSettings);
            ReloadCategories();
            if (_NO_TRANSLATE_Categories.Items.Count > 0)
            {
                _NO_TRANSLATE_Categories.SelectedIndex = 0;
            }
            CategoryChanged();
        }

        protected override void PageToSettings()
        {
            if (parser != null)
            {
                parser.SaveToSettings();
            }
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(RevisionLinksSettingsPage));
        }

        private void _NO_TRANSLATE_Categories_SelectedIndexChanged(object sender, EventArgs e)
        {
            CategoryChanged();
        }

        private void ReloadCategories()
        {
            _NO_TRANSLATE_Categories.DataSource = null;
            if (parser != null)
            {
                _NO_TRANSLATE_Categories.DisplayMember = "Name";
                _NO_TRANSLATE_Categories.DataSource = parser.EffectiveLinkDefs;
            }
        }

        private GitExtLinkDef SelectedCategory
        {
            get
            {
                return _NO_TRANSLATE_Categories.SelectedItem as GitExtLinkDef;
            }
        }

        private void CategoryChanged()
        {
            if (SelectedCategory == null)
            {
                splitContainer1.Panel2.Enabled = false;
                _NO_TRANSLATE_Name.Text = string.Empty;
                EnabledChx.Checked = false;
                MessageChx.Checked = false;
                LocalBranchChx.Checked = false;
                RemoteBranchChx.Checked = false;
                _NO_TRANSLATE_SearchPatternEdit.Text = string.Empty;
                _NO_TRANSLATE_NestedPatternEdit.Text = string.Empty;
                LinksGrid.DataSource = null;
            }
            else
            {
                splitContainer1.Panel2.Enabled = true;
                _NO_TRANSLATE_Name.Text = SelectedCategory.Name;
                EnabledChx.Checked = SelectedCategory.Enabled;
                MessageChx.Checked = SelectedCategory.SearchInParts.Contains(GitExtLinkDef.RevisionPart.Message);
                LocalBranchChx.Checked = SelectedCategory.SearchInParts.Contains(GitExtLinkDef.RevisionPart.LocalBranches);
                RemoteBranchChx.Checked = SelectedCategory.SearchInParts.Contains(GitExtLinkDef.RevisionPart.RemoteBranches);
                _NO_TRANSLATE_SearchPatternEdit.Text = SelectedCategory.SearchPattern;
                _NO_TRANSLATE_NestedPatternEdit.Text = SelectedCategory.NestedSearchPattern;
                LinksGrid.DataSource = SelectedCategory.LinkFormats;
            }
        }

        private void Add_Click(object sender, EventArgs e)
        {
            GitExtLinkDef newCategory = new GitExtLinkDef();
            newCategory.Name = "<new>";
            newCategory.SearchInParts.Add(GitExtLinkDef.RevisionPart.Message);
            newCategory.Enabled = true;
            parser.AddLinkDef(newCategory);
            ReloadCategories();
            _NO_TRANSLATE_Categories.SelectedItem = newCategory;
            CategoryChanged();           
        }

        private void Remove_Click(object sender, EventArgs e)
        {
            if (SelectedCategory == null)
                return;

            int idx = _NO_TRANSLATE_Categories.SelectedIndex;

            parser.RemoveLinkDef(SelectedCategory);
            ReloadCategories();

            if (idx >= 0)
            {
                _NO_TRANSLATE_Categories.SelectedIndex = Math.Min(idx, _NO_TRANSLATE_Categories.Items.Count - 1);
            }

            CategoryChanged();
        }

        private void _NO_TRANSLATE_Name_Leave(object sender, EventArgs e)
        {
            if (SelectedCategory != null)
            {
                var selected = SelectedCategory;
                selected.Name = _NO_TRANSLATE_Name.Text;
                ReloadCategories();
                _NO_TRANSLATE_Categories.SelectedItem = selected;
            }
        }

        private void EnabledChx_CheckedChanged(object sender, EventArgs e)
        {
            if (SelectedCategory != null)
            {
                SelectedCategory.Enabled = EnabledChx.Checked;
            }
        }

        private void MessageChx_CheckedChanged(object sender, EventArgs e)
        {

            if (SelectedCategory != null)
            {
                if (MessageChx.Checked)
                {
                    SelectedCategory.SearchInParts.Add(GitExtLinkDef.RevisionPart.Message);
                }
                else
                {
                    SelectedCategory.SearchInParts.Remove(GitExtLinkDef.RevisionPart.Message);
                }
            }
        }

        private void _NO_TRANSLATE_SearchPatternEdit_Leave(object sender, EventArgs e)
        {
            if (SelectedCategory != null)
            {
                SelectedCategory.SearchPattern = _NO_TRANSLATE_SearchPatternEdit.Text.Trim();
            }
        }

        private void _NO_TRANSLATE_NestedPatternEdit_Leave(object sender, EventArgs e)
        {
            if (SelectedCategory != null)
            {
                SelectedCategory.NestedSearchPattern = _NO_TRANSLATE_NestedPatternEdit.Text.Trim();
            }
        }

        private void LocalBranchChx_CheckedChanged(object sender, EventArgs e)
        {
            if (SelectedCategory != null)
            {
                if (LocalBranchChx.Checked)
                {
                    SelectedCategory.SearchInParts.Add(GitExtLinkDef.RevisionPart.LocalBranches);
                }
                else
                {
                    SelectedCategory.SearchInParts.Remove(GitExtLinkDef.RevisionPart.LocalBranches);
                }
            }
        }

        private void RemoteBranchChx_CheckedChanged(object sender, EventArgs e)
        {
            if (SelectedCategory != null)
            {
                if (RemoteBranchChx.Checked)
                {
                    SelectedCategory.SearchInParts.Add(GitExtLinkDef.RevisionPart.RemoteBranches);
                }
                else
                {
                    SelectedCategory.SearchInParts.Remove(GitExtLinkDef.RevisionPart.RemoteBranches);
                }
            }
        }
    }
}
