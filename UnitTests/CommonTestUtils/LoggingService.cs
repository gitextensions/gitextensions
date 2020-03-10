using System;
using System.Diagnostics;
using System.Text;

namespace CommonTestUtils
{
    public class LoggingService : IDisposable
    {
        private readonly StringBuilder _stringBuilder = new StringBuilder();

        public bool AutoFlush { get; set; } = true;

        public void Dispose() => Flush();

        public LoggingService Flush()
        {
            lock (_stringBuilder)
            {
                if (_stringBuilder.Length == 0)
                {
                    return this;
                }

                Console.Write(_stringBuilder);
                _stringBuilder.Clear();
            }

            return this;
        }

        public LoggingService Log(string message, bool debugOnly = true)
        {
#if !DEBUG
            if (debugOnly)
            {
                return this;
            }
#endif
            lock (_stringBuilder)
            {
                _stringBuilder.Append(DateTime.Now).Append(": ").AppendLine(message);

                if (AutoFlush)
                {
                    Flush();
                }
            }

            return this;
        }

        public LoggingService Log(string message, Exception ex)
            => Log($"{message}: {ex.Demystify()}", debugOnly: false);
    }
}
