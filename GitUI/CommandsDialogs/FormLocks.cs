using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GitCommands;
using GitUI.Properties;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed class FormLocks : GitExtensionsForm
    {
        // private readonly TranslationString _developers = new TranslationString("Developers");
        // private readonly TranslationString _translators = new TranslationString("Translators");
        // private readonly TranslationString _designers = new TranslationString("Designers");
        // private readonly TranslationString _team = new TranslationString("Team");
        // private readonly TranslationString _contributors = new TranslationString("Contributors");
        // private readonly TranslationString _caption = new TranslationString("The application would not be possible without...");

        // [CanBeNull] private IReadOnlyList<GitItemStatus> _currentSelection;

        private FileStatusList _currentFilesList;

        private void StagedSelectionChanged(object sender, EventArgs e)
        {
            _currentFilesList.ClearSelected();

            // _currentSelection = this._currentFilesList.SelectedItems.Items().ToList();

            var item = _currentFilesList.SelectedItem;
        }

        private void Staged_DataSourceChanged(object sender, EventArgs e)
        {
        }

        private void Staged_Enter(object sender, EnterEventArgs e)
        {
        }

        private void Staged_DoubleClick(object sender, EventArgs e)
        {
        }

        public FormLocks()
        {
            InitialiseComponent();
            InitializeComplete();

            void InitialiseComponent()
            {
                SuspendLayout();
                Controls.Clear();

                _currentFilesList = new GitUI.FileStatusList();

                // this._currentFilesList.ContextMenuStrip = this.StagedFileContext;

                _currentFilesList.Dock = System.Windows.Forms.DockStyle.Fill;
                _currentFilesList.Location = new System.Drawing.Point(0, 28);
                _currentFilesList.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
                _currentFilesList.Name = "Staged";
                _currentFilesList.SelectFirstItemOnSetItems = false;
                _currentFilesList.Size = new System.Drawing.Size(397, 314);
                _currentFilesList.TabIndex = 0;
                _currentFilesList.SelectedIndexChanged += new System.EventHandler(StagedSelectionChanged);
                _currentFilesList.DataSourceChanged += new System.EventHandler(Staged_DataSourceChanged);
                _currentFilesList.DoubleClick += new System.EventHandler(Staged_DoubleClick);
                _currentFilesList.Enter += new FileStatusList.EnterEventHandler(Staged_Enter);

                Controls.Add(_currentFilesList);

                AutoScaleDimensions = new SizeF(96F, 96F);
                AutoScaleMode = AutoScaleMode.Dpi;
                ClientSize = new Size(624, 442);
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                StartPosition = FormStartPosition.CenterParent;

                // Text = _caption.Text;

                ResumeLayout(false);

                return;
            }
        }
    }
}
