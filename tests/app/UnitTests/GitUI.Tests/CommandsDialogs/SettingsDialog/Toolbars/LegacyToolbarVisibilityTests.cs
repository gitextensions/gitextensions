using GitUI.CommandsDialogs.SettingsDialog.Toolbars;

namespace GitUITests.CommandsDialogs.SettingsDialog.Toolbars;

// Before toolbars could be customized, View > Toolbars stored one visibility setting per button or
// group. An upgrade must carry the user's choices over into the layout rather than drop them.
public class LegacyToolbarVisibilityTests
{
    private const string Prefix = LegacyToolbarVisibility.SettingPrefix;

    private static LegacyToolbarItem Button(string name, string? group = null) => new(name, group, IsSeparator: false);

    private static LegacyToolbarItem Separator(string name) => new(name, Group: null, IsSeparator: true);

    private static Func<string, bool?> Settings(params (string Key, bool Value)[] settings)
    {
        Dictionary<string, bool> values = settings.ToDictionary(s => Prefix + s.Key, s => s.Value);
        return name => values.TryGetValue(name, out bool value) ? value : null;
    }

    // The item names a toolbar ends up with, inserted shortcuts included.
    private static string[] Apply(LegacyToolbarItem[] items, Func<string, bool?> settings)
        => LegacyToolbarVisibility.Apply(items, settings, out _)
            .Select(slot => slot.SourceIndex is int index ? items[index].Name! : $"+{slot.InsertedItemName}")
            .ToArray();

    private static bool Changes(LegacyToolbarItem[] items, Func<string, bool?> settings)
    {
        LegacyToolbarVisibility.Apply(items, settings, out bool changed);
        return changed;
    }

    private static readonly LegacyToolbarItem[] _standard =
    [
        Button("toolStripButtonLevelUp"),
        Separator("sep1"),
        Button("toolStripButtonCommit"),
        Button("toolStripButtonPull"),
        Button("toolStripButtonPush"),
        Separator("sep2"),
        Button("toolStripFileExplorer"),
        Button("userShell"),
    ];

    [Test]
    public void Apply_should_keep_everything_when_nothing_was_changed()
    {
        Apply(_standard, Settings()).Should().Equal(_standard.Select(i => i.Name));
    }

    [Test]
    public void Apply_should_leave_out_a_button_the_user_hid()
    {
        Apply(_standard, Settings(("toolStripButtonCommit", false)))
            .Should().Equal("toolStripButtonLevelUp", "sep1", "toolStripButtonPull", "toolStripButtonPush", "sep2", "toolStripFileExplorer", "userShell");
    }

    [Test]
    public void Apply_should_keep_a_button_explicitly_shown()
    {
        Apply(_standard, Settings(("toolStripButtonCommit", true))).Should().Equal(_standard.Select(i => i.Name));
    }

    [Test]
    public void Apply_should_leave_out_every_item_of_a_hidden_group()
    {
        LegacyToolbarItem[] filters =
        [
            Button("tssbtnShowBranches"),
            Button("toolStripLabel1", "ToolBar_group:Branch filter"),
            Button("tscboBranchFilter", "ToolBar_group:Branch filter"),
            Button("tsddbtnBranchFilter", "ToolBar_group:Branch filter"),
            Separator("sep"),
            Button("tstxtRevisionFilter", "ToolBar_group:Text filter"),
        ];

        Apply(filters, Settings(("ToolBar_group:Branch filter", false)))
            .Should().Equal("tssbtnShowBranches", "sep", "tstxtRevisionFilter");
    }

    [Test]
    public void Apply_should_read_a_grouped_item_by_its_group_not_its_name()
    {
        // The old code never stored a grouped item under its own name.
        LegacyToolbarItem[] filters = [Button("tscboBranchFilter", "ToolBar_group:Branch filter"), Button("other")];

        Apply(filters, Settings(("tscboBranchFilter", false))).Should().Equal("tscboBranchFilter", "other");
    }

    [Test]
    public void Apply_should_place_the_shortcuts_the_user_showed_before_the_pull_button_in_their_old_order()
    {
        Apply(_standard, Settings(("pull_shortcut_pullToolStripMenuItem1", true), ("pull_shortcut_fetchToolStripMenuItem", true)))
            .Should().Equal(
                "toolStripButtonLevelUp", "sep1", "toolStripButtonCommit",
                "+fetchToolStripMenuItem", "+pullToolStripMenuItem1",
                "toolStripButtonPull", "toolStripButtonPush", "sep2", "toolStripFileExplorer", "userShell");
    }

