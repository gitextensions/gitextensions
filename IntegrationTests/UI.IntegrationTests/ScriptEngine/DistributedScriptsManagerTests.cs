using GitCommands;
using GitUI.ScriptsEngine;

namespace GitExtensions.UITests.Script
{
    [TestFixture]
    public class DistributedScriptsManagerTests
    {
        ////[Test]
        ////public async Task Can_save_settings()
        ////{
        ////    string originalScripts = AppSettings.OwnScripts;

        ////    try
        ////    {
        ////        AppSettings.OwnScripts = "<ArrayOfScriptInfo />";

        ////        DistributedScriptsManager scriptsManager = new(new UserScriptsStorage());
        ////        scriptsManager.Initialize(??);

        ////        List<ScriptInfo> scripts = new();
        ////        scripts.AddRange(scriptsManager.GetScripts());

        ////        scripts.Add(new ScriptInfo()
        ////        {
        ////            Name = "name",
        ////            Command = "cmd",
        ////            Arguments = "args"
        ////        });

        ////        scriptsManager.Save();

        ////        // Verify as a string, as the xml verifier ignores line breaks.
        ////        await Verifier.VerifyXml(xml);
        ////    }
        ////    finally
        ////    {
        ////        AppSettings.OwnScripts = originalScripts;
        ////    }
        ////}
    }
}
