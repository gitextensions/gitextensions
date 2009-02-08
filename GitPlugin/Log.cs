// Copyright (C) 2006-2008 Jim Tilander. See COPYING for and README for more details.
using System;
using System.Collections.Generic;
using System.Text;

namespace Aurora
{
	public static class Log
	{
		// Internal enumeration. Only used in handlers to identify the type of message
		public enum Level
		{
			Debug,
			Info,
			Warn,
			Error,
		}

		// This needs to be implemented by all clients.
		public interface Handler
		{
			void OnMessage(Level level, string message, string formattedLine);
		}

		// Helper class to keep the indent levels balanced (with the help of the using statement)


		// Log class implement below
		public static string Prefix
		{
			get { return mPrefix; }
			set { mPrefix = value; }
		}

		public static int HandlerCount
		{
			get { return mHandlers.Count; }
		}

		static Log()
		{
			mHandlers = new List<Handler>();
			mPrefix = "";
			mIndent = 0;
		}

		public static void AddHandler(Handler handler)
		{
			if(null == handler)
				return;

			lock(mHandlers)
			{
				mHandlers.Add(handler);
			}
		}

		public static void RemoveHandler(Handler handler)
		{
			lock(mHandlers)
			{
				mHandlers.Remove(handler);
			}
		}

		public static void ClearHandlers()
		{
			lock(mHandlers)
			{
				mHandlers.Clear();
			}
		}

		public static void IncIndent()
		{
			mIndent++;
		}

		public static void DecIndent()
		{
			mIndent--;
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
			string message = string.Format(format, args);
			string formattedLine;
			string indent = "";
			string levelName = level.ToString().PadLeft(5, ' ');

			for(int i = 0; i < mIndent; i++)
			{
				indent += "    ";
			}

			if(mPrefix.Length > 0)
			{
				formattedLine = mPrefix + " (" + levelName + "): " + indent + message + "\n";
			}
			else
			{
				formattedLine = levelName + ": " + indent + message + "\n";
			}

			lock(mHandlers)
			{
				foreach(Handler handler in mHandlers)
				{
					handler.OnMessage(level, message, formattedLine);
				}
			}
		}

		private static List<Handler> mHandlers;
		private static string mPrefix;
		private static int mIndent;
	}
}
