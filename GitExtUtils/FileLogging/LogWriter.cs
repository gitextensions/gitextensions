using System;
using System.Collections.Concurrent;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Text;
using System.Threading;

namespace GitExtUtils.FileLogging
{
    public interface ILogWriter
    {
        void Enqueue(LogEntry entry);
    }

    public class LogWriter : ILogWriter
    {
        private readonly LogPathGenerator _logPathGenerator;
        private readonly ILogFormatter _formatter;
        private readonly TimeSpan _flushInterval;
        private readonly ConcurrentQueue<LogEntry> _unsaved = new ConcurrentQueue<LogEntry>();

        public LogWriter(
            LogPathGenerator logPathGenerator,
            ILogFormatter formatter,
            TimeSpan flushInterval)
        {
            _logPathGenerator = logPathGenerator;
            _formatter = formatter;
            _flushInterval = flushInterval;
        }

        public void Enqueue(LogEntry entry)
        {
            _unsaved.Enqueue(entry);
        }

        public void Run(CancellationToken cancellationToken)
        {
            try
            {
                if (!Directory.Exists(_logPathGenerator.LogPath))
                {
                    Directory.CreateDirectory(_logPathGenerator.LogPath);
                }
            }
            catch (Exception e)
            {
                throw new Exception(message: $"Cannot create folder for log files", e);
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    cancellationToken.WaitHandle.WaitOne(_flushInterval);

                    WriteUnsaved();
                }
                catch (Exception e)
                {
                    throw new Exception($"Cannot save entries to disc.", e);
                }
            }
        }

        private void WriteUnsaved()
        {
            int max = _unsaved.Count;
            if (max <= 0)
            {
                return;
            }

            var sb = new StringBuilder(max * 1000);
            int i = 0;
            while (i++ < max && _unsaved.TryDequeue(out var entry))
            {
                sb.AppendLine(_formatter.Format(entry));
            }

            using (var file = new FileStream(_logPathGenerator.GenerateFullPath(DateTime.Now), FileMode.Append, FileAccess.Write, FileShare.Read))
            using (var writer = new StreamWriter(file, Encoding.Unicode))
            {
                writer.Write(sb.ToString());
            }
        }
    }
}