using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using GitExtUtils.GitUI;
using JetBrains.Annotations;

namespace GitUI
{
    public static class ControlDpiExtensions
    {
        public static void AdjustForDpiScaling([NotNull] this Control control)
        {
            if (control == null)
            {
                throw new ArgumentNullException(nameof(control));
            }

            var isDpiScaled = DpiUtil.IsNonStandard;

            // If we are in design mode, don't scale anything as the designer may
            // write scaled values back to InitializeComponent.
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            foreach (var descendant in control.FindDescendants())
            {
                // NOTE we can't automatically scale TreeView or ListView here as
                // adjustment must be done before images are added to the
                // ImageList otherwise they're all removed.
                switch (descendant)
                {
                    case ButtonBase button:
                    {
                        if (isDpiScaled && button.Image != null)
                        {
                            button.Image = DpiUtil.Scale(button.Image);
                            button.Padding = DpiUtil.Scale(new Padding(4, 0, 4, 0));
                        }

                        break;
                    }

                    case PictureBox pictureBox:
                    {
                        if (isDpiScaled && pictureBox.Image != null)
                        {
                            pictureBox.Image = DpiUtil.Scale(pictureBox.Image);
                        }

                        break;
                    }

                    case TabControl tabControl:
                    {
                        if (!isDpiScaled)
                        {
                            tabControl.Padding = new Point(8, 6);
                        }
                        else if (tabControl.Tag as string != "__DPI_SCALED__")
                        {
                            tabControl.Tag = "__DPI_SCALED__";
                            tabControl.Padding = DpiUtil.Scale(new Point(8, 6));
                        }

                        break;
                    }

                    case SplitContainer splitContainer:
                    {
                        const int splitterWidth = 8;

                        if (!isDpiScaled)
                        {
                            splitContainer.SplitterWidth = splitterWidth;
                        }
                        else if (splitContainer.Tag as string != "__DPI_SCALED__")
                        {
                            splitContainer.Tag = "__DPI_SCALED__";
                            splitContainer.SplitterWidth = DpiUtil.Scale(splitterWidth);
                        }

                        splitContainer.BackColor = Color.FromArgb(218, 218, 218);
                        break;
                    }

                    case TextBoxBase textBox when textBox.Margin == new Padding(12):
                    {
                        // Work around a bug in WinForms where the control's margin gets scaled beyond expectations
                        // see https://github.com/gitextensions/gitextensions/issues/5098
                        textBox.Margin = DpiUtil.Scale(new Padding(3));
                        break;
                    }

                    case UpDownBase upDown when upDown.Margin == new Padding(96):
                    {
                        // Work around a bug in WinForms where the control's margin gets scaled beyond expectations
                        // see https://github.com/gitextensions/gitextensions/issues/5098
                        upDown.Margin = DpiUtil.Scale(new Padding(3));
                        break;
                    }
                }
            }
        }
    }
}