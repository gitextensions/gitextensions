using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms;
using GitUI.Design;
using GitUI.Script;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class ScriptsSettingsPage
    {
        [TypeConverter(typeof(PropertySorter))]
        private class ScriptInfoProxy
        {
            private const string ScriptCategory = "Script";
            private const string ScriptBehaviourCategory = "Script Behaviour";
            private const string ScriptContextCategory = "Script Context";

            // Needed for the ImageKeyConverter to work
            [Browsable(false)]
            public ImageList ImageList { get; set; }

            [Browsable(false)]
            public int HotkeyCommandIdentifier { get; set; }

            [Category(ScriptCategory)]
            [PropertyOrder(0)]
            public string Name { get; set; }

            [Category(ScriptCategory)]
            [DefaultValue(true)]
            [PropertyOrder(1)]
            public bool Enabled { get; set; }

            [Category(ScriptCategory)]
            [PropertyOrder(2)]
            [Editor(typeof(ExecutableFileNameEditor), typeof(UITypeEditor))]
            public string Command { get; set; }

            [Category(ScriptCategory)]
            [PropertyOrder(3)]
            [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
            public string Arguments { get; set; }

            [Category(ScriptCategory)]
            [PropertyOrder(4)]
            [DisplayName("Execute on event")]
            [DefaultValue(ScriptEvent.None)]
            public ScriptEvent OnEvent { get; set; }

            [TypeConverter(typeof(ImageKeyConverter))]
            [Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design", typeof(UITypeEditor))]
            [Category(ScriptCategory)]
            [PropertyOrder(4)]
            public string Icon { get; set; }

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

            internal void SetImages(ImageList images)
            {
                ImageList = images;
            }

            public static implicit operator ScriptInfoProxy(ScriptInfo script)
            {
                if (script == null)
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
                    Icon = script.Icon
                };
            }

            public static implicit operator ScriptInfo(ScriptInfoProxy proxy)
            {
                if (proxy == null)
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
                    Icon = proxy.Icon
                };
            }
        }
    }
}
