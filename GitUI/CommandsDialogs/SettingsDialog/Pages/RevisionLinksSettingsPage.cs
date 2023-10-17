﻿using GitCommands.ExternalLinks;
using GitExtUtils.GitUI;
using GitUI.CommandsDialogs.SettingsDialog.RevisionLinks;
using GitUIPluginInterfaces;
using Microsoft;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public sealed partial class RevisionLinksSettingsPage : DistributedSettingsPage
    {
        private readonly TranslationString _addTemplate = new("Add {0} templates");
        private ExternalLinksManager? _externalLinksManager;

        public RevisionLinksSettingsPage(IServiceProvider serviceProvider)
           : base(serviceProvider)
        {
            InitializeComponent();
            CaptionCol.Width = DpiUtil.Scale(150);
            splitContainer1.Panel1MinSize = toolStripManageCategories.Width;
            InitializeComplete();
            LinksGrid.AutoGenerateColumns = false;
            CaptionCol.DataPropertyName = nameof(ExternalLinkFormat.Caption);
            URICol.DataPropertyName = nameof(ExternalLinkFormat.Format);
            LoadTemplatesInMenu();
        }

        protected override void SettingsToPage()
        {
            Validates.NotNull(CurrentSettings);
            _externalLinksManager = new ExternalLinksManager(CurrentSettings);

            ReloadCategories();
            if (_NO_TRANSLATE_Categories.Items.Count > 0)
            {
                _NO_TRANSLATE_Categories.SelectedIndex = 0;
            }

            CategoryChanged();

            base.SettingsToPage();
        }

        protected override void PageToSettings()
        {
            Validates.NotNull(_externalLinksManager);
            _externalLinksManager.Save();

            base.PageToSettings();
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
            Validates.NotNull(_externalLinksManager);

            IReadOnlyList<ExternalLinkDefinition> effectiveLinkDefinitions = _externalLinksManager.GetEffectiveSettings();

            _NO_TRANSLATE_Categories.DataSource = null;
            _NO_TRANSLATE_Categories.DisplayMember = nameof(ExternalLinkDefinition.Name);
            _NO_TRANSLATE_Categories.DataSource = effectiveLinkDefinitions;
        }

        private ExternalLinkDefinition? SelectedLinkDefinition => _NO_TRANSLATE_Categories.SelectedItem as ExternalLinkDefinition;

        private void CategoryChanged()
        {
            if (SelectedLinkDefinition is null)
            {
                splitContainer1.Panel2.Enabled = false;
                _NO_TRANSLATE_Name.Text = string.Empty;
                EnabledChx.Checked = false;
                MessageChx.Checked = false;
                LocalBranchChx.Checked = false;
                RemoteBranchChx.Checked = false;
                _NO_TRANSLATE_SearchPatternEdit.Text = string.Empty;
                _NO_TRANSLATE_NestedPatternEdit.Text = string.Empty;
                _NO_TRANSLATE_RemotePatern.Text = string.Empty;
                _NO_TRANSLATE_UseRemotes.Text = string.Empty;
                chkOnlyFirstRemote.Checked = false;
                chxURL.Checked = false;
                chxPushURL.Checked = false;
                LinksGrid.DataSource = null;
            }
            else
            {
                splitContainer1.Panel2.Enabled = true;
                _NO_TRANSLATE_Name.Text = SelectedLinkDefinition.Name;
                EnabledChx.Checked = SelectedLinkDefinition.Enabled;
                MessageChx.Checked = SelectedLinkDefinition.SearchInParts.Contains(ExternalLinkDefinition.RevisionPart.Message);
                LocalBranchChx.Checked = SelectedLinkDefinition.SearchInParts.Contains(ExternalLinkDefinition.RevisionPart.LocalBranches);
                RemoteBranchChx.Checked = SelectedLinkDefinition.SearchInParts.Contains(ExternalLinkDefinition.RevisionPart.RemoteBranches);
                _NO_TRANSLATE_SearchPatternEdit.Text = SelectedLinkDefinition.SearchPattern;
                _NO_TRANSLATE_NestedPatternEdit.Text = SelectedLinkDefinition.NestedSearchPattern;
                _NO_TRANSLATE_RemotePatern.Text = SelectedLinkDefinition.RemoteSearchPattern;
                chxURL.Checked = SelectedLinkDefinition.RemoteSearchInParts.Contains(ExternalLinkDefinition.RemotePart.URL);
                chxPushURL.Checked = SelectedLinkDefinition.RemoteSearchInParts.Contains(ExternalLinkDefinition.RemotePart.PushURL);
                _NO_TRANSLATE_UseRemotes.Text = SelectedLinkDefinition.UseRemotesPattern;
                chkOnlyFirstRemote.Checked = SelectedLinkDefinition.UseOnlyFirstRemote;
                LinksGrid.DataSource = SelectedLinkDefinition.LinkFormats;
            }
        }

        private void Add_Click(object sender, EventArgs e)
        {
            ExternalLinkDefinition definition = new()
            {
                Name = "<new>",
                Enabled = true,
                UseRemotesPattern = "upstream|origin",
                UseOnlyFirstRemote = true,
                SearchInParts = { ExternalLinkDefinition.RevisionPart.Message },
                RemoteSearchInParts = { ExternalLinkDefinition.RemotePart.URL }
            };
            Validates.NotNull(_externalLinksManager);
            _externalLinksManager.Add(definition);

            ReloadCategories();
            _NO_TRANSLATE_Categories.SelectedItem = definition;
            CategoryChanged();
        }

        private void LoadTemplatesInMenu()
        {
            foreach (ICloudProviderExternalLinkDefinitionExtractor externalLinkDefinitionExtractor in new CloudProviderExternalLinkDefinitionExtractorFactory().GetAllExtractor())
            {
                Add.DropDownItems.Add(new ToolStripMenuItem(
                    string.Format(_addTemplate.Text, externalLinkDefinitionExtractor.ServiceName),
                    externalLinkDefinitionExtractor.Icon,
                    (o, i) => ExtractExternalLinkDefinitions(externalLinkDefinitionExtractor))
                {
                    Tag = externalLinkDefinitionExtractor
                });
            }
        }

        private static Remote FindRemoteByPreference(IList<Remote> remotes)
        {
            if (remotes?.Count is not > 0)
            {
                return default;
            }

            string[] remoteNames = new[] { "upstream", "fork", "origin" };
            foreach (string remoteName in remoteNames)
            {
                Remote remoteFound = remotes.FirstOrDefault(r => r.Name == remoteName);
                if (remoteFound.Name is not null)
                {
                    return remoteFound;
                }
            }

            return remotes[0];
        }

        private void ExtractExternalLinkDefinitions(ICloudProviderExternalLinkDefinitionExtractor externalLinkDefinitionExtractor)
        {
            Validates.NotNull(Module);
            Validates.NotNull(_externalLinksManager);

            IReadOnlyList<Remote> remotes = ThreadHelper.JoinableTaskFactory.Run(Module.GetRemotesAsync).ToList();
            Remote selectedRemote = FindRemoteByPreference(remotes.Where(r => externalLinkDefinitionExtractor.IsValidRemoteUrl(r.FetchUrl)).ToList());

            IList<ExternalLinkDefinition> externalLinkDefinitions = externalLinkDefinitionExtractor.GetDefinitions(selectedRemote.FetchUrl);
            _externalLinksManager.AddRange(externalLinkDefinitions);

            ReloadCategories();
            _NO_TRANSLATE_Categories.SelectedItem = externalLinkDefinitions[0];
            CategoryChanged();
        }

        private void Remove_Click(object sender, EventArgs e)
        {
            if (SelectedLinkDefinition is null)
            {
                return;
            }

            Validates.NotNull(_externalLinksManager);

            int idx = _NO_TRANSLATE_Categories.SelectedIndex;

            _externalLinksManager.Remove(SelectedLinkDefinition);

            ReloadCategories();

            if (idx >= 0)
            {
                _NO_TRANSLATE_Categories.SelectedIndex = Math.Min(idx, _NO_TRANSLATE_Categories.Items.Count - 1);
            }

            CategoryChanged();
        }

        private void _NO_TRANSLATE_Name_Leave(object sender, EventArgs e)
        {
            if (SelectedLinkDefinition is not null)
            {
                ExternalLinkDefinition selected = SelectedLinkDefinition;
                selected.Name = _NO_TRANSLATE_Name.Text;
                ReloadCategories();
                _NO_TRANSLATE_Categories.SelectedItem = selected;
            }
        }

        private void EnabledChx_CheckedChanged(object sender, EventArgs e)
        {
            if (SelectedLinkDefinition is not null)
            {
                SelectedLinkDefinition.Enabled = EnabledChx.Checked;
            }
        }

        private void MessageChx_CheckedChanged(object sender, EventArgs e)
        {
            if (SelectedLinkDefinition is not null)
            {
                if (MessageChx.Checked)
                {
                    SelectedLinkDefinition.SearchInParts.Add(ExternalLinkDefinition.RevisionPart.Message);
                }
                else
                {
                    SelectedLinkDefinition.SearchInParts.Remove(ExternalLinkDefinition.RevisionPart.Message);
                }
            }
        }

        private void _NO_TRANSLATE_SearchPatternEdit_Leave(object sender, EventArgs e)
        {
            if (SelectedLinkDefinition is not null)
            {
                SelectedLinkDefinition.SearchPattern = _NO_TRANSLATE_SearchPatternEdit.Text.Trim();
            }
        }

        private void _NO_TRANSLATE_NestedPatternEdit_Leave(object sender, EventArgs e)
        {
            if (SelectedLinkDefinition is not null)
            {
                SelectedLinkDefinition.NestedSearchPattern = _NO_TRANSLATE_NestedPatternEdit.Text.Trim();
            }
        }

        private void LocalBranchChx_CheckedChanged(object sender, EventArgs e)
        {
            if (SelectedLinkDefinition is not null)
            {
                if (LocalBranchChx.Checked)
                {
                    SelectedLinkDefinition.SearchInParts.Add(ExternalLinkDefinition.RevisionPart.LocalBranches);
                }
                else
                {
                    SelectedLinkDefinition.SearchInParts.Remove(ExternalLinkDefinition.RevisionPart.LocalBranches);
                }
            }
        }

        private void RemoteBranchChx_CheckedChanged(object sender, EventArgs e)
        {
            if (SelectedLinkDefinition is not null)
            {
                if (RemoteBranchChx.Checked)
                {
                    SelectedLinkDefinition.SearchInParts.Add(ExternalLinkDefinition.RevisionPart.RemoteBranches);
                }
                else
                {
                    SelectedLinkDefinition.SearchInParts.Remove(ExternalLinkDefinition.RevisionPart.RemoteBranches);
                }
            }
        }

        private void _NO_TRANSLATE_RemotePatern_Leave(object sender, EventArgs e)
        {
            if (SelectedLinkDefinition is not null)
            {
                SelectedLinkDefinition.RemoteSearchPattern = _NO_TRANSLATE_RemotePatern.Text.Trim();
            }
        }

        private void chxURL_CheckedChanged(object sender, EventArgs e)
        {
            if (SelectedLinkDefinition is not null)
            {
                if (chxURL.Checked)
                {
                    SelectedLinkDefinition.RemoteSearchInParts.Add(ExternalLinkDefinition.RemotePart.URL);
                }
                else
                {
                    SelectedLinkDefinition.RemoteSearchInParts.Remove(ExternalLinkDefinition.RemotePart.URL);
                }
            }
        }

        private void chxPushURL_CheckedChanged(object sender, EventArgs e)
        {
            if (SelectedLinkDefinition is not null)
            {
                if (chxPushURL.Checked)
                {
                    SelectedLinkDefinition.RemoteSearchInParts.Add(ExternalLinkDefinition.RemotePart.PushURL);
                }
                else
                {
                    SelectedLinkDefinition.RemoteSearchInParts.Remove(ExternalLinkDefinition.RemotePart.PushURL);
                }
            }
        }

        private void _NO_TRANSLATE_UseRemotes_Leave(object sender, EventArgs e)
        {
            if (SelectedLinkDefinition is not null)
            {
                SelectedLinkDefinition.UseRemotesPattern = _NO_TRANSLATE_UseRemotes.Text.Trim();
            }
        }

        private void chkOnlyFirstRemote_CheckedChanged(object sender, EventArgs e)
        {
            if (SelectedLinkDefinition is not null)
            {
                SelectedLinkDefinition.UseOnlyFirstRemote = chkOnlyFirstRemote.Checked;
            }
        }
    }
}
