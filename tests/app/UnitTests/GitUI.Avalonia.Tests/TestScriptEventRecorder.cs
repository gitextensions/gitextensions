using System.ComponentModel.Design;
using GitExtensions.Extensibility.Git;
using GitExtensions.Shims.WinForms;
using GitUI.ScriptsEngine;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitExtensionsTests;

internal sealed class TestScriptEventRecorder : IScriptsRunner
{
    internal List<ScriptEvent> Events { get; } = [];

    internal HashSet<ScriptEvent> CancelledEvents { get; } = [];

    internal static TestScriptEventRecorder Install(ServiceContainer serviceContainer)
    {
        TestScriptEventRecorder recorder = new();
        serviceContainer.RemoveService(typeof(IScriptsRunner));
        serviceContainer.AddService(typeof(IScriptsRunner), recorder);
        return recorder;
    }

    public bool RunEventScripts<THostForm>(ScriptEvent scriptEvent, THostForm form)
        where THostForm : IGitModuleForm, IScriptOptionsForm, IWin32Window
    {
        Events.Add(scriptEvent);
        return !CancelledEvents.Contains(scriptEvent);
    }

    public bool RunScript(
        ScriptInfo scriptInfo,
        IWin32Window owner,
        IGitUICommands commands,
        IScriptOptionsProvider scriptOptionsProvider)
        => true;
}
