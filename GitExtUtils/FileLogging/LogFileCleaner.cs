using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace GitExtUtils.FileLogging
{
    public class LogFileCleaner
    {
        private readonly ILogger _logger;
        private readonly LogPathGenerator _logPathGenerator;
        private readonly TimeSpan _maxLogAge;

        public LogFileCleaner(ILogger logger, LogPathGenerator logPathGenerator, TimeSpan maxLogAge)
        {
            _logger = logger;
            _logPathGenerator = logPathGenerator;
            _maxLogAge = maxLogAge;
        }

        public void Run()
        {
            try
            {
                WaitToPreventAddedStartupTime();

                var deleteThreshold = DateTime.Now - _maxLogAge;

                new DirectoryInfo(_logPathGenerator.LogPath)
                    .GetFileSystemInfos(_logPathGenerator.GetFilenamePattern())
                    .Where(x => x.CreationTime < deleteThreshold)
                    .ForEach(x => x.Delete());
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
            }
        }

        private static void WaitToPreventAddedStartupTime()
        {
            Thread.Sleep(TimeSpan.FromSeconds(10));
        }
    }
}
