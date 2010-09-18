using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace GitUI
{
    public partial class FormEditor : GitExtensionsForm
    {
        public FormEditor()
        {
            InitializeComponent();
            Translate();
        }

        public FormEditor(string fileName)
        {
            InitializeComponent();
            Translate();

            OpenFile(fileName);
        }

        private string _fileName;

        private void OpenFile(string fileName)
        {
            RestorePosition("fileeditor");

            try
            {
                _fileName = fileName;
                fileViewer.ViewFile(_fileName);
                fileViewer.IsReadOnly = false;
                fileViewer.EnableDiffContextMenu(false);
                Text = _fileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot open file: " + Environment.NewLine + ex.Message, "Error");
                _fileName = string.Empty;
                Close();
            }
        }

        private void FormEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(_fileName))
                    File.WriteAllText(_fileName, fileViewer.GetText(), GitCommands.Settings.Encoding);
            }
            catch (Exception ex)
            {
                if (MessageBox.Show("Cannot save file: " + Environment.NewLine + ex.Message, "Error", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }

            SavePosition("fileeditor");
        }
    }
}