    [Test]
    public void Apply_should_not_place_a_shortcut_that_stayed_hidden()
    {
        // Hidden was their default, so a false setting was never written; treat one like absence.
        Apply(_standard, Settings(("pull_shortcut_fetchToolStripMenuItem", false))).Should().Equal(_standard.Select(i => i.Name));
    }

    [Test]
    public void Apply_should_place_the_shortcuts_even_when_the_pull_button_itself_was_hidden()
    {
        Apply(_standard, Settings(("toolStripButtonPull", false), ("pull_shortcut_mergeToolStripMenuItem", true)))
            .Should().Equal(
                "toolStripButtonLevelUp", "sep1", "toolStripButtonCommit",
                "+mergeToolStripMenuItem",
                "toolStripButtonPush", "sep2", "toolStripFileExplorer", "userShell");
    }

    [Test]
    public void Apply_should_drop_separators_left_with_nothing_to_separate()
    {
        Func<string, bool?> settings = Settings(
            ("toolStripButtonLevelUp", false),
            ("toolStripFileExplorer", false),
            ("userShell", false));

        Apply(_standard, settings).Should().Equal("toolStripButtonCommit", "toolStripButtonPull", "toolStripButtonPush");
    }

    [Test]
    public void Apply_should_keep_one_separator_of_a_run()
    {
        LegacyToolbarItem[] items = [Button("a"), Separator("s1"), Button("hidden"), Separator("s2"), Button("b")];

        Apply(items, Settings(("hidden", false))).Should().Equal("a", "s1", "b");
    }

    [Test]
    public void Apply_should_ignore_an_item_without_a_name()
    {
        // User script buttons have none; the old code stored nothing usable for them.
        LegacyToolbarItem[] scripts = [new(Name: "", Group: null, IsSeparator: false), Button("b")];

        LegacyToolbarVisibility.Apply(scripts, _ => false, out _).Select(slot => slot.SourceIndex).Should().Equal(0);
    }

    [Test]
    public void Apply_should_report_a_change_only_when_something_is_hidden_or_added()
    {
        // A present setting may change nothing on a toolbar; the migration must then leave no
        // layout behind, or a default install would stop following the designer's toolbars.
        Changes(_standard, Settings()).Should().BeFalse();
        Changes(_standard, Settings(("toolStripButtonCommit", true))).Should().BeFalse();
        Changes(_standard, Settings(("ToolBar_group:Text search", false))).Should().BeFalse();
        Changes(_standard, Settings(("pull_shortcut_fetchToolStripMenuItem", false))).Should().BeFalse();

        Changes(_standard, Settings(("toolStripButtonCommit", false))).Should().BeTrue();
        Changes(_standard, Settings(("pull_shortcut_fetchToolStripMenuItem", true))).Should().BeTrue();
    }

    [Test]
    public void HasAny_should_tell_whether_the_user_changed_anything_the_old_way()
    {
        LegacyToolbarVisibility.HasAny(Settings()).Should().BeFalse();
        LegacyToolbarVisibility.HasAny(Settings(("userShell", false))).Should().BeTrue();
        LegacyToolbarVisibility.HasAny(Settings(("pull_shortcut_fetchAllToolStripMenuItem", true))).Should().BeTrue();
        LegacyToolbarVisibility.HasAny(Settings(("ToolBar_group:Text search", false))).Should().BeTrue();
    }

    [Test]
    public void Purge_should_remove_every_setting_the_old_code_could_write()
    {
        List<string> removed = [];

        LegacyToolbarVisibility.Purge(removed.Add);

        removed.Should().BeEquivalentTo(LegacyToolbarVisibility.SettingNames);
        removed.Should().HaveCount(18)
            .And.Contain(Prefix + "toolStripButtonCommit")
            .And.Contain(Prefix + "pull_shortcut_rebaseToolStripMenuItem1")
            .And.Contain(Prefix + "ToolBar_group:Text filter")
            .And.OnlyContain(name => name.StartsWith(Prefix));
    }
}
