using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public partial class FormAvailableEncodings : GitExtensionsForm
    {
        public FormAvailableEncodings()
        {
            InitializeComponent();
            InitializeComplete();
            LoadEncoding();
        }

        private void LoadEncoding()
        {
            var includedEncoding = AppSettings.AvailableEncodings;
            ListIncludedEncodings.BeginUpdate();
            try
            {
                ListIncludedEncodings.Items.AddRange(includedEncoding.Values.ToArray<object>());
                ListIncludedEncodings.DisplayMember = nameof(Encoding.EncodingName);
            }
            finally
            {
                ListIncludedEncodings.EndUpdate();
            }

            var availableEncoding = Encoding.GetEncodings()
                .Select(ei => ei.GetEncoding())
#pragma warning disable SYSLIB0001 // Type or member is obsolete
                .Where(e => e != Encoding.UTF7) // UTF-7 is no longer supported, see: https://github.com/dotnet/docs/issues/19274
#pragma warning restore SYSLIB0001 // Type or member is obsolete
                .Where(e => !includedEncoding.ContainsKey(e.HeaderName))
                .ToList();

            // If exists utf-8, then replace to utf-8 without BOM
            var utf8 = availableEncoding.FirstOrDefault(e => typeof(UTF8Encoding) == e.GetType());
            if (utf8 is not null)
            {
                availableEncoding.Remove(utf8);
                availableEncoding.Add(new UTF8Encoding(false));
            }

            ListAvailableEncodings.BeginUpdate();
            try
            {
                ListAvailableEncodings.Items.AddRange(availableEncoding.ToArray<object>());
                ListAvailableEncodings.DisplayMember = nameof(Encoding.EncodingName);
            }
            finally
            {
                ListAvailableEncodings.EndUpdate();
            }
        }

        private void ToLeft_Click(object sender, EventArgs e)
        {
            if (ListAvailableEncodings.SelectedItem is not null)
            {
                var index = ListAvailableEncodings.SelectedIndex;
                ListIncludedEncodings.Items.Add(ListAvailableEncodings.SelectedItem);
                ListAvailableEncodings.Items.RemoveAt(index);
            }
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            AppSettings.AvailableEncodings.Clear();
            foreach (Encoding encoding in ListIncludedEncodings.Items)
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
            if (ListIncludedEncodings.SelectedItem is not null)
            {
                var index = ListIncludedEncodings.SelectedIndex;
                ListAvailableEncodings.Items.Add(ListIncludedEncodings.SelectedItem);
                ListIncludedEncodings.Items.RemoveAt(index);
            }
        }

        private void ListIncludedEncodings_SelectedValueChanged(object sender, EventArgs e)
        {
            // Get selected encoding
            var encoding = ListIncludedEncodings.SelectedItem as Encoding;
            Type? encodingType = null;

            // Get type if exists
            if (encoding is not null)
            {
                encodingType = encoding.GetType();
            }

            // If selected encoding and encoding not default list
            ToRight.Enabled = encoding is not null &&
                !(
                    encodingType == typeof(ASCIIEncoding) ||
                    encodingType == typeof(UnicodeEncoding) ||
                    encodingType == typeof(UTF8Encoding) ||
                    encodingType == typeof(UTF7Encoding) ||
                    encoding == Encoding.Default);
        }

        private void ListAvailableEncodings_SelectedValueChanged(object sender, EventArgs e)
        {
            ToLeft.Enabled = ListAvailableEncodings.SelectedItem is not null;
        }
    }
}
