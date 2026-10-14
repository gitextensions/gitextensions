using System.ComponentModel;
using GitCommands;
using GitUI.ScriptsEngine;

namespace GitUITests.Script;

// ShowInUserMenuBar is no longer a ScriptEvent, yet scripts settings saved before still carry it.
public class ScriptsManagerTests
{
    private const string LegacyEvent = "ShowInUserMenuBar";

    private string? _originalScripts;

    [SetUp]
    public void Setup()
    {
        _originalScripts = AppSettings.OwnScripts;
    }

    [TearDown]
    public void TearDown()
    {
        AppSettings.OwnScripts = _originalScripts;
    }

    [Test]
    public void DeserializeFromXml_should_read_the_former_user_menu_bar_event_as_none()
    {
        // The serializer rejects the whole list over a single value it does not know, which would
        // have lost every script of the user, not only the ones shown in the user menu bar.
        string xml = Scripts(("Fetch", "AfterCommit", 9000), ("Open", LegacyEvent, 9001), ("Calc", LegacyEvent, 9002));

        BindingList<ScriptInfo> scripts = ScriptsManager.DeserializeFromXml(xml, out List<int> userMenuBarScriptIndices);

        scripts.Select(script => (script.Name, script.OnEvent, script.HotkeyCommandIdentifier)).Should().Equal(
            ("Fetch", ScriptEvent.AfterCommit, 9000),
            ("Open", ScriptEvent.None, 9001),
            ("Calc", ScriptEvent.None, 9002));
        userMenuBarScriptIndices.Should().Equal(1, 2);
    }

    [Test]
    public void DeserializeFromXml_should_find_no_former_user_menu_bar_event_in_current_settings()
    {
        string xml = Scripts(("Fetch", "AfterCommit", 9000), ("Files", "ShowInFileList", 9001));

        BindingList<ScriptInfo> scripts = ScriptsManager.DeserializeFromXml(xml, out List<int> userMenuBarScriptIndices);

        scripts.Select(script => script.OnEvent).Should().Equal(ScriptEvent.AfterCommit, ScriptEvent.ShowInFileList);
        userMenuBarScriptIndices.Should().BeEmpty();
    }

    [Test]
    public void DeserializeFromXml_should_not_take_a_script_named_after_the_former_event_for_one()
    {
        string xml = Scripts((LegacyEvent, "None", 9000));

        ScriptsManager.DeserializeFromXml(xml, out List<int> userMenuBarScriptIndices).Should().ContainSingle();
        userMenuBarScriptIndices.Should().BeEmpty();
    }

    [Test]
    public void TakeScriptsFormerlyShownInUserMenuBar_should_return_them_only_once()
    {
        AppSettings.OwnScripts = Scripts(("Fetch", "AfterCommit", 9000), ("Open", LegacyEvent, 9001));
        ScriptsManager scriptsManager = new();

        scriptsManager.TakeScriptsFormerlyShownInUserMenuBar().Should().ContainSingle()
            .Which.Should().BeSameAs(scriptsManager.GetScripts()[1]);
        scriptsManager.TakeScriptsFormerlyShownInUserMenuBar().Should().BeEmpty();
    }

    [Test]
    public void TakeScriptsFormerlyShownInUserMenuBar_should_return_the_scripts_with_the_identifier_they_were_given()
    {
        // Loading gives a script whose identifier another script already uses a new one, which is
        // the identifier its toolbar button is then named after.
        AppSettings.OwnScripts = Scripts(("Fetch", "AfterCommit", 9000), ("Open", LegacyEvent, 9000));
        ScriptsManager scriptsManager = new();

        scriptsManager.TakeScriptsFormerlyShownInUserMenuBar().Should().ContainSingle()
            .Which.HotkeyCommandIdentifier.Should().Be(9001);
    }

    [Test]
    public void SerializeIntoXml_should_no_longer_write_the_former_event()
    {
        AppSettings.OwnScripts = Scripts(("Open", LegacyEvent, 9001));
        ScriptsManager scriptsManager = new();
        scriptsManager.GetScripts();

        scriptsManager.SerializeIntoXml().Should().NotContain(LegacyEvent);
    }

    // Scripts settings as the serializer writes them, with only the members these tests look at.
    private static string Scripts(params (string Name, string OnEvent, int Id)[] scripts)
    {
        IEnumerable<string> scriptElements = scripts.Select(script =>
            $"<ScriptInfo><Enabled>true</Enabled><Name>{script.Name}</Name><OnEvent>{script.OnEvent}</OnEvent>"
            + $"<HotkeyCommandIdentifier>{script.Id}</HotkeyCommandIdentifier></ScriptInfo>");

        return """<?xml version="1.0" encoding="utf-16"?>"""
            + """<ArrayOfScriptInfo xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">"""
            + string.Concat(scriptElements)
            + "</ArrayOfScriptInfo>";
    }
}
