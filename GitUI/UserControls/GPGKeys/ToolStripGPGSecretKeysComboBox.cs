using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GitUI.UserControls.GPGKeys
{
    public class ToolStripGPGSecretKeysComboBox : ToolStripControlHost
    {
        private GPGSecretKeysCombobox cmbo { get => (GPGSecretKeysCombobox)Control; }
        public ToolStripGPGSecretKeysComboBox() : base(new GPGSecretKeysCombobox())
        {
            // left blank for now
        }

        public string KeyID { get => cmbo.KeyID; set => cmbo.KeyID = value; }
    }
}
