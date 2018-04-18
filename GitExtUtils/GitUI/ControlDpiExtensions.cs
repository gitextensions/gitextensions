using System;
using System.Collections.Generic;
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

            if (!DpiUtil.IsNonStandard)
            {
                return;
            }

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
                        if (button.Image != null)
                        {
                            button.Image = DpiUtil.Scale(button.Image);
                        }

                        break;
                    }

                    case PictureBox pictureBox:
                    {
                        if (pictureBox.Image != null)
                        {
                            pictureBox.Image = DpiUtil.Scale(pictureBox.Image);
                        }

                        break;
                    }

                    case TabControl tabControl:
                    {
                        tabControl.Padding = DpiUtil.Scale(tabControl.Padding);
                        EnqueueChildren();
                        break;
                    }

                    default:
                    {
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