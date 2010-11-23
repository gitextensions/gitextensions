using System.Diagnostics;

namespace GitPlugin
{
    /// <summary>
    ///   Aurora.DebugLogHandler
    ///   Copyright (C) 2006-2008 Jim Tilander. See COPYING for and README for more details.
    /// </summary>
    public class DebugLogHandler : Log.IHandler
    {
        #region Handler Members

        public void OnMessage(Log.Level level, string message, string formattedLine)
        {
            Debug.Write(formattedLine);
        }

        #endregion
    }
}