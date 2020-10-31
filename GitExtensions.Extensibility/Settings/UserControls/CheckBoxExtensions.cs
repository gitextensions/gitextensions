using System.Windows.Forms;

namespace GitExtensions.Extensibility.Settings.UserControls
{
    public static class CheckBoxExtensions
    {
        public static bool? GetNullableChecked(this CheckBox chx)
        {
            if (chx.CheckState == CheckState.Indeterminate)
            {
                return null;
            }

            return chx.Checked;
        }

        public static void SetNullableChecked(this CheckBox chx, bool? @checked)
        {
            if (@checked.HasValue)
            {
                chx.CheckState = @checked.Value ? CheckState.Checked : CheckState.Unchecked;
            }
            else
            {
                chx.CheckState = CheckState.Indeterminate;
            }
        }
    }
}
