using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using GitUI.Properties;
using System.Drawing;
using ResourceManager;
using System.ComponentModel;
using System.Reflection;
using System.Globalization;
using ResourceManager.Translation;

namespace GitUI
{
    public class GitExtensionsControl : UserControl
    {
        public GitExtensionsControl()
        {
            this.Font = SystemFonts.MessageBoxFont;

            this.Load += new EventHandler(GitExtensionsControl_Load);
        }

        private bool translated = false;

        private static bool CheckComponent(object value)
        {
            bool isComponentInDesignMode = false;
            IComponent component = value as IComponent;
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
            Translator translator = new Translator(GitCommands.Settings.Translation);
            translator.TranslateControl(this);
            translated = true;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // GitExtensionsControl
            // 
            this.Name = "GitExtensionsControl";
            this.ResumeLayout(false);

        }
    }
}
