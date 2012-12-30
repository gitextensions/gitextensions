using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace ReleaseNotesGenerator
{
    public partial class ReleaseNotesGeneratorForm : Form
    {
        private readonly IGitPluginSettingsContainer _settings;
        private readonly GitUIBaseEventArgs _gitUiCommands;

        public ReleaseNotesGeneratorForm(IGitPluginSettingsContainer settings, GitUIBaseEventArgs gitUiCommands)
        {
            InitializeComponent();

            _settings = settings;
            _gitUiCommands = gitUiCommands;
        }
    }
}
