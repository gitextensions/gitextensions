using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.CommandsDialogs.SettingsDialog.Plugins;
using GitUI.Compat;
using GitUI.Properties;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[NonParallelizable]
public sealed class PluginSettingsTests
{
    [SetUp]
    public void SetUp()
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [AvaloniaTest]
    public void Generic_renderer_should_load_and_save_builtin_setting_models()
    {
        TestSettingsSource settings = new() { SettingLevel = SettingLevel.Global };
        BoolSetting boolSetting = new("Enabled", defaultValue: false);
        StringSetting stringSetting = new("Command", "default");
        ChoiceSetting choiceSetting = new("Mode", ["one", "two"], "one");
        NumberSetting<int> numberSetting = new("Interval", 10);
        PasswordSetting passwordSetting = new("Token", "fallback");

        settings.SetValue("Enabled", null);
        settings.SetValue("Command", string.Empty);
        settings.SetValue("Mode", "two");
        settings.SetValue("Interval", "42");
        settings.SetValue("Token", "secret");

        PluginSettingBinding boolBinding = PluginSettingControlFactory.Create(boolSetting);
        PluginSettingBinding stringBinding = PluginSettingControlFactory.Create(stringSetting);
        PluginSettingBinding choiceBinding = PluginSettingControlFactory.Create(choiceSetting);
        PluginSettingBinding numberBinding = PluginSettingControlFactory.Create(numberSetting);
        PluginSettingBinding passwordBinding = PluginSettingControlFactory.Create(passwordSetting);
        foreach (PluginSettingBinding binding in new[]
                 {
                     boolBinding,
                     stringBinding,
                     choiceBinding,
                     numberBinding,
                     passwordBinding,
                 })
        {
            binding.Load(settings);
        }

        ((CheckBox)boolBinding.Control).IsChecked.Should().BeNull();
        ((TextBox)stringBinding.Control).Text.Should().Be("<empty string>");
        ((ComboBox)choiceBinding.Control).SelectedItem.Should().Be("two");
        ((NumericUpDown)numberBinding.Control).Value.Should().Be(42);
        ((TextBox)passwordBinding.Control).PasswordChar.Should().Be('\u25CF');

        ((CheckBox)boolBinding.Control).IsChecked = true;
        ((TextBox)stringBinding.Control).Text = " fetch --all ";
        ((ComboBox)choiceBinding.Control).SelectedItem = "one";
        ((NumericUpDown)numberBinding.Control).Value = 60;
        ((TextBox)passwordBinding.Control).Text = "updated";
        foreach (PluginSettingBinding binding in new[]
                 {
                     boolBinding,
                     stringBinding,
                     choiceBinding,
                     numberBinding,
                     passwordBinding,
                 })
        {
            binding.Save(settings);
        }

        settings.GetValue("Enabled").Should().Be("true");
        settings.GetValue("Command").Should().Be("fetch --all");
        settings.GetValue("Mode").Should().Be("one");
        settings.GetValue("Interval").Should().Be("60");
        settings.GetValue("Token").Should().Be("updated");
    }

    [AvaloniaTest]
    public void Generic_renderer_should_adapt_pseudo_and_custom_headless_controls()
    {
        PseudoSetting pseudoSetting = new("Warning text", height: 80);
        PluginSettingBinding pseudoBinding = PluginSettingControlFactory.Create(pseudoSetting);
        pseudoBinding.Load(new TestSettingsSource());

        TextBox pseudoText = pseudoBinding.Control.Should().BeOfType<TextBox>().Subject;
        pseudoText.Text.Should().Be("Warning text");
        pseudoText.IsReadOnly.Should().BeTrue();
        pseudoText.AcceptsReturn.Should().BeTrue();
        pseudoText.Height.Should().Be(80);

        TestCustomSetting customSetting = new();
        PluginSettingBinding customBinding = PluginSettingControlFactory.Create(customSetting);
        customBinding.Load(new TestSettingsSource());
        TextBox customText = customBinding.Control.Should().BeOfType<TextBox>().Subject;
        customText.Text.Should().Be("loaded");
        customText.Text = "saved";
        customBinding.Save(new TestSettingsSource());
        customSetting.Binding.SavedText.Should().Be("saved");
    }

    [AvaloniaTest]
    public void Plugin_icon_provider_should_materialize_an_embedded_portable_png()
    {
        Avalonia.Media.IImage? icon = PluginIconProvider.GetIcon(new EmbeddedIconPlugin());

        icon.Should().NotBeNull();
        icon!.Size.Width.Should().BeGreaterThan(0);
        icon.Size.Height.Should().BeGreaterThan(0);
    }

