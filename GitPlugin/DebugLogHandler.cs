// Copyright (C) 2006-2008 Jim Tilander. See COPYING for and README for more details.
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Aurora
{
	public class DebugLogHandler : Log.Handler
	{
		public void OnMessage(Log.Level level, string message, string formattedLine)
		{
			Debug.Write(formattedLine);
		}
	}
}
