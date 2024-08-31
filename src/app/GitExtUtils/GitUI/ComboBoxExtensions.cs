using System.Runtime.InteropServices;

namespace GitUI
{
    public static class ComboBoxExtensions
    {
        public static void AdjustWidthToFitContent(this ComboBox comboBox)
        {
            if (comboBox is null)
            {
                throw new ArgumentNullException(nameof(comboBox));
            }

            int width = GetPreferredDropDownWidth(comboBox);

            comboBox.Width = width;

            if (width != 0)
            {
                comboBox.DropDownWidth = width;
            }
        }

        public static void ResizeDropDownWidth(this ComboBox comboBox, int minWidth, int maxWidth)
        {
            ArgumentNullException.ThrowIfNull(comboBox);

            int calculatedWidth = GetPreferredDropDownWidth(comboBox);
            comboBox.DropDownWidth = Math.Min(Math.Max(calculatedWidth, minWidth), maxWidth);
        }

        public static void ResizeDropDownWidth(this ToolStripComboBox comboBox, int minWidth, int maxWidth)
        {
            ArgumentNullException.ThrowIfNull(comboBox);

            int calculatedWidth = GetPreferredDropDownWidth((ComboBox)comboBox.Control);
            comboBox.DropDownWidth = Math.Min(Math.Max(calculatedWidth, minWidth), maxWidth);
        }

        private static int GetPreferredDropDownWidth(ComboBox comboBox)
        {
            int calculatedWidth = 0;
            using Graphics graphics = comboBox.CreateGraphics();

            string displayMemberName = comboBox.DisplayMember;
            Font font = comboBox.Font;

            foreach (object item in comboBox.Items)
            {
                SizeF area = graphics.MeasureString(GetDisplayValue(displayMemberName, item), font);
                calculatedWidth = Math.Max((int)area.Width, calculatedWidth);
            }

            return calculatedWidth + CalculateVerticalScrollBarWidth(comboBox);
        }

        private static string GetDisplayValue(string displayMemberName, object item)
        {
            if (displayMemberName.Length == 0)
            {
                return item.ToString();
            }

            System.Reflection.PropertyInfo displayMemberProperty = item
                .GetType()
                .GetProperty(displayMemberName);

            if (displayMemberProperty is null)
            {
                return item.ToString();
            }

            return displayMemberProperty
                .GetValue(item)
                ?.ToString();
        }

        private static int CalculateVerticalScrollBarWidth(ComboBox comboBox)
        {
            NativeMethods.COMBOBOXINFO cboInfo = NativeMethods.COMBOBOXINFO.Create();
            if (!NativeMethods.GetComboBoxInfo(comboBox.Handle, ref cboInfo))
            {
                return 0;
            }

            int listStyle = NativeMethods.GetWindowLong(cboInfo.hwndList, NativeMethods.GWL_STYLE);
            bool hasVerticalScrollBar = (listStyle & NativeMethods.WS_VSCROLL) == NativeMethods.WS_VSCROLL;

            return hasVerticalScrollBar
                ? SystemInformation.VerticalScrollBarWidth
                : 0;
        }
    }
}
