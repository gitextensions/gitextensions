using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI;
using GitUIPluginInterfaces;
using ResourceManager.Translation;

namespace Gerrit
{
    public class FormGerritBase : GitExtensionsForm
    {
        protected GerritSettings Settings { get; private set; }
        protected readonly IGitUICommands UICommands;
        protected IGitModule Module { get { return UICommands.GitModule; } }

        private FormGerritBase()
            : this(null)
        { }

        protected FormGerritBase(IGitUICommands agitUiCommands)
            : base(true)
        {
            UICommands = agitUiCommands;
        }

        protected override void OnLoad(EventArgs e)
        {
            if (DesignMode)
                return;

            Settings = GerritSettings.Load(Module);

            if (Settings == null)
            {
                Dispose();
                return;
            }

            base.OnLoad(e);
        }
    }
}
