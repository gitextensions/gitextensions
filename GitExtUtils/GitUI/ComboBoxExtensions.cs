using System;
using System.Windows.Forms;

namespace GitUI
{
    public static class ComboBoxExtensions
    {
        public static void AdjustWidthToFitContent(this ComboBox comboBox)
        {
            if (comboBox == null)
            {
                throw new ArgumentNullException(nameof(comboBox));
            }

            var width = GetPreferredDropDownWidth(comboBox);

            comboBox.Width = width;

            if (width != 0)
            {
                comboBox.DropDownWidth = width;
            }
        }

        public static void ResizeDropDownWidth(this ComboBox comboBox, int minWidth, int maxWidth)
        {
            if (comboBox == null)
            {
                throw new ArgumentNullException(nameof(comboBox));
            }

            var calculatedWidth = GetPreferredDropDownWidth(comboBox);
            comboBox.DropDownWidth = Math.Min(Math.Max(calculatedWidth, minWidth), maxWidth);
        }

        public static void ResizeDropDownWidth(this ToolStripComboBox comboBox, int minWidth, int maxWidth)
        {
            if (comboBox == null)
            {
                throw new ArgumentNullException(nameof(comboBox));
            }

            var calculatedWidth = GetPreferredDropDownWidth(comboBox.Control);
            comboBox.DropDownWidth = Math.Min(Math.Max(calculatedWidth, minWidth), maxWidth);
        }

        private static int GetPreferredDropDownWidth(dynamic comboBox)
        {
            var calculatedWidth = 0;
            using (var graphics = comboBox.CreateGraphics())
            {
                foreach (object obj in comboBox.Items)
                {
                    var area = graphics.MeasureString(obj.ToString(), comboBox.Font);
                    calculatedWidth = Math.Max((int)area.Width, calculatedWidth);
                }
            }

            return calculatedWidth;
        }
    }
}