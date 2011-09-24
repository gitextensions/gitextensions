using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public static class ControlExtensions
    {

        public static bool? GetNullableChecked(this CheckBox chx)
        {
            if (chx.CheckState == CheckState.Indeterminate)
                return null;
            else
                return chx.Checked;

        }

        public static void SetNullableChecked(this CheckBox chx, bool? Checked)
        {
            if (Checked.HasValue)
                chx.Checked = Checked.Value;
            else
                chx.CheckState = CheckState.Indeterminate;

        }
    }
}
