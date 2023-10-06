using GitUIPluginInterfaces;

namespace ResourceManager
{
    public interface IGitModuleForm
    {
        IGitUICommands UICommands { get; }
    }
}
