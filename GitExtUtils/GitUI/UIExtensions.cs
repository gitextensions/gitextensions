using System.Windows.Forms;

namespace GitUI
{
    public static class UIExtensions
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
                chx.CheckState = Checked.Value ? CheckState.Checked : CheckState.Unchecked;
            else
                chx.CheckState = CheckState.Indeterminate;
        }
    }
}
