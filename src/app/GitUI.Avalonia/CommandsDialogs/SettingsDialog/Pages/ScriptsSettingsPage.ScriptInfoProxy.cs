using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using GitUI.ScriptsEngine;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

partial class ScriptsSettingsPage
{
    private sealed class ScriptInfoProxy
    {
        private const string ScriptCategory = "Script";
        private const string ScriptBehaviourCategory = "Script behaviour";
        private const string ScriptContextCategory = "Script context";

        [Browsable(false)]
        public int HotkeyCommandIdentifier { get; set; }

        [Category(ScriptCategory)]
        public string? Name { get; set; }

        [Category(ScriptCategory)]
        [DefaultValue(true)]
        public bool Enabled { get; set; }

        [Category(ScriptCategory)]
        public string? Command { get; set; }

        [Category(ScriptCategory)]
        public string? Arguments { get; set; }

        [Category(ScriptCategory)]
        [DisplayName("Execute on event")]
        [DefaultValue(ScriptEvent.None)]
        public ScriptEvent OnEvent { get; set; }

        [Category(ScriptCategory)]
        public string? Icon { get; set; }

        [Category(ScriptCategory)]
        [DisplayName("Icon or associated file path")]
        [Description("This can either be a path to an .ico file or to any other file in which case its \"associated icon\" is used.")]
        public string? IconFilePath { get; set; }

        [Category(ScriptBehaviourCategory)]
        [DisplayName("Ask confirmation")]
        [DefaultValue(false)]
        public bool AskConfirmation { get; set; }

        [Category(ScriptBehaviourCategory)]
        [DefaultValue(false)]
        [DisplayName("Run in background")]
        public bool RunInBackground { get; set; }

        [Category(ScriptBehaviourCategory)]
        [DefaultValue(false)]
        [DisplayName("Is PowerShell script")]
        public bool IsPowerShell { get; set; }

        [Category(ScriptContextCategory)]
        [DefaultValue(false)]
        [DisplayName("Show in RevisionGrid")]
        public bool AddToRevisionGridContextMenu { get; set; }

        public string GetIconImageKey()
            => File.Exists(IconFilePath) ? IconFilePath : Icon ?? string.Empty;

        [return: NotNullIfNotNull(nameof(script))]
        public static implicit operator ScriptInfoProxy?(ScriptInfo? script)
        {
            if (script is null)
            {
                return null;
            }

            return new ScriptInfoProxy
            {
                Enabled = script.Enabled,
                HotkeyCommandIdentifier = script.HotkeyCommandIdentifier,
                Name = script.Name,
                Command = script.Command,
                Arguments = script.Arguments,
                AskConfirmation = script.AskConfirmation,
                IsPowerShell = script.IsPowerShell,
                OnEvent = script.OnEvent,
                AddToRevisionGridContextMenu = script.AddToRevisionGridContextMenu,
                RunInBackground = script.RunInBackground,
                Icon = script.Icon,
                IconFilePath = script.IconFilePath,
            };
        }

        [return: NotNullIfNotNull(nameof(proxy))]
        public static implicit operator ScriptInfo?(ScriptInfoProxy? proxy)
        {
            if (proxy is null)
            {
                return null;
            }

            return new ScriptInfo
            {
                Enabled = proxy.Enabled,
                HotkeyCommandIdentifier = proxy.HotkeyCommandIdentifier,
                Name = proxy.Name,
                Command = proxy.Command,
                Arguments = proxy.Arguments,
                AskConfirmation = proxy.AskConfirmation,
                IsPowerShell = proxy.IsPowerShell,
                OnEvent = proxy.OnEvent,
                AddToRevisionGridContextMenu = proxy.AddToRevisionGridContextMenu,
                RunInBackground = proxy.RunInBackground,
                Icon = proxy.Icon,
                IconFilePath = proxy.IconFilePath,
            };
        }
    }
}
