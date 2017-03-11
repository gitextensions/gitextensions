using System;
using System.Windows.Forms;

namespace GitUI.UserControls
{
    public static class ComboBoxHelper
    {
        public static void ResizeComboBoxDropDownWidth(ComboBox comboBox, int minWidth, int maxWidth)
        {
            var calculatedWidth = 0;
            using (var graphics = comboBox.CreateGraphics())
            {
                foreach (object obj in comboBox.Items)
                {
                    var area = graphics.MeasureString(obj.ToString(), comboBox.Font);
                    calculatedWidth = Math.Max((int) area.Width, calculatedWidth);
                }
            }
            comboBox.DropDownWidth = Math.Min(Math.Max(calculatedWidth, minWidth), maxWidth);
        }
    }
}