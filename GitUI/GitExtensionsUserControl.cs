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
            this.Load += new EventHandler(GitExtensionsControl_Load);
        }

        void GitExtensionsControl_Load(object sender, EventArgs e)
        {
            ApplyResources();
        }

        protected void ApplyResources()
        {
            /*
            //resources.ApplyResources(this, Name);
            foreach (FieldInfo fieldInfo in this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                Component component = fieldInfo.GetValue(this) as Component;

                if (component != null)
                    resources.ApplyResources(component, fieldInfo.Name, new CultureInfo("nl-NL"));
            }
            */
            Translator translator = new Translator("Translations_nl");
            translator.TranslateControl(this);
        }
    }
}
