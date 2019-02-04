using System.Text;
using GitCommands;

namespace GitUIPluginInterfaces
{
    public interface IGitCommandRunner
    {
        IProcess RunDetached(
            ArgumentString arguments = default,
            bool createWindow = false,
            bool redirectInput = false,
            bool redirectOutput = false,
            Encoding outputEncoding = null);
    }
}