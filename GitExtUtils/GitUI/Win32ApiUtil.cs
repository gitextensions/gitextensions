﻿namespace GitUI
{
    public static class Win32ApiUtil
    {
        /// <summary>
        /// Convert <see cref="Message.LParam"/> to <see cref="Point"/>.
        /// </summary>
        public static Point ToPoint(this IntPtr lparam) =>
            new(unchecked((int)lparam.ToInt64()));
    }
}
