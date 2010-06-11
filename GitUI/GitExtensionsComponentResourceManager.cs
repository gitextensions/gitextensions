using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace GitUI
{
    public class GitExtensionsComponentResourceManager : ComponentResourceManager
    {
        public GitExtensionsComponentResourceManager()
            : base()
        {
        }
        public GitExtensionsComponentResourceManager(Type t)
            : base(t)
        {
        }

        public override string GetString(string name)
        {
            return "TEST";
        }

        public override string GetString(string name, System.Globalization.CultureInfo culture)
        {
            return "TEST2";
        }

        public override object GetObject(string name)
        {
            return base.GetObject(name);
        }

        public override object GetObject(string name, System.Globalization.CultureInfo culture)
        {
            return base.GetObject(name, culture);
        }


        public override System.Resources.ResourceSet GetResourceSet(System.Globalization.CultureInfo culture, bool createIfNotExists, bool tryParents)
        {
            return new GitExtensionsResourceSet(base.GetResourceSet(culture, createIfNotExists, tryParents));
        }


        public override void ApplyResources(object value, string objectName, System.Globalization.CultureInfo culture)
        {
            base.ApplyResources(value, objectName, culture);
        }
    }
}
