using System.Collections.Generic;
using System.Text;

namespace GitCommands.Logging
{
    public class CommandLogger
    {
        private const int LogLimit = 100;
        private readonly Queue<string> _logQueue = new Queue<string>(LogLimit);

        public void Log(string command)
        {
            if (_logQueue.Count >= LogLimit)
                _logQueue.Dequeue();

            _logQueue.Enqueue(command);
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            var logQueueArray = _logQueue.ToArray();

            for (int n = logQueueArray.Length; n > 0; n--)
            {
                stringBuilder.AppendLine(logQueueArray[n - 1]);
            }

            return stringBuilder.ToString();
        }
    }
}