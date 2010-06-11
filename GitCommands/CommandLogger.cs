using System.Text;

namespace GitCommands
{
    public class CommandLogger
    {
        // TODO: Replace the string builder with a rolling collection (keep X latest commands)
        private readonly StringBuilder _commands = new StringBuilder();

        public void Log(string command)
        {
            _commands.AppendLine(command);
        }

        public override string ToString()
        {
            return _commands.ToString();
        }
    }
}