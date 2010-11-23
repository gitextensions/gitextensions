using System.Collections.Generic;

namespace GitPlugin
{
    /// <summary>
    ///   Aurora.Log
    ///   Copyright (C) 2006-2008 Jim Tilander. See COPYING for and README for more details.
    /// </summary>
    public static class Log
    {
        // Internal enumeration. Only used in handlers to identify the type of message

        #region Level enum

        public enum Level
        {
            Debug,
            Info,
            Warn,
            Error,
        }

        #endregion

        private static readonly List<IHandler> Handlers;
        private static int _indent;

        static Log()
        {
            Handlers = new List<IHandler>();
            Prefix = "";
            _indent = 0;
        }

        // This needs to be implemented by all clients.
        // Helper class to keep the indent levels balanced (with the help of the using statement)
        // Log class implement below

        public static string Prefix { get; set; }

        public static int HandlerCount
        {
            get { return Handlers.Count; }
        }

        public static void AddHandler(IHandler handler)
        {
            if (null == handler)
                return;

            lock (Handlers)
            {
                Handlers.Add(handler);
            }
        }

        public static void RemoveHandler(IHandler handler)
        {
            lock (Handlers)
            {
                Handlers.Remove(handler);
            }
        }

        public static void ClearHandlers()
        {
            lock (Handlers)
            {
                Handlers.Clear();
            }
        }

        public static void IncIndent()
        {
            _indent++;
        }

        public static void DecIndent()
        {
            _indent--;
        }

        public static void Debug(string message, params object[] args)
        {
#if DEBUG
            OnMessage(Level.Debug, message, args);
#endif
        }

        public static void Info(string message, params object[] args)
        {
            OnMessage(Level.Info, message, args);
        }

        public static void Warning(string message, params object[] args)
        {
            OnMessage(Level.Warn, message, args);
        }

        public static void Error(string message, params object[] args)
        {
            OnMessage(Level.Error, message, args);
        }

        private static void OnMessage(Level level, string format, object[] args)
        {
            var message = string.Format(format, args);
            string formattedLine;
            var indent = "";
            var levelName = level.ToString().PadLeft(5, ' ');

            for (var i = 0; i < _indent; i++)
            {
                indent += "    ";
            }

            if (Prefix.Length > 0)
            {
                formattedLine = Prefix + " (" + levelName + "): " + indent + message + "\n";
            }
            else
            {
                formattedLine = levelName + ": " + indent + message + "\n";
            }

            lock (Handlers)
            {
                foreach (var handler in Handlers)
                {
                    handler.OnMessage(level, message, formattedLine);
                }
            }
        }

        #region Nested type: Handler

        public interface IHandler
        {
            void OnMessage(Level level, string message, string formattedLine);
        }

        #endregion
    }
}