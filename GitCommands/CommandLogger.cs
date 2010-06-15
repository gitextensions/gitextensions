using System.Text;
using System.Collections.Generic;

namespace GitCommands
{
    public class CommandLogger
    {
        const int logLimit = 100;
        private Queue<string> logQueue = new Queue<string>(logLimit);

        public void Log(string command)
        {
            if (logQueue.Count >= logLimit)
                logQueue.Dequeue();

            logQueue.Enqueue(command);           
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            
            foreach (string loggedCmd in logQueue.ToArray())
            {
                stringBuilder.AppendLine(loggedCmd);
            }

            return stringBuilder.ToString();
        }
    }
}