    [AvaloniaTest]
    public void FormSettings_should_register_plugin_root_and_settings_page()
    {
        IGitPlugin plugin = CreatePlugin();
        AddPlugin(plugin);
        try
        {
            FormSettings form = new();
            FormSettings.TestAccessor accessor = form.GetTestAccessor();
            accessor.InitializePages();

            accessor.SettingsTreeView.SettingsPages.OfType<PluginsSettingsGroup>().Should().ContainSingle();
            accessor.SettingsTreeView.SettingsPages.OfType<PluginRootIntroductionPage>().Should().ContainSingle();
            PluginSettingsPage page = accessor.SettingsTreeView.SettingsPages
                .OfType<PluginSettingsPage>()
                .Single(candidate => candidate.GetTitle() == plugin.Name);
            page.PageReference.Should().Be(new SettingsPageReferenceByType(plugin.GetType()));
            page.GetTestAccessor().Bindings.Should().HaveCount(2);

            form.GotoPage(page.PageReference);
            SettingsPageHeader header = accessor.CurrentPage.Should().BeOfType<SettingsPageHeader>().Subject;
            header.GetTestAccessor().Page.Should().BeSameAs(page);

            ITranslation translation = Substitute.For<ITranslation>();
            translation.TranslateItem(
                    nameof(SettingsPageBase),
                    "_stringSettingPlaceholder",
                    "Text",
                    Arg.Any<Func<string>>())
                .Returns("translated {0}");
            page.TranslateItems(translation);
            TextBox stringControl = page.GetTestAccessor().Bindings[1].Control.Should().BeOfType<TextBox>().Subject;
            stringControl.PlaceholderText.Should().Be("translated <empty string>");

            form.Show();
            try
            {
                Dispatcher.UIThread.RunJobs();
                form.CaptureRenderedFrame().Should().NotBeNull();
            }
            finally
            {
                form.GotoPage(GeneralSettingsPage.GetPageReference());
                form.Close();
            }
        }
        finally
        {
            RemovePlugin(plugin);
        }
    }

    [AvaloniaTest]
    public void FormBrowse_should_list_loaded_plugins_with_their_platform_icons()
    {
        IGitPlugin plugin = CreatePlugin();
        AddPlugin(plugin);
        try
        {
            using FormBrowse form = CreateBrowseForm();
            form.PopulatePluginMenuForTest();

            MenuItem pluginsMenu = form.FindControl<MenuItem>("pluginsToolStripMenuItem")
                ?? throw new InvalidOperationException("Plugins menu was not created.");
            pluginsMenu.Items.Should().NotContain(item => item == form.FindControl<MenuItem>("pluginsLoadingToolStripMenuItem"));
            MenuItem pluginItem = pluginsMenu.Items
                .OfType<MenuItem>()
                .Single(item => ReferenceEquals(item.Tag, plugin));
            pluginItem.Header.Should().Be(plugin.Name);
            ((Image)pluginItem.Icon!).Source.Should().BeSameAs(Images.Plugin);
            pluginItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            plugin.Received(1).Execute(Arg.Is<GitUIEventArgs>(args => ReferenceEquals(args.OwnerForm, form)));
        }
        finally
        {
            RemovePlugin(plugin);
        }
    }

    private static FormBrowse CreateBrowseForm()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.GetTempPath());
        module.IsValidGitWorkingDir().Returns(false);

        IAppTitleGenerator appTitleGenerator = Substitute.For<IAppTitleGenerator>();
        appTitleGenerator.Generate(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>()).Returns("Git Extensions");
        IHotkeySettingsLoader hotkeySettingsLoader = Substitute.For<IHotkeySettingsLoader>();
        hotkeySettingsLoader.LoadHotkeys(Arg.Any<string>()).Returns([]);

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(Substitute.For<ILockableNotifier>());
        commands.GetService(typeof(IAppTitleGenerator)).Returns(appTitleGenerator);
        commands.GetService(typeof(IHotkeySettingsLoader)).Returns(hotkeySettingsLoader);
        return new FormBrowse(commands);
    }

    private static IGitPlugin CreatePlugin()
    {
        IGitPlugin plugin = Substitute.For<IGitPlugin>();
        plugin.Id.Returns(new Guid("A930F097-34F3-47F8-8978-4319B49026CF"));
        plugin.Name.Returns("Portable test plugin");
        plugin.Description.Returns("Portable test plugin");
        plugin.HasSettings.Returns(true);
        plugin.Icon.Returns(new WinFormsShims.Image { PlatformImage = Images.Plugin });
        plugin.GetSettings().Returns(
        [
            new BoolSetting("Enabled", false),
            new StringSetting("Command", "git status"),
        ]);
        return plugin;
    }

    private static void AddPlugin(IGitPlugin plugin)
    {
        lock (PluginRegistry.Plugins)
        {
            PluginRegistry.Plugins.Add(plugin);
        }
    }

    private static void RemovePlugin(IGitPlugin plugin)
    {
        lock (PluginRegistry.Plugins)
        {
            PluginRegistry.Plugins.Remove(plugin);
        }
    }

    private sealed class TestSettingsSource : SettingsSource
    {
        private readonly Dictionary<string, string?> _values = [];

        public override string? GetValue(string name)
            => _values.GetValueOrDefault(name);

        public override void SetValue(string name, string? value)
            => _values[name] = value;
    }

    private sealed class TestCustomSetting : ISetting
    {
        internal TestCustomBinding Binding { get; } = new();

        public string Name => "Custom";

        public string Caption => "Custom caption";

        public ISettingControlBinding CreateControlBinding() => Binding;
    }

    private sealed class EmbeddedIconPlugin : GitPluginBase
    {
        internal EmbeddedIconPlugin()
            : base(hasSettings: false)
        {
            Id = new Guid("3CA36A4F-C7B5-44ED-853A-220020E6E0EA");
            Name = "Embedded icon plugin";
            Description = Name;
        }

        public override bool Execute(GitUIEventArgs args) => false;
    }

    private sealed class TestCustomBinding : ISettingControlBinding
    {
        private readonly WinFormsShims.TextBox _control = new();

        internal string? SavedText { get; private set; }

        public WinFormsShims.Control GetControl() => _control;

        public void LoadSetting(SettingsSource settings) => _control.Text = "loaded";

        public void SaveSetting(SettingsSource settings) => SavedText = _control.Text;

        public string Caption() => "Custom caption";

        public ISetting GetSetting() => throw new NotSupportedException();
    }
}
