using System.Collections.Generic;
using System.Text;

namespace GitCommands.Logging
{
    public class CommandLogger
    {
        private const int LogLimit = 100;
        private Queue<string> _logQueue = new Queue<string>(LogLimit);

        public void Log(string command)
        {
            try
            {
                if (_logQueue.Count >= LogLimit)
                    _logQueue.Dequeue();

                _logQueue.Enqueue(command);
            }
            catch //This should NEVER happen... but it did happen (issue 271)
            {
                _logQueue = new Queue<string>(LogLimit);
            }
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