using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using System.IO;

namespace GitUI
{
    public partial class FormMailMap : GitExtensionsForm
    {
        public FormMailMap()
        {
            InitializeComponent();
            MailMapFile = "";

            try
            {
                if (File.Exists(Settings.WorkingDir + ".mailmap"))
                {
                    StreamReader re = File.OpenText(Settings.WorkingDir + ".mailmap");
                    MailMapFile = re.ReadToEnd();
                    re.Close();
                }
                MailMapText.Text = MailMapFile;
            }
            catch
            {
            }
        }

        public string MailMapFile;

        private void Save_Click(object sender, EventArgs e)
        {
            MailMapFile = MailMapText.Text;
            TextWriter tw = new StreamWriter(Settings.WorkingDir + ".mailmap", false);
            tw.Write(MailMapFile);
            tw.Close();
            Close();
        }

        private void FormMailMap_Load(object sender, EventArgs e)
        {
            if (Settings.IsBareRepository())
            {
                MessageBox.Show(".mailmap is only supported when there is a working dir.");
                Close();
            }
        }
    }
}
