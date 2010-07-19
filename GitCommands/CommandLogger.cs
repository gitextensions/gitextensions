using System.Collections.Generic;
using System.Text;

namespace GitCommands
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

            foreach (var loggedCmd in _logQueue)
            {
                stringBuilder.AppendLine(loggedCmd);
            }

            return stringBuilder.ToString();
        }
    }
}