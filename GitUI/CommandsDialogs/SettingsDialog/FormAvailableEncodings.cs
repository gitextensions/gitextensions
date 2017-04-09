using GitCommands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public partial class FormAvailableEncodings : GitModuleForm
    {
        public FormAvailableEncodings()
        {
            InitializeComponent();
            Translate();
            LoadEncoding();
        }

        private void LoadEncoding()
        {
            var includedEncoding = AppSettings.AvailableEncodings;
            ListIncludedEncodings.Items.AddRange(includedEncoding.Values.ToArray());
            ListIncludedEncodings.DisplayMember = "EncodingName";

            var availableEncoding = System.Text.Encoding.GetEncodings()
                .Select(ei => ei.GetEncoding())
                .Where(e => !includedEncoding.ContainsKey(e.HeaderName)).ToList();
            var utf8 = availableEncoding.Where(e => typeof(UTF8Encoding) == e.GetType()).FirstOrDefault();
            if (utf8 != null)
            {
                availableEncoding.Remove(utf8);
                availableEncoding.Add(new UTF8Encoding(false));
            }
            ListAvailableEncodings.Items.AddRange(availableEncoding.OrderBy(e => e.HeaderName).ToArray());
            ListAvailableEncodings.DisplayMember = "EncodingName";
        }

        private void ToLeft_Click(object sender, EventArgs e)
        {
            if (ListAvailableEncodings.SelectedItem != null)
            {
                var indx = ListAvailableEncodings.SelectedIndex;
                ListIncludedEncodings.Items.Add(ListAvailableEncodings.SelectedItem);
                ListAvailableEncodings.Items.RemoveAt(indx);
            }
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            AppSettings.AvailableEncodings.Clear();
            foreach(Encoding encoding in ListIncludedEncodings.Items)
            {
                AppSettings.AvailableEncodings.Add(encoding.HeaderName, encoding);
            }

            DialogResult = DialogResult.OK;
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void ToRight_Click(object sender, EventArgs e)
        {
            if (ListIncludedEncodings.SelectedItem != null)
            {
                var indx = ListIncludedEncodings.SelectedIndex;
                ListAvailableEncodings.Items.Add(ListIncludedEncodings.SelectedItem);
                ListIncludedEncodings.Items.RemoveAt(indx);
            }
        }
    }
}
