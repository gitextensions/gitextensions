using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public partial class FormDirtyDirWarn : GitExtensionsForm
    {
        public FormDirtyDirWarn()
        {
            InitializeComponent();
            Translate();
            picBoxWarn.Image = SystemIcons.Warning.ToBitmap();
        }
    }
}
