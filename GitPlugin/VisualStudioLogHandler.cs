using EnvDTE;

namespace GitPlugin
{
    public class VisualStudioLogHandler : Log.IHandler
    {
        private readonly OutputWindowPane _outputWindowPane;

        public VisualStudioLogHandler(OutputWindowPane pane)
        {
            _outputWindowPane = pane;
        }

        #region Handler Members

        public void OnMessage(Log.Level level, string message, string formattedLine)
        {
            _outputWindowPane?.OutputString(formattedLine);
        }

        #endregion
    }
}