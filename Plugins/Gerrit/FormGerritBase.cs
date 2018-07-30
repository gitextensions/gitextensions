using System;
using GitUI;
using GitUIPluginInterfaces;

namespace Gerrit
{
    public class FormGerritBase : GitExtensionsForm
    {
        protected GerritSettings Settings { get; private set; }
        protected readonly IGitUICommands UICommands;
        protected IGitModule Module => UICommands.GitModule;

        private FormGerritBase()
            : this(null)
        {
        }

        protected FormGerritBase(IGitUICommands uiCommands)
            : base(true)
        {
            UICommands = uiCommands;
        }

        protected override void OnLoad(EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

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
