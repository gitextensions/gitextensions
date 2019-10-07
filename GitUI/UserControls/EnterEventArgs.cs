using System;

namespace GitUI
{
    public class EnterEventArgs : EventArgs
    {
        public bool ByMouse { get; }

        public EnterEventArgs(bool byMouse)
        {
            ByMouse = byMouse;
        }
    }
}
