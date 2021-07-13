using System;
using System.Windows.Forms;
using static System.NativeMethods;

namespace GitUI.UserControls
{
    public class NativeListView : System.Windows.Forms.ListView
    {
        internal static event EventHandler? BeginCreateHandle;
        internal static event EventHandler? EndCreateHandle;
        internal event ScrollEventHandler? Scroll;

        public NativeListView()
        {
            DoubleBuffered = true;
        }

        protected override void CreateHandle()
        {
            BeginCreateHandle?.Invoke(this, EventArgs.Empty);
            base.CreateHandle();
            NativeMethods.SetWindowTheme(Handle, "explorer", null);
            EndCreateHandle?.Invoke(this, EventArgs.Empty);
        }

        protected override void WndProc(ref Message m)
        {
            var message = m;
            switch (m.Msg)
            {
                default:
                    HandleScroll(m);
                    base.WndProc(ref m);
                    break;
            }

            void HandleScroll(Message msg)
            {
                ScrollEventType type;
                int? newValue = null;

                switch (msg.Msg)
                {
                    case WM_VSCROLL:
                        type = (ScrollEventType)LowWord(msg.WParam.ToInt64());
                        newValue = HighWord(msg.WParam.ToInt64());
                        break;

                    case WM_MOUSEWHEEL:
                        type = HighWord(msg.WParam.ToInt64()) > 0
                            ? ScrollEventType.SmallDecrement
                            : ScrollEventType.SmallIncrement;
                        break;

                    case WM_KEYDOWN:
                        switch ((Keys)(int)(long)msg.WParam)
                        {
                            case Keys.Up:
                                type = ScrollEventType.SmallDecrement;
                                break;
                            case Keys.Down:
                                type = ScrollEventType.SmallIncrement;
                                break;
                            case Keys.PageUp:
                                type = ScrollEventType.LargeDecrement;
                                break;
                            case Keys.PageDown:
                                type = ScrollEventType.LargeIncrement;
                                break;
                            case Keys.Home:
                                type = ScrollEventType.First;
                                break;
                            case Keys.End:
                                type = ScrollEventType.Last;
                                break;
                            default:
                                return;
                        }

                        break;

                    default:
                        return;
                }

                newValue ??= GetScrollPos(Handle, SB.VERT);
                Scroll?.Invoke(this, new ScrollEventArgs(type, newValue.Value));

                short LowWord(long number) =>
                    unchecked((short)(number & 0x0000ffff));

                short HighWord(long number) =>
                    unchecked((short)(number >> 16));
            }
        }
    }
}
