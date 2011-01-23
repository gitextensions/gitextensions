using System;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using ResourceManager.Translation;

namespace GitUI
{
    public class GitExtensionsControl : UserControl
    {
        public GitExtensionsControl()
        {
            Font = SystemFonts.MessageBoxFont;

            Load += GitExtensionsControl_Load;
        }

        private bool translated;

        private static bool CheckComponent(object value)
        {
            bool isComponentInDesignMode = false;
            var component = value as IComponent;
            if (component != null)
            {
                ISite site = component.Site;
                if ((site != null) && site.DesignMode)
                    isComponentInDesignMode = true;
            }

            return isComponentInDesignMode;
        }

        void GitExtensionsControl_Load(object sender, EventArgs e)
        {
            // find out if the value is a component and is currently in design mode
            bool isComponentInDesignMode = CheckComponent(this);

            if (!translated && !isComponentInDesignMode)
                throw new Exception("The control " + GetType().Name + " is not translated in the constructor. You need to call Translate() right after InitializeComponent().");
        }

        protected void Translate()
        {
            var translator = new Translator(GitCommands.Settings.Translation);
            translator.TranslateControl(this);
            translated = true;
        }
    }
}
