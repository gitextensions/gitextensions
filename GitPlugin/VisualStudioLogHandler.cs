// Copyright (C) 2006-2008 Jim Tilander. See COPYING for and README for more details.
using System;
using EnvDTE;
using EnvDTE80;
using System.Collections.Generic;
using System.Text;

namespace Aurora
{
	public class VisualStudioLogHandler : Log.Handler
	{
		private OutputWindowPane mPane;

		public VisualStudioLogHandler(OutputWindowPane pane)
		{
			mPane = pane;
		}

		public void OnMessage(Log.Level level, string message, string formattedLine)
		{
			if (null == mPane)
				return;

			mPane.OutputString(formattedLine);
		}
	}
}
