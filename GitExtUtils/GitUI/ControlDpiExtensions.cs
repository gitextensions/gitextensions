using System;
using System.Collections.Generic;
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

            var queue = new Queue<Control>();
            queue.Enqueue(control);

            while (queue.Count != 0)
            {
                var next = queue.Dequeue();

                // NOTE we can't automatically scale TreeView or ListView here as
                // adjustment must be done before images are added to the
                // ImageList otherwise they're all removed.

                switch (next)
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

                        EnqueueChildren();
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
                        EnqueueChildren();
                        break;
                    }

                    default:
                    {
                        if (next is TextBoxBase || next is UpDownBase)
                        {
                            // BUG: looks like a bug in WinForms - control's margin gets scaled beyond expectations
                            // see https://github.com/gitextensions/gitextensions/issues/5098
                            DpiUtil.ScaleDefaultMargins(next);
                        }

                        EnqueueChildren();
                        break;
                    }
                }

                void EnqueueChildren()
                {
                    foreach (Control child in next.Controls)
                    {
                        queue.Enqueue(child);
                    }
                }
            }
        }
    }
}