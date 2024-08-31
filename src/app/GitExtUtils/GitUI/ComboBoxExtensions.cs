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

            Font font = comboBox.Font;

            foreach (object item in comboBox.Items)
            {
                SizeF area = graphics.MeasureString(item.ToString(), font);
                calculatedWidth = Math.Max((int)area.Width, calculatedWidth);
            }

            return calculatedWidth;
        }
    }
}
