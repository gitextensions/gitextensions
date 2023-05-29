using GitCommands;
using GitUI.Script;

namespace GitExtensions.UITests.Script
{
    [TestFixture]
    public class ScriptManagerTests
    {
        [Test]
        public async Task Can_save_settings()
        {
            string originalScripts = AppSettings.OwnScripts;

            try
            {
                AppSettings.OwnScripts = "<ArrayOfScriptInfo />";

                var scripts = ScriptManager.GetScripts();

                scripts.Add(new ScriptInfo()
                {
                    Name = "name",
                    Command = "cmd",
                    Arguments = "args"
                });

                string? xml = ScriptManager.SerializeIntoXml();

                // Verify as a string, as the xml verifier ignores line breaks.
                await Verifier.VerifyXml(xml);
            }
            finally
            {
                AppSettings.OwnScripts = originalScripts;
                ScriptManager.TestAccessor.Reset();
            }
        }
    }
}